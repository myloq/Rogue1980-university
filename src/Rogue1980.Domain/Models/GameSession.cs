namespace Rogue1980.Domain.Models;

public class GameSession
{
    public Character Player { get; init; } = new();
    public Level CurrentLevel { get; set; } = null!;
    
    // Текущая статистика игрока
    public RunStats Stats { get; init; } = new();
    
    // Флаг окончания игры (смерть или победа)
    public bool IsGameOver { get; set; }
    
    // Нужно ли сохранить игру (например, после перехода на новый уровень)
    public bool NeedsSaving { get; set; }
}