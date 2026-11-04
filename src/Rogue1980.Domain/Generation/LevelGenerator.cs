using Rogue1980.Domain.Models;
using Rogue1980.Domain.Models.Enums;
using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Generation;

public class LevelGenerator
{
    private readonly Random _random = new();
    
    // Размеры всего подземелья
    private const int MapWidth = 80;
    private const int MapHeight = 30;

    public Level Generate(int levelIndex)
    {
        var map = new TileType[MapWidth, MapHeight];
        
        // 1. Изначально всё заполнено стенами
        for (int x = 0; x < MapWidth; x++)
            for (int y = 0; y < MapHeight; y++)
                map[x, y] = TileType.Wall;

        var rooms = new List<Room>();
        var corridors = new List<Corridor>();

        // 2. Генерируем 9 комнат (сетка 3x3)
        int sectionWidth = MapWidth / 3;
        int sectionHeight = MapHeight / 3;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                var room = GenerateRoomInSection(col, row, sectionWidth, sectionHeight);
                rooms.Add(room);
            }
        }

        // 3. Прорезаем комнаты в карте (заполняем полом)
        foreach (var room in rooms)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
            {
                for (int y = room.Y; y < room.Y + room.Height; y++)
                {
                    map[x, y] = TileType.Floor;
                }
            }
        }

        // 4. Соединяем комнаты коридорами
        for (int i = 1; i < rooms.Count; i++)
        {
            // Выбираем случайную комнату из уже сгенерированных (от 0 до i-1)
            int targetRoomIndex = _random.Next(0, i);
            var corridor = CreateLShapedCorridor(rooms[i], rooms[targetRoomIndex], map);
            corridors.Add(corridor);
        }

        // 5. Настраиваем стартовую и конечную комнаты
        // Стартовая - первая (верхний левый угол), конечная - последняя (нижний правый)
        int startRoomIndex = 0;
        int endRoomIndex = rooms.Count - 1;
        
        var endRoom = rooms[endRoomIndex];
        var exitPos = new Coord(endRoom.Center.X, endRoom.Center.Y);
        map[exitPos.X, exitPos.Y] = TileType.Exit;

        var level = new Level
        {
            Index = levelIndex,
            Map = map,
            Rooms = rooms,
            Corridors = corridors,
            StartRoomIndex = startRoomIndex,
            EndRoomIndex = endRoomIndex,
            ExitPosition = exitPos
        };

        // 6. Наполняем уровень врагами и предметами
        PopulateLevel(level, levelIndex);

        return level;
    }

    // Генерация одной комнаты внутри выделенной секции
    private Room GenerateRoomInSection(int col, int row, int sectionWidth, int sectionHeight)
    {
        // Случайный размер комнаты
        int width = _random.Next(6, Math.Min(14, sectionWidth - 2));
        int height = _random.Next(4, Math.Min(8, sectionHeight - 2));

        // Случайная позиция внутри секции
        int x = col * sectionWidth + _random.Next(1, Math.Max(2, sectionWidth - width - 1));
        int y = row * sectionHeight + _random.Next(1, Math.Max(2, sectionHeight - height - 1));

        return new Room { X = x, Y = y, Width = width, Height = height };
    }

    // Г-образный коридор
    private Corridor CreateLShapedCorridor(Room roomA, Room roomB, TileType[,] map)
    {
        var corridor = new Corridor();
        var start = roomA.Center;
        var end = roomB.Center;

        // Идем сначала по горизонтали, затем по вертикали (или наоборот)
        bool horizontalFirst = _random.Next(2) == 0;

        if (horizontalFirst)
        {
            CarveHorizontal(start.X, end.X, start.Y, map, corridor);
            CarveVertical(start.Y, end.Y, end.X, map, corridor);
        }
        else
        {
            CarveVertical(start.Y, end.Y, start.X, map, corridor);
            CarveHorizontal(start.X, end.X, end.Y, map, corridor);
        }

        return corridor;
    }

    private void CarveHorizontal(int xStart, int xEnd, int y, TileType[,] map, Corridor corridor)
    {
        int min = Math.Min(xStart, xEnd);
        int max = Math.Max(xStart, xEnd);
        for (int x = min; x <= max; x++)
        {
            if (map[x, y] == TileType.Wall)
            {
                map[x, y] = TileType.Corridor;
                corridor.Tiles.Add(new Coord(x, y));
            }
        }
    }

    private void CarveVertical(int yStart, int yEnd, int x, TileType[,] map, Corridor corridor)
    {
        int min = Math.Min(yStart, yEnd);
        int max = Math.Max(yStart, yEnd);
        for (int y = min; y <= max; y++)
        {
            if (map[x, y] == TileType.Wall)
            {
                map[x, y] = TileType.Corridor;
                corridor.Tiles.Add(new Coord(x, y));
            }
        }
    }

    // Наполнение уровня (чем выше индекс, тем сложнее)
    private void PopulateLevel(Level level, int levelIndex)
    {
        // В стартовой комнате врагов не спавним
        var safeRooms = level.Rooms.Where((r, i) => i != level.StartRoomIndex).ToList();

        // Количество врагов и предметов
        int enemyCount = 3 + (levelIndex / 2); 
        int itemCount = 5 - (levelIndex / 5); 

        for (int i = 0; i < enemyCount; i++)
        {
            var room = safeRooms[_random.Next(safeRooms.Count)];
            var pos = GetRandomPositionInRoom(room, level);
            
            if (pos != null)
            {
                level.Enemies.Add(new Enemy
                {
                    Type = (EnemyType)_random.Next(0, 5), // 5 базовых типов
                    Position = pos.Value,
                    Health = 10 + (levelIndex * 2),
                    Strength = 3 + levelIndex,
                    Dexterity = 3 + levelIndex,
                    Hostility = 5 + (levelIndex / 2)
                });
            }
        }

        for (int i = 0; i < itemCount; i++)
        {
            var room = safeRooms[_random.Next(safeRooms.Count)];
            var pos = GetRandomPositionInRoom(room, level);
            
            if (pos != null)
            {
                level.Items.Add(new Item
                {
                    Type = (ItemType)_random.Next(1, 5), // Без сокровищ (сокровища падают с врагов)
                    SubType = "Item",
                    HealthValue = 10,
                    MaxHealthValue = 1,
                    StrengthValue = 1,
                    DexterityValue = 1,
                    Position = pos.Value
                });
            }
        }
    }

    // Ищет свободную клетку в комнате
    private Coord? GetRandomPositionInRoom(Room room, Level level)
    {
        // Пытаемся найти свободное место 20 раз
        for (int attempt = 0; attempt < 20; attempt++)
        {
            int x = _random.Next(room.X + 1, room.X + room.Width - 1);
            int y = _random.Next(room.Y + 1, room.Y + room.Height - 1);
            var pos = new Coord(x, y);

            // Проверяем, что там нет врагов, предметов и выхода
            bool isOccupied = level.Enemies.Any(e => e.Position == pos) ||
                              level.Items.Any(i => i.Position == pos) ||
                              level.ExitPosition == pos;

            if (!isOccupied)
                return pos;
        }
        return null;
    }
}