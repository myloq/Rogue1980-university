namespace Rogue1980.DataLayer.Dtos;

public class GameSessionDto
{
    public CharacterDto Player { get; set; } = new();
    public LevelDto CurrentLevel { get; set; } = new();
    public RunStatsDto Stats { get; set; } = new();
    public bool IsGameOver { get; set; }
}