using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Models;

public class Character
{
    public Coord Position { get; set; }
    
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Dexterity { get; set; }
    public int Strength { get; set; }
    
    public Item? EquippedWeapon { get; set; }
    public Backpack Backpack { get; } = new();
    
    // Флаг для сна (Змей-маг усыпляет на 1 ход)
    public bool IsAsleep { get; set; }
    
    // Временные модификаторы от эликсиров
    public int TemporaryStrengthBonus { get; set; }
    public int TemporaryDexterityBonus { get; set; }
}