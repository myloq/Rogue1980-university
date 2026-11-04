using Rogue1980.Domain.Models;
using Rogue1980.Domain.Models.Enums;
using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Services;

public class EnemyAiService
{
    private readonly Random _random = new();
    private readonly CombatService _combatService = new(); // Для атак

    // Главный метод, который вызывается после хода игрока
    public void ProcessEnemyTurns(Level level, Character player)
    {
        foreach (var enemy in level.Enemies.ToList())
        {
            if (enemy.Health <= 0 || player.Health <= 0) continue;

            int distance = CalculateDistance(enemy.Position, player.Position);
            bool isChasing = distance <= enemy.Hostility;

            // Если враг рядом — атакует
            if (distance == 1) // Соседняя клетка (для обычных)
            {
                _combatService.EnemyAttackPlayer(enemy, player);
                continue;
            }

            // Логика передвижения
            if (isChasing)
            {
                ChasePlayer(enemy, player, level);
            }
            else
            {
                // Патрулирование (случайное движение по комнате)
                Patrol(enemy, player, level);
            }
        }
    }

    // --- Логика преследования (BFS) ---
    private void ChasePlayer(Enemy enemy, Character player, Level level)
    {
        int speed = enemy.Type == EnemyType.Ogre ? 2 : 1; // Огр ходит на 2 клетки

        for (int step = 0; step < speed; step++)
        {
            // Если после первого шага огр оказался рядом с игроком — он атакует и останавливается
            if (CalculateDistance(enemy.Position, player.Position) == 1)
            {
                _combatService.EnemyAttackPlayer(enemy, player);
                break;
            }

            var nextStep = FindShortestPathStep(enemy.Position, player.Position, level, player);
            if (nextStep != enemy.Position)
            {
                enemy.Position = nextStep;
            }
        }
    }

    // --- Простой патруль (если игрок далеко) ---
    private void Patrol(Enemy enemy, Character player, Level level)
    {
        // Для привидения — телепортация
        if (enemy.Type == EnemyType.Ghost)
        {
            TeleportGhost(enemy, player, level);
            return;
        }

        var directions = new[] { Coord.Up, Coord.Down, Coord.Left, Coord.Right };
        var randomDir = directions[_random.Next(directions.Length)];
        var target = enemy.Position + randomDir;

        // Игрок считается занятой клеткой - враг не может на него наступить
        if (level.IsWalkable(target.X, target.Y) && !IsOccupied(target, level, player))
        {
            enemy.Position = target;
        }
    }

    // --- Спецспособность привидения ---
    private void TeleportGhost(Enemy enemy, Character player, Level level)
    {
        // Ищем комнату, в которой находится привидение
        var room = level.Rooms.FirstOrDefault(r => r.Contains(enemy.Position));

        // Если привидение оказалось вне комнат (в коридоре) - телепортируем в случайную комнату
        if (room == null)
        {
            if (level.Rooms.Count == 0) return;
            room = level.Rooms[_random.Next(level.Rooms.Count)];
        }

        // Телепортируем в случайную точку внутри комнаты
        int newX = _random.Next(room.X, room.X + room.Width);
        int newY = _random.Next(room.Y, room.Y + room.Height);
        var newPos = new Coord(newX, newY);

        // Игрок считается занятой клеткой - привидение не может телепортироваться на него
        if (level.IsWalkable(newX, newY) && !IsOccupied(newPos, level, player))
        {
            enemy.Position = newPos;
        }
    }

    // --- Алгоритм поиска пути (BFS) ---
    // Возвращает координату следующего шага к цели
    private Coord FindShortestPathStep(Coord start, Coord target, Level level, Character player)
    {
        var queue = new Queue<Coord>();
        var cameFrom = new Dictionary<Coord, Coord>();
        
        queue.Enqueue(start);
        cameFrom[start] = start;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == target) break;

            foreach (var dir in Coord.Offsets4)
            {
                var next = current + dir;
                
                if (level.IsWalkable(next.X, next.Y) 
                    && !cameFrom.ContainsKey(next) 
                    && (!IsOccupied(next, level, player) || next == target)) // Цель (игрок) проходима для поиска пути
                {
                    queue.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        // Восстанавливаем путь обратно до старта и берем первый шаг
        var step = target;
        if (!cameFrom.ContainsKey(step)) return start; // Пути нет

        while (cameFrom[step] != start)
        {
            step = cameFrom[step];
        }

        return step;
    }

    // Вспомогательные методы
    private int CalculateDistance(Coord a, Coord b)
    {
        // Манхэттенское расстояние (для 4-х направлений)
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    private bool IsOccupied(Coord pos, Level level, Character player)
    {
        return level.Enemies.Any(e => e.Position == pos) || pos == player.Position;
    }
}