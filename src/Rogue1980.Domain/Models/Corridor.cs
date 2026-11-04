using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Models;

public class Corridor
{
    // Координаты, которые занимает коридор
    public List<Coord> Tiles { get; } = [];
    
    // Ссылки на комнаты, которые он соединяет
    public int FromRoomIndex { get; init; }
    public int ToRoomIndex { get; init; }
}