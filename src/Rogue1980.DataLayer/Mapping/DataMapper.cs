using Rogue1980.DataLayer.Dtos;
using Rogue1980.Domain.Models;
using Rogue1980.Domain.Models.Enums;
using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.DataLayer.Mapping;

public static class DataMapper
{
    // --- Domain -> DTO ---
    public static GameSessionDto ToDto(GameSession session)
    {
        return new GameSessionDto
        {
            IsGameOver = session.IsGameOver,
            Player = ToDto(session.Player),
            CurrentLevel = ToDto(session.CurrentLevel),
            Stats = ToDto(session.Stats)
        };
    }

    private static CharacterDto ToDto(Character character)
    {
        return new CharacterDto
        {
            Position = new CoordDto { X = character.Position.X, Y = character.Position.Y },
            Health = character.Health,
            MaxHealth = character.MaxHealth,
            Dexterity = character.Dexterity,
            Strength = character.Strength,
            EquippedWeapon = character.EquippedWeapon != null ? ToDto(character.EquippedWeapon) : null,
            Weapons = character.Backpack.Weapons.Select(ToDto).ToList(),
            Foods = character.Backpack.Foods.Select(ToDto).ToList(),
            Elixirs = character.Backpack.Elixirs.Select(ToDto).ToList(),
            Scrolls = character.Backpack.Scrolls.Select(ToDto).ToList(),
            TreasureCount = character.Backpack.TreasureCount,
            IsAsleep = character.IsAsleep,
            TemporaryStrengthBonus = character.TemporaryStrengthBonus,
            TemporaryDexterityBonus = character.TemporaryDexterityBonus
        };
    }

    private static ItemDto ToDto(Item item)
    {
        return new ItemDto
        {
            Type = (int)item.Type,
            SubType = item.SubType,
            HealthValue = item.HealthValue,
            MaxHealthValue = item.MaxHealthValue,
            DexterityValue = item.DexterityValue,
            StrengthValue = item.StrengthValue,
            Cost = item.Cost,
            Position = new CoordDto { X = item.Position.X, Y = item.Position.Y },
        };
    }

    private static RoomDto ToDto(Room room)
    {
        return new RoomDto { X = room.X, Y = room.Y, Width = room.Width, Height = room.Height };
    }

    private static LevelDto ToDto(Level level)
    {
        int width = level.Map.GetLength(0);
        int height = level.Map.GetLength(1);
        int[] flatMap = new int[width * height];
        
        // "Сплющиваем" 2D массив в 1D
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                flatMap[x * height + y] = (int)level.Map[x, y];
            }
        }

        return new LevelDto
        {
            Index = level.Index,
            MapWidth = width,
            MapHeight = height,
            MapTiles = flatMap,
            Rooms = level.Rooms.Select(ToDto).ToList(),
            Enemies = level.Enemies.Select(ToDto).ToList(),
            Items = level.Items.Select(ToDto).ToList(),
            ExitPosition = new CoordDto { X = level.ExitPosition.X, Y = level.ExitPosition.Y },
            StartRoomIndex = level.StartRoomIndex,
            EndRoomIndex = level.EndRoomIndex
        };
    }

    private static EnemyDto ToDto(Enemy enemy)
    {
        return new EnemyDto
        {
            Type = (int)enemy.Type,
            Position = new CoordDto { X = enemy.Position.X, Y = enemy.Position.Y },
            Health = enemy.Health,
            Dexterity = enemy.Dexterity,
            Strength = enemy.Strength,
            Hostility = enemy.Hostility,
            IsInvisible = enemy.IsInvisible,
            IsResting = enemy.IsResting,
            HasBeenHit = enemy.HasBeenHit
        };
    }

    private static RunStatsDto ToDto(RunStats stats)
    {
        return new RunStatsDto
        {
            LevelReached = stats.LevelReached,
            TreasuresCollected = stats.TreasuresCollected,
            EnemiesKilled = stats.EnemiesKilled,
            FoodEaten = stats.FoodEaten,
            ElixirsDrunk = stats.ElixirsDrunk,
            ScrollsRead = stats.ScrollsRead,
            HitsDealt = stats.HitsDealt,
            HitsMissed = stats.HitsMissed,
            StepsTaken = stats.StepsTaken,
            IsVictory = stats.IsVictory,
            Date = stats.Date
        };
    }

    // --- DTO -> Domain ---
    public static GameSession ToDomain(GameSessionDto dto)
    {
        return new GameSession
        {
            Player = ToDomain(dto.Player),
            CurrentLevel = ToDomain(dto.CurrentLevel),
            Stats = ToDomain(dto.Stats),
            IsGameOver = dto.IsGameOver
        };
    }

    public static Character ToDomain(CharacterDto dto)
    {
        var character = new Character
        {
            Position = new Coord(dto.Position.X, dto.Position.Y),
            Health = dto.Health,
            MaxHealth = dto.MaxHealth,
            Dexterity = dto.Dexterity,
            Strength = dto.Strength,
            EquippedWeapon = dto.EquippedWeapon != null ? ToDomain(dto.EquippedWeapon) : null,
            IsAsleep = dto.IsAsleep,
            TemporaryStrengthBonus = dto.TemporaryStrengthBonus,
            TemporaryDexterityBonus = dto.TemporaryDexterityBonus
        };

        character.Backpack.Weapons.AddRange(dto.Weapons.Select(ToDomain));
        character.Backpack.Foods.AddRange(dto.Foods.Select(ToDomain));
        character.Backpack.Elixirs.AddRange(dto.Elixirs.Select(ToDomain));
        character.Backpack.Scrolls.AddRange(dto.Scrolls.Select(ToDomain));
        
        character.Backpack.AddTreasure(dto.TreasureCount);
        
        return character;
    }

    public static Item ToDomain(ItemDto dto)
    {
        return new Item
        {
            Type = (ItemType)dto.Type,
            SubType = dto.SubType,
            HealthValue = dto.HealthValue,
            MaxHealthValue = dto.MaxHealthValue,
            DexterityValue = dto.DexterityValue,
            StrengthValue = dto.StrengthValue,
            Cost = dto.Cost,
            Position = new Coord(dto.Position.X, dto.Position.Y),
        };
    }

    public static Level ToDomain(LevelDto dto)
    {
        // Восстанавливаем двумерный массив карты
        var map = new TileType[dto.MapWidth, dto.MapHeight];
        for (int x = 0; x < dto.MapWidth; x++)
        {
            for (int y = 0; y < dto.MapHeight; y++)
            {
                map[x, y] = (TileType)dto.MapTiles[x * dto.MapHeight + y];
            }
        }

        var level = new Level
        {
            Index = dto.Index,
            Map = map,
            ExitPosition = new Coord(dto.ExitPosition.X, dto.ExitPosition.Y),
            StartRoomIndex = dto.StartRoomIndex,
            EndRoomIndex = dto.EndRoomIndex
        };

        // Восстанавливаем комнаты (нужны, например, для телепорта привидения)
        level.Rooms.AddRange((dto.Rooms ?? []).Select(r => new Room { X = r.X, Y = r.Y, Width = r.Width, Height = r.Height }));

        // Восстанавливаем сущности
        level.Enemies.AddRange(dto.Enemies.Select(ToDomain));
        level.Items.AddRange(dto.Items.Select(ToDomain));

        return level;
    }

    public static Enemy ToDomain(EnemyDto dto)
    {
        return new Enemy
        {
            Type = (EnemyType)dto.Type,
            Position = new Coord(dto.Position.X, dto.Position.Y),
            Health = dto.Health,
            Dexterity = dto.Dexterity,
            Strength = dto.Strength,
            Hostility = dto.Hostility,
            IsInvisible = dto.IsInvisible,
            IsResting = dto.IsResting,
            HasBeenHit = dto.HasBeenHit
        };
    }

    public static RunStats ToDomain(RunStatsDto dto)
    {
        return new RunStats
        {
            LevelReached = dto.LevelReached,
            TreasuresCollected = dto.TreasuresCollected,
            EnemiesKilled = dto.EnemiesKilled,
            FoodEaten = dto.FoodEaten,
            ElixirsDrunk = dto.ElixirsDrunk,
            ScrollsRead = dto.ScrollsRead,
            HitsDealt = dto.HitsDealt,
            HitsMissed = dto.HitsMissed,
            StepsTaken = dto.StepsTaken,
            IsVictory = dto.IsVictory,
            Date = dto.Date
        };
    }
}