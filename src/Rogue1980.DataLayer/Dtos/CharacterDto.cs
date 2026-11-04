namespace Rogue1980.DataLayer.Dtos;

public class CharacterDto
{
    public CoordDto Position { get; set; } = new();
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Dexterity { get; set; }
    public int Strength { get; set; }
    public ItemDto? EquippedWeapon { get; set; }
    
    // Рюкзак разворачиваем в списки
    public List<ItemDto> Weapons { get; set; } = [];
    public List<ItemDto> Foods { get; set; } = [];
    public List<ItemDto> Elixirs { get; set; } = [];
    public List<ItemDto> Scrolls { get; set; } = [];
    public int TreasureCount { get; set; }
    public List<int> Keys { get; set; } = []; // KeyColor как числа
    
    public bool IsAsleep { get; set; }
    public int TemporaryStrengthBonus { get; set; }
    public int TemporaryDexterityBonus { get; set; }
}