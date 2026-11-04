namespace Rogue1980.DataLayer.Dtos;

public class ItemDto
{
    public int Type { get; set; } // Сохраняем Enum как число
    public string SubType { get; set; } = string.Empty;
    public CoordDto Position { get; set; } = new();
    public int HealthValue { get; set; }
    public int MaxHealthValue { get; set; }
    public int DexterityValue { get; set; }
    public int StrengthValue { get; set; }
    public int Cost { get; set; }
}