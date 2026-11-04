using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Models;

public class Room
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    
    // Центр комнаты для генерации коридоров
    public Coord Center => new(X + Width / 2, Y + Height / 2);

    public bool Contains(Coord pos)
    {
        return pos.X >= X && pos.X < X + Width && 
               pos.Y >= Y && pos.Y < Y + Height;
    }
}