using Rogue1980.Domain.Models;
using Rogue1980.Domain.Models.Enums;
using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Presentation.Services;

public class FogOfWarService
{
    // Храним то, что игрок уже видел за всю игру на этом уровне
    public HashSet<Coord> ExploredTiles { get; } = new();

    // Вычисляет видимые клетки в текущий момент
    public HashSet<Coord> CalculateVisibility(Character player, Level level)
    {
        var visibleTiles = new HashSet<Coord>();
        int radius = 8; // Радиус обзора

        for (int x = player.Position.X - radius; x <= player.Position.X + radius; x++)
        {
            for (int y = player.Position.Y - radius; y <= player.Position.Y + radius; y++)
            {
                var target = new Coord(x, y);
                
                if (x < 0 || y < 0 || x >= level.Map.GetLength(0) || y >= level.Map.GetLength(1))
                    continue;

                if (HasLineOfSight(player.Position, target, level))
                {
                    visibleTiles.Add(target);
                    ExploredTiles.Add(target);
                }
            }
        }

        return visibleTiles;
    }

    private bool HasLineOfSight(Coord start, Coord end, Level level)
    {
        int x0 = start.X, y0 = start.Y;
        int x1 = end.X, y1 = end.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 == x1 && y0 == y1) return true;

            if (!(x0 == start.X && y0 == start.Y) && level.Map[x0, y0] == TileType.Wall)
                return false;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }
    
    // Метод для сброса тумана при переходе на новый уровень
    public void Reset()
    {
        ExploredTiles.Clear();
    }
}