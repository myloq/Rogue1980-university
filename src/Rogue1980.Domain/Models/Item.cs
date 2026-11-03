using Rogue1980.Domain.Models.Enums;
using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Models;

public class Item
{
    public ItemType Type { get; init; }
    public string SubType { get; init; } = string.Empty; // Например: "Меч", "Яблоко", "Эликсир силы"
    public Coord Position { get; set; }
    
    // Модификаторы характеристик
    public int HealthValue { get; init; }      // Для еды
    public int MaxHealthValue { get; init; }   // Для свитков и эликсиров
    public int DexterityValue { get; init; }   // Для свитков, эликсиров
    public int StrengthValue { get; init; }    // Для свитков, эликсиров, оружия
    
    // Для сокровищ
    public int Cost { get; init; } 
}