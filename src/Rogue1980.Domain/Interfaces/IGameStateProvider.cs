using Rogue1980.Domain.Models;
using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Interfaces;

public interface IGameStateProvider
{
    // Получить текущую сессию и уровень (для рендера карты)
    GameSession GetSession();
    Level GetCurrentLevel();
    
    // Получить игрока (для рендера персонажа и статуса)
    Character GetPlayer();
    
    // Получить видимые клетки (для тумана войны)
    // Возвращает координаты, которые сейчас видит игрок
    HashSet<Coord> GetVisibleTiles();
    
    // Получить врагов и предметы для отрисовки
    IEnumerable<Enemy> GetVisibleEnemies();
    IEnumerable<Item> GetVisibleItems();
}