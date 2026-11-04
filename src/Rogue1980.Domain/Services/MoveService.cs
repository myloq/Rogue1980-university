using Rogue1980.Domain.Models;
using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Services;

public class MoveService
{
    // Возвращает результат хода, чтобы презентация знала, что произошло
    public MoveResult TryMove(Character player, Coord direction, Level level)
    {
        Coord targetPos = player.Position + direction;

        // 1. Проверка на стены и границы карты
        if (!level.IsWalkable(targetPos.X, targetPos.Y))
        {
            return MoveResult.BlockedByWall;
        }

        // 2. Проверка на врагов
        var enemy = level.Enemies.FirstOrDefault(e => e.Position == targetPos);
        if (enemy != null)
        {
            // Здесь сигнализируем, что нужно атаковать
            return MoveResult.EnemyEncountered;
        }

        // 3. Проверка на выход с уровня
        if (targetPos == level.ExitPosition)
        {
            return MoveResult.ExitReached;
        }

        // 4. Если все чисто — двигаемся
        player.Position = targetPos;
        
        // Примечание: подбор предметов выполняется в GameEngine после успешного хода
        var item = level.Items.FirstOrDefault(i => i.Position == targetPos); 

        return MoveResult.Success;
    }
}

public enum MoveResult
{
    Success,
    BlockedByWall,
    EnemyEncountered,
    ExitReached
}