namespace Rogue1980.DataLayer.Dtos;

public class EnemyDto
{
    public int Type { get; set; }
    public CoordDto Position { get; set; } = new();
    public int Health { get; set; }
    public int Dexterity { get; set; }
    public int Strength { get; set; }
    public int Hostility { get; set; }
    public bool IsInvisible { get; set; }
    public bool IsResting { get; set; }
    public bool HasBeenHit { get; set; }
}