using System.Drawing;
using Raylib_cs;
using Rogue1980.Domain.Generation;
using Rogue1980.Domain.Models;
using Rogue1980.Domain.Models.Enums;
using Rogue1980.Domain.Services;
using Rogue1980.Domain.Gameplay;
using Rogue1980.Domain.ValueObjects;
using Rogue1980.DataLayer.Repositories;
using Rogue1980.Presentation.Views;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace Rogue1980.App;

public class Program
{
    private static readonly JsonGameRepository GameRepo = new("savegame.json");
    private static readonly JsonLeaderboardRepository LeaderboardRepo = new("leaderboard.json");
    private static int _lastLevelIndex = 0;

    private enum GameState { MainMenu, Playing, Leaderboard }

    public static void Main(string[] args)
    {
        Raylib.InitWindow(1280, 600, "Rogue 1980");
        Raylib.SetTargetFPS(60);
        Raylib.SetExitKey(KeyboardKey.Null); // Esc обрабатываем вручную: Esc - полный выход из игры
        
        var menuView = new RaylibMenuView();

        GameState state = GameState.MainMenu;
        
        GameSession? currentSession = null;
        GameEngine? currentEngine = null;
        RaylibGameView? gameView = null;
        
        bool isInventoryOpen = false;
        char inventoryType = ' ';

        while (!Raylib.WindowShouldClose())
        {
            // Esc - полный выход из игры из любого состояния
            if (Raylib.IsKeyPressed(KeyboardKey.Escape)) break;

            if (currentSession != null && currentSession.CurrentLevel.Index != _lastLevelIndex)
            {
                _lastLevelIndex = currentSession.CurrentLevel.Index;
                gameView?.OnLevelChanged();
            }
            
            switch (state)
            {
                case GameState.MainMenu:
                    // Освобождаем ресурсы предыдущей игры при выходе в меню
                    if (gameView != null) { gameView.Close(); gameView = null; } 
                    currentEngine = null;
                    currentSession = null;

                    int menuAction = menuView.DrawMainMenu(GameRepo.HasSavedSession());
                    
                    if (menuAction == 0) // Новая игра
                    {
                        currentSession = CreateNewSession();
                        currentEngine = SetupEngine(currentSession);
                        gameView = new RaylibGameView();
                        gameView.Initialize();
                        state = GameState.Playing;
                    }
                    else if (menuAction == 1) // Продолжить
                    {
                        // Поврежденный сейв не должен ронять игру
                        try { currentSession = GameRepo.LoadSession(); }
                        catch { currentSession = null; }

                        if (currentSession != null)
                        {
                            currentEngine = SetupEngine(currentSession);
                            gameView = new RaylibGameView();
                            gameView.Initialize();
                            state = GameState.Playing;
                        }
                    }
                    else if (menuAction == 2) // Таблица лидеров
                    {
                        state = GameState.Leaderboard;
                    }
                    else if (menuAction == 3) // Выход
                    {
                        return; 
                    }
                    break;

                case GameState.Leaderboard:
                    if (DrawLeaderboard())
                    {
                        state = GameState.MainMenu;
                    }
                    break;

                case GameState.Playing:
                    // Страховка: в состоянии игры все объекты должны существовать
                    if (currentSession == null || currentEngine == null || gameView == null)
                    {
                        state = GameState.MainMenu;
                        break;
                    }

                    if (currentSession.IsGameOver)
                    {
                        LeaderboardRepo.SaveResult(currentSession.Stats);
                        GameRepo.DeleteSession(); // Удаляем сейв после смерти
                        state = GameState.MainMenu;
                        break;
                    }

                    // 1. Движение (только если инвентарь закрыт)
                    if (!isInventoryOpen)
                    {
                        Coord direction = new Coord(0, 0);
                        if (Raylib.IsKeyPressed(KeyboardKey.W)) direction = Coord.Up;
                        if (Raylib.IsKeyPressed(KeyboardKey.S)) direction = Coord.Down;
                        if (Raylib.IsKeyPressed(KeyboardKey.A)) direction = Coord.Left;
                        if (Raylib.IsKeyPressed(KeyboardKey.D)) direction = Coord.Right;
                        
                        if (direction != new Coord(0, 0)) currentEngine!.MovePlayer(direction);

                        // Открытие инвентаря
                        if (Raylib.IsKeyPressed(KeyboardKey.H)) { isInventoryOpen = true; inventoryType = 'h'; }
                        if (Raylib.IsKeyPressed(KeyboardKey.J)) { isInventoryOpen = true; inventoryType = 'j'; }
                        if (Raylib.IsKeyPressed(KeyboardKey.K)) { isInventoryOpen = true; inventoryType = 'k'; }
                        if (Raylib.IsKeyPressed(KeyboardKey.E)) { isInventoryOpen = true; inventoryType = 'e'; }
                    }

                    // 2. Отрисовка и получение действия инвентаря
                    int invAction = gameView!.Render(currentSession, isInventoryOpen, inventoryType, currentEngine!);

                    // 3. Обработка действия инвентаря
                    if (invAction == -2)
                    {
                        isInventoryOpen = false; // Кнопка "Close" или клавиша Q
                    }
                    else if (invAction >= 0)
                    {
                        currentEngine.UseItem(inventoryType, invAction); // Использование предмета
                        isInventoryOpen = false; // Автоматически закрываем меню
                    }
                    break;
            }
        }

        Raylib.CloseWindow();
    }

    private static GameSession CreateNewSession()
    {
        var generator = new LevelGenerator();
        var level = generator.Generate(1);
        var startRoom = level.Rooms[level.StartRoomIndex];

        return new GameSession
        {
            Player = new Character
            {
                Position = startRoom.Center,
                Health = 100, MaxHealth = 100, Strength = 10, Dexterity = 10
            },
            CurrentLevel = level,
            Stats = new RunStats { LevelReached = 1 }
        };
    }

    private static GameEngine SetupEngine(GameSession session)
    {
        return new GameEngine(
            session, 
            new MoveService(), 
            new CombatService(), 
            new EnemyAiService(), 
            new InventoryService(), 
            new LevelGenerator(), 
            GameRepo
        );
    }

    private static bool DrawLeaderboard()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawText("LEADERBOARD", 550, 50, 30, Color.Gold);

        // Ограничиваем вывод, чтобы записи не наезжали на кнопку BACK
        var results = LeaderboardRepo.GetLeaderboard().OrderByDescending(r => r.TreasuresCollected).Take(12).ToList();
        int y = 100;
        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            Raylib.DrawText($"{i+1}. Gold: {r.TreasuresCollected} | Level: {r.LevelReached} | Kills: {r.EnemiesKilled}", 400, y, 20, Color.White);
            y += 30;
        }

        Rectangle backBtn = new Rectangle(540, 500, 200, 40);
        bool isHover = Raylib.CheckCollisionPointRec(new System.Numerics.Vector2(Raylib.GetMouseX(), Raylib.GetMouseY()), backBtn);
        
        Raylib.DrawRectangleRec(backBtn, isHover ? Color.DarkPurple : Color.Black);
        Raylib.DrawRectangleLinesEx(backBtn, 2, isHover ? Color.Lime : Color.Gray);
        
        int textWidth = Raylib.MeasureText("BACK (Q)", 20);
        Raylib.DrawText("BACK (Q)", 540 + 100 - textWidth/2, 510, 20, Color.White);

        // Q тоже возвращает в меню
        bool clicked = (isHover && Raylib.IsMouseButtonPressed(MouseButton.Left)) || Raylib.IsKeyPressed(KeyboardKey.Q);

        Raylib.EndDrawing();
        return clicked;
    }
}