using Rogue1980.Domain.Models.Enums;

namespace Rogue1980.Domain.Models;

public class Backpack
{
    public const int MaxItemsPerType = 9;
    
    public List<Item> Weapons { get; } = [];
    public List<Item> Foods { get; } = [];
    public List<Item> Elixirs { get; } = [];
    public List<Item> Scrolls { get; } = [];
    
    // Сокровища хранятся в одной ячейке (просто сумма)
    public int TreasureCount { get; private set; }

    public void AddTreasure(int amount) => TreasureCount += amount;

    public bool CanAdd(ItemType type)
    {
        return type switch
        {
            ItemType.Weapon => Weapons.Count < MaxItemsPerType,
            ItemType.Food => Foods.Count < MaxItemsPerType,
            ItemType.Elixir => Elixirs.Count < MaxItemsPerType,
            ItemType.Scroll => Scrolls.Count < MaxItemsPerType,
            ItemType.Treasure => true, 
            _ => false
        };
    }
}