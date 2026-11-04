using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Interfaces;

public interface IGameCommandHandler
{
    // Управление сессией
    void StartNewGame();
    void ContinueGame();
    
    // Движение (WASD)
    // Принимает вектор направления (например, Coord.Up)
    void MovePlayer(Coord direction);
    
    // Использование предметов из инвентаря
    // actionType: h (оружие), j (еда), k (эликсир), e (свиток)
    // itemIndex: индекс предмета в рюкзаке (1-9, или 0 для снятия оружия)
    void UseItem(char actionType, int itemIndex);
    
    // Переключение режима 3D/2D (доп. задание)
    void ToggleRenderMode();
}