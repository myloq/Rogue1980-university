using Rogue1980.Domain.Models;

namespace Rogue1980.Domain.Interfaces;

public interface ILeaderboardProvider
{
    // Возвращает отсортированный по сокровищам список прохождений
    IEnumerable<RunStats> GetLeaderboard();
    
    // Добавляет результат текущей (или завершенной) игры в таблицу
    void SaveResult(RunStats stats);
}