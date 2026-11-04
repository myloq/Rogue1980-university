namespace Rogue1980.Domain.Models;

public class RunStats
{
    public int LevelReached { get; set; }
    public int TreasuresCollected { get; set; }
    public int EnemiesKilled { get; set; }
    public int FoodEaten { get; set; }
    public int ElixirsDrunk { get; set; }
    public int ScrollsRead { get; set; }
    public int HitsDealt { get; set; }
    public int HitsMissed { get; set; }
    public int StepsTaken { get; set; }
    public bool IsVictory { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
}