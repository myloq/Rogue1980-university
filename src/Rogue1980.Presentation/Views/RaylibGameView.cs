using System.Numerics;
using Raylib_cs;
using Rogue1980.Domain.Gameplay;
using Rogue1980.Domain.Models;
using Rogue1980.Domain.Models.Enums;
using Rogue1980.Domain.ValueObjects;
using Rogue1980.Presentation.Services;

namespace Rogue1980.Presentation.Views;

public class RaylibGameView
{
    private const int TileSize = 16;
    private readonly FogOfWarService _fogOfWar = new();
    private HashSet<Coord> _visibleTiles = new();

    // Лог событий (подобранные предметы и т.д.) для нижней части экрана
    private sealed record LogEntry(string Text, DateTime ShownAt);
    private readonly List<LogEntry> _log = new();
    private const int MaxLogLines = 3;
    private const double LogLifetimeSec = 6.0;

    public void Initialize() { } // Окно уже создано в Program.cs

    public void OnLevelChanged()
    {
        _fogOfWar.Reset();
        _visibleTiles.Clear();
    }

    public int Render(GameSession session, bool isInventoryOpen, char inventoryType, GameEngine engine)
    {
        _visibleTiles = _fogOfWar.CalculateVisibility(session.Player, session.CurrentLevel);
        CollectMessages(engine);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        DrawMap(session);
        DrawEntities(session);
        DrawUi(session);

        int action = -1;
        if (isInventoryOpen) action = DrawInventoryMenu(session, inventoryType);

        Raylib.EndDrawing();
        return action;
    }

    private void DrawMap(GameSession session)
    {
        var level = session.CurrentLevel;
        for (int x = 0; x < level.Map.GetLength(0); x++)
        {
            for (int y = 0; y < level.Map.GetLength(1); y++)
            {
                var pos = new Coord(x, y);
                if (!_visibleTiles.Contains(pos) && !_fogOfWar.ExploredTiles.Contains(pos)) continue;

                bool isVisibleNow = _visibleTiles.Contains(pos);
                var tile = level.Map[x, y];
                char symbol = ' ';
                Color color = Color.Black;

                switch (tile)
                {
                    case TileType.Wall: symbol = '#'; color = isVisibleNow ? Color.DarkGray : new Color(40, 40, 40, 255); break;
                    case TileType.Floor: symbol = '.'; color = isVisibleNow ? Color.Gray : new Color(30, 30, 30, 255); break;
                    case TileType.Corridor: symbol = '+'; color = isVisibleNow ? Color.DarkGray : new Color(30, 30, 30, 255); break;
                    case TileType.Exit: symbol = 'E'; color = isVisibleNow ? Color.Green : Color.DarkGreen; break;
                }

                if (symbol != ' ')
                    Raylib.DrawText(symbol.ToString(), x * TileSize, y * TileSize, TileSize, color);
            }
        }
    }

    private void DrawEntities(GameSession session)
    {
        var level = session.CurrentLevel;

        foreach (var item in level.Items)
            if (_visibleTiles.Contains(item.Position))
                Raylib.DrawText("$", item.Position.X * TileSize, item.Position.Y * TileSize, TileSize, Color.Gold);

        foreach (var enemy in level.Enemies)
        {
            if (!_visibleTiles.Contains(enemy.Position)) continue;

            string symbol = enemy.Type switch
            {
                EnemyType.Zombie => "z", EnemyType.Vampire => "v", EnemyType.Ghost => "g",
                EnemyType.Ogre => "O", EnemyType.SnakeMage => "s", _ => "?"
            };
            Color color = enemy.Type switch
            {
                EnemyType.Zombie => Color.Green, EnemyType.Vampire => Color.Red, EnemyType.Ghost => Color.White,
                EnemyType.Ogre => Color.Yellow, EnemyType.SnakeMage => Color.LightGray, _ => Color.Red
            };
            Raylib.DrawText(symbol, enemy.Position.X * TileSize, enemy.Position.Y * TileSize, TileSize, color);
        }

        Raylib.DrawText("@", session.Player.Position.X * TileSize, session.Player.Position.Y * TileSize, TileSize, Color.SkyBlue);
    }

    private void DrawUi(GameSession session)
    {
        int uiY = session.CurrentLevel.Map.GetLength(1) * TileSize + 10;
        
        Raylib.DrawText($"HP: {session.Player.Health}/{session.Player.MaxHealth} | STR: {session.Player.Strength} | DEX: {session.Player.Dexterity}", 10, uiY, 20, Color.White);
        Raylib.DrawText("WASD - Move | H/J/K/E - Inventory | Q - Close | Esc - Quit", 700, uiY + 4, 16, Color.Gray);
        Raylib.DrawText($"Gold: {session.Player.Backpack.TreasureCount} | Level: {session.CurrentLevel.Index}", 10, uiY + 25, 20, Color.Gold);

        // Лог последних событий (подобранные предметы и др.) в самом низу экрана
        DrawMessageLog(10, uiY + 55);
    }

    // Забирает сообщения из движка и подчищает устаревшие записи
    private void CollectMessages(GameEngine engine)
    {
        foreach (var message in engine.DrainMessages())
            _log.Add(new LogEntry(message, DateTime.UtcNow));

        _log.RemoveAll(entry => (DateTime.UtcNow - entry.ShownAt).TotalSeconds > LogLifetimeSec);
        if (_log.Count > 16) _log.RemoveRange(0, _log.Count - 16);
    }

    private void DrawMessageLog(int x, int yTop)
    {
        var visible = _log.TakeLast(MaxLogLines).ToList();
        for (int i = 0; i < visible.Count; i++)
        {
            double age = (DateTime.UtcNow - visible[i].ShownAt).TotalSeconds;
            // Затухание записи в течение последних 1.5 секунд её жизни
            float fade = (float)Math.Clamp(LogLifetimeSec - age, 0.0, 1.5) / 1.5f;
            bool isNewest = i == visible.Count - 1;
            Color color = isNewest ? Color.Gold : Color.LightGray;
            Raylib.DrawText(visible[i].Text, x, yTop + i * 18, 16, Raylib.Fade(color, fade));
        }
    }

    private int DrawInventoryMenu(GameSession session, char type)
    {
        Raylib.DrawRectangle(0, 0, 1280, 600, new Color(0, 0, 0, 200));

        var backpack = session.Player.Backpack;
        List<Item> items = type switch
        {
            'h' => backpack.Weapons, 'j' => backpack.Foods, 'k' => backpack.Elixirs, 'e' => backpack.Scrolls, _ => new List<Item>()
        };

        string title = type switch { 'h' => "WEAPONS", 'j' => "FOOD", 'k' => "ELIXIRS", 'e' => "SCROLLS", _ => "INVENTORY" };
        Raylib.DrawText($"INVENTORY: {title}", 100, 100, 30, Color.Gold);

        int y = 150;
        int mouseX = (int)Raylib.GetMouseX();
        int mouseY = (int)Raylib.GetMouseY();
        int selectedIndex = -1;

        if (type == 'h' && session.Player.EquippedWeapon != null)
        {
            Rectangle btn = new Rectangle(100, y, 400, 28);
            if (DrawInvButton(btn, "0. Unequip current weapon", mouseX, mouseY)) selectedIndex = 0;
            y += 34;
        }

        for (int i = 0; i < items.Count; i++)
        {
            Rectangle btn = new Rectangle(100, y, 400, 28);
            if (DrawInvButton(btn, $"{i + 1}. {items[i].SubType} (HP:{items[i].HealthValue} STR:{items[i].StrengthValue})", mouseX, mouseY))
                selectedIndex = i + 1;
            y += 34;
        }

        Rectangle closeBtn = new Rectangle(100, 520, 200, 40);
        // Q или кнопка - закрыть инвентарь
        if (DrawInvButton(closeBtn, "Close (Q)", mouseX, mouseY) || Raylib.IsKeyPressed(KeyboardKey.Q))
            return -2;

        return selectedIndex;
    }

    private bool DrawInvButton(Rectangle rect, string text, int mouseX, int mouseY)
    {
        bool isHover = Raylib.CheckCollisionPointRec(new Vector2(mouseX, mouseY), rect);
        Raylib.DrawRectangleRec(rect, isHover ? Color.DarkPurple : Color.Black);
        Raylib.DrawRectangleLinesEx(rect, 1, isHover ? Color.Lime : Color.Gray);
        Raylib.DrawText(text, (int)rect.X + 10, (int)rect.Y + 5, 20, Color.White);
        
        return isHover && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }
    
    public void Close() { } // Окно закрывается в Program.cs
}