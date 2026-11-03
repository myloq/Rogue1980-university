using Rogue1980.Domain.Models.Enums;
using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Models;

public class Enemy
{
    public EnemyType Type { get; init; }
    public Coord Position { get; set; }
    
    public int Health { get; set; }
    public int Dexterity { get; set; }
    public int Strength { get; set; }
    public int Hostility { get; set; } // Радиус агрессии
    
    // Специфичные состояния 
    public bool IsInvisible { get; set; } // Для привидения
    public bool IsResting { get; set; }   // Для огра (пропуск хода)
    public bool HasBeenHit { get; set; } = false; // Для вампира
}