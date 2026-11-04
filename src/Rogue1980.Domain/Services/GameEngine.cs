using Rogue1980.Domain.Generation;
using Rogue1980.Domain.Interfaces;
using Rogue1980.Domain.Models;
using Rogue1980.Domain.Models.Enums;
using Rogue1980.Domain.Services;
using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Gameplay;

public class GameEngine : IGameCommandHandler
{
    private readonly GameSession _session;
    private readonly MoveService _moveService;
    private readonly CombatService _combatService;
    private readonly EnemyAiService _enemyAiService;
    private readonly InventoryService _inventoryService;
    private readonly LevelGenerator  _levelGenerator;
    private readonly IGameStateRepository _repository;
    private readonly Queue<string> _pendingMessages = new(); // Очередь сообщений для игрового лога

    public GameEngine(GameSession session, 
                      MoveService moveService, 
                      CombatService combatService, 
                      EnemyAiService enemyAiService,
                      InventoryService inventoryService,
                      LevelGenerator levelGenerator,
                      IGameStateRepository repository)
    {
        _session = session;
        _moveService = moveService;
        _combatService = combatService;
        _enemyAiService = enemyAiService;
        _inventoryService = inventoryService;
        _levelGenerator =  levelGenerator;
        _repository = repository;
    }

    public void MovePlayer(Coord direction)
    {
        if (_session.IsGameOver) return;

        // 1. Проверка на сон (Змей-маг)
        if (_session.Player.IsAsleep)
        {
            _session.Player.IsAsleep = false; // Просыпаемся
            AddMessage("You are fast asleep - you skip your turn");
            _enemyAiService.ProcessEnemyTurns(_session.CurrentLevel, _session.Player);
            CheckGameOver();
            return; // Пропускаем ход
        }

        // 2. Пытаемся сдвинуть игрока
        var result = _moveService.TryMove(_session.Player, direction, _session.CurrentLevel);

        switch (result)
        {
            case MoveResult.Success:
                _session.Stats.StepsTaken++;
                
                // --- ЛОГИКА ПОДБОРА ПРЕДМЕТОВ ---
                var itemOnFloor = _session.CurrentLevel.Items.FirstOrDefault(i => i.Position == _session.Player.Position);
                
                if (itemOnFloor != null)
                {
                    if (_session.Player.Backpack.CanAdd(itemOnFloor.Type))
                    {
                        // Добавляем в рюкзак в зависимости от типа
                        switch (itemOnFloor.Type)
                        {
                            case ItemType.Weapon:
                                _session.Player.Backpack.Weapons.Add(itemOnFloor);
                                break;
                            case ItemType.Food:
                                _session.Player.Backpack.Foods.Add(itemOnFloor);
                                break;
                            case ItemType.Elixir:
                                _session.Player.Backpack.Elixirs.Add(itemOnFloor);
                                break;
                            case ItemType.Scroll:
                                _session.Player.Backpack.Scrolls.Add(itemOnFloor);
                                break;
                            case ItemType.Treasure:
                                _session.Player.Backpack.AddTreasure(itemOnFloor.Cost);
                                break;
                        }
                        
                        // Убираем предмет с пола
                        _session.CurrentLevel.Items.Remove(itemOnFloor);
                        AddMessage($"You picked up: {DescribeItem(itemOnFloor)}");
                    }
                    else
                    {
                        AddMessage($"Backpack is full - {itemOnFloor.Type} left on the floor");
                    }
                }

                _enemyAiService.ProcessEnemyTurns(_session.CurrentLevel, _session.Player);
                break;

            case MoveResult.EnemyEncountered:
                // Ищем врага на целевой клетке
                var targetPos = _session.Player.Position + direction;
                var enemy = _session.CurrentLevel.Enemies.FirstOrDefault(e => e.Position == targetPos);
                
                if (enemy != null)
                {
                    int enemyHealthBefore = enemy.Health;
                    bool isHit = _combatService.PlayerAttackEnemy(_session.Player, enemy);
                    
                    if (isHit)
                    {
                        _session.Stats.HitsDealt++;
                        if (enemy.Health <= 0)
                        {
                            // Лут считаем по здоровью врага до получения урона
                            int loot = CalculateLoot(enemy.Hostility, enemy.Strength, enemy.Dexterity, enemyHealthBefore);
                            _session.CurrentLevel.Enemies.Remove(enemy);
                            _session.Stats.EnemiesKilled++;
                            _session.Stats.TreasuresCollected += loot;
                            _session.Player.Backpack.AddTreasure(loot);
                            AddMessage($"You killed the {enemy.Type}! +{loot} gold");
                        }
                    }
                    else
                    {
                        _session.Stats.HitsMissed++;
                    }
                    
                    // После удара враги делают свой ход
                    _enemyAiService.ProcessEnemyTurns(_session.CurrentLevel, _session.Player);
                }
                break;

            case MoveResult.ExitReached:
                if (_session.CurrentLevel.Index >= 21)
                {
                    _session.IsGameOver = true;
                    _session.Stats.IsVictory = true;
                    AddMessage("VICTORY! You have completed all 21 levels!");
                }
                else
                {
                    // Генерируем новый уровень
                    var nextLevel = _levelGenerator.Generate(_session.CurrentLevel.Index + 1);
                    _session.CurrentLevel = nextLevel;
                    
                    // Перемещаем игрока в центр стартовой комнаты
                    var startRoom = nextLevel.Rooms[nextLevel.StartRoomIndex];
                    _session.Player.Position = startRoom.Center;
                    
                    AddMessage($"You descended to level {nextLevel.Index}");

                    // Обновляем статистику и сбрасываем временные баффы
                    _session.Stats.LevelReached = nextLevel.Index;
                    _session.Player.TemporaryStrengthBonus = 0;
                    _session.Player.TemporaryDexterityBonus = 0;
                }
                // Сохраняем только если игра продолжается (после победы сейв не нужен)
                if (!_session.IsGameOver)
                {
                    _repository.SaveSession(_session);
                }
                break;

            case MoveResult.BlockedByWall:
                // Ничего не делаем, ход не тратится (или можно тратить, по желанию)
                break;
        }

        CheckGameOver();
    }

    public void UseItem(char actionType, int itemIndex)
    {
        if (_session.IsGameOver) return;

        bool itemUsed = false;

        switch (char.ToLower(actionType))
        {
            case 'h': // Оружие
                // Проверяем, есть ли смысл использовать
                if (itemIndex == 0 || itemIndex <= _session.Player.Backpack.Weapons.Count)
                {
                    _inventoryService.UseWeapon(_session.Player, _session.CurrentLevel, itemIndex);
                    itemUsed = true;
                }
                break;

            case 'j': // Еда
                if (itemIndex > 0 && itemIndex <= _session.Player.Backpack.Foods.Count)
                {
                    _inventoryService.UseFood(_session.Player, itemIndex);
                    _session.Stats.FoodEaten++;
                    itemUsed = true;
                }
                break;

            case 'k': // Эликсир
                if (itemIndex > 0 && itemIndex <= _session.Player.Backpack.Elixirs.Count)
                {
                    _inventoryService.UseElixir(_session.Player, itemIndex);
                    _session.Stats.ElixirsDrunk++;
                    itemUsed = true;
                }
                break;

            case 'e': // Свиток
                if (itemIndex > 0 && itemIndex <= _session.Player.Backpack.Scrolls.Count)
                {
                    _inventoryService.UseScroll(_session.Player, itemIndex);
                    _session.Stats.ScrollsRead++;
                    itemUsed = true;
                }
                break;
        }

        // Если предмет был успешно использован, враги делают ход
        if (itemUsed)
        {
            _enemyAiService.ProcessEnemyTurns(_session.CurrentLevel, _session.Player);
            CheckGameOver();
        }
    }

    private void CheckGameOver()
    {
        if (_session.Player.Health <= 0)
        {
            _session.IsGameOver = true;
            _session.Stats.IsVictory = false;
        }
    }

    // Формула из ТЗ: зависит от враждебности, силы, ловкости, здоровья
    private int CalculateLoot(int hostility, int strength, int dexterity, int health)
    {
        return (hostility + strength + dexterity + health) / 2;
    }

    // Краткое описание предмета для игрового лога
    private static string DescribeItem(Item item)
    {
        if (item.Type == ItemType.Treasure)
            return $"Treasure (+{item.Cost} gold)";

        var parts = new List<string>();
        if (item.HealthValue != 0) parts.Add($"HP+{item.HealthValue}");
        if (item.MaxHealthValue != 0) parts.Add($"MaxHP+{item.MaxHealthValue}");
        if (item.StrengthValue != 0) parts.Add($"STR+{item.StrengthValue}");
        if (item.DexterityValue != 0) parts.Add($"DEX+{item.DexterityValue}");

        var modifiers = parts.Count > 0 ? $" ({string.Join(", ", parts)})" : string.Empty;
        return $"{item.Type}{modifiers}";
    }

    // Забирает накопленные сообщения для вывода в интерфейс
    public IReadOnlyList<string> DrainMessages()
    {
        var messages = new List<string>(_pendingMessages.Count);
        while (_pendingMessages.Count > 0)
            messages.Add(_pendingMessages.Dequeue());

        return messages;
    }

    private void AddMessage(string message)
    {
        _pendingMessages.Enqueue(message);
    }

    // Заглушки для интерфейса (допишешь позже)
    public void StartNewGame() { }
    public void ContinueGame() { }
    public void ToggleRenderMode() { }
}