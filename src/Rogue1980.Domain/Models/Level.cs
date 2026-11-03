using Rogue1980.Domain.ValueObjects;
using Rogue1980.Domain.Models.Enums;

namespace Rogue1980.Domain.Models;

public class Level
{
    public int Index { get; init; } // Глубина (от 1 до 21)
    
    // Карта уровня (2D массив клеток)
    public TileType[,] Map { get; init; } = new TileType[0,0];
    
    public List<Room> Rooms { get; set; } = [];
    public List<Corridor> Corridors { get; set; } = [];
    
    public int StartRoomIndex { get; init; }
    public int EndRoomIndex { get; init; }
    
    public Coord ExitPosition { get; init; }
    
    // Сущности на уровне
    public List<Enemy> Enemies { get; } = [];
    public List<Item> Items { get; } = [];
    
    // Метод-помощник для проверки, можно ли пройти в клетку
    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Map.GetLength(0) || y >= Map.GetLength(1))
            return false;
            
        return Map[x, y] != TileType.Wall; 
    }
}