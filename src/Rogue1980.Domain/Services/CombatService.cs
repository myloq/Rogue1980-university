using Rogue1980.Domain.Models;
using Rogue1980.Domain.Models.Enums;

namespace Rogue1980.Domain.Services;

public class CombatService
{
    private readonly Random _random = new();

    public int CalculateDamage(int baseStrength, int weaponStrength, bool isEnemy = false)
    {
        int baseDamage = baseStrength + weaponStrength;
        int finalDamage = (int)(baseDamage * (_random.NextDouble() * 0.4 + 0.8)); // 80% - 120%
        
        // Враги наносят в 2 раза больше урона, чтобы игра не казалась слишком легкой
        if (isEnemy) finalDamage *= 2; 
        
        return Math.Max(1, finalDamage); 
    }
    
    // Возвращает true, если атака попала
    public bool CheckHit(int attackerDexterity, int defenderDexterity)
    {
        // шанс попадания 
        int chance = 60 + (attackerDexterity - defenderDexterity) * 3;
        
        // Ограничиваем шанс
        chance = Math.Clamp(chance, 20, 90); 
        
        return _random.Next(1, 101) <= chance;
    }
    
    // Атака игрока по врагу (с учетом вампира)
    public bool PlayerAttackEnemy(Character player, Enemy enemy)
    {
        // Механика вампира: первый удар всегда промах
        if (enemy.Type == EnemyType.Vampire && !enemy.HasBeenHit)
        {
            enemy.HasBeenHit = true;
            return false; // Промах
        }

        bool isHit = CheckHit(player.Dexterity, enemy.Dexterity);
        if (isHit)
        {
            int damage = CalculateDamage(player.Strength, player.EquippedWeapon?.StrengthValue ?? 0);
            enemy.Health -= damage;
        }
        
        return isHit;
    }

    // Атака врага по игроку
    public void EnemyAttackPlayer(Enemy enemy, Character player)
    {
        // Огр отдыхает один ход после атаки; флаг сбрасывается при пропуске
        if (enemy.IsResting)
        {
            enemy.IsResting = false;
            return;
        }

        bool isHit = CheckHit(enemy.Dexterity, player.Dexterity);
        
        if (isHit)
        {
            int damage = CalculateDamage(enemy.Strength, 0, true);
            player.Health -= damage;

            // Спецспособности врагов
            switch (enemy.Type)
            {
                case EnemyType.Vampire:
                    // Отнимает максимальное здоровье
                    player.MaxHealth = Math.Max(1, player.MaxHealth - 1);
                    if (player.Health > player.MaxHealth) 
                        player.Health = player.MaxHealth;
                    break;

                case EnemyType.SnakeMage:
                    // Вероятность усыпить
                    if (_random.Next(1, 101) <= 30)
                        player.IsAsleep = true;
                    break;

                case EnemyType.Ogre:
                    // После атаки огр отдыхает один ход
                    enemy.IsResting = true;
                    break;
            }
        }
    }

    // Вспомогательный метод для расчета урона
    private int CalculateDamage(int baseStrength, int weaponStrength)
    {
        int baseDamage = baseStrength + weaponStrength;
        int finalDamage = (int)(baseDamage * (_random.NextDouble() * 0.4 + 0.8)); // 80% - 120%
        return Math.Max(1, finalDamage);
    }
}