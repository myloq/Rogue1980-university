using System.Numerics;
using Raylib_cs;

namespace Rogue1980.Presentation.Views;

public class RaylibMenuView
{
    public int DrawMainMenu(bool hasSave)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        int titleWidth = Raylib.MeasureText("ROGUE 1980", 50);
        Raylib.DrawText("ROGUE 1980", 640 - titleWidth/2, 100, 50, Color.Gold);

        int mouseX = (int)Raylib.GetMouseX();
        int mouseY = (int)Raylib.GetMouseY();
        bool isClick = Raylib.IsMouseButtonPressed(MouseButton.Left);
        int result = -1;

        Rectangle btnNew = new Rectangle(540, 250, 200, 40);
        DrawButton(btnNew, "NEW GAME", mouseX, mouseY);
        if (isClick && Raylib.CheckCollisionPointRec(new Vector2(mouseX, mouseY), btnNew)) result = 0;

        Rectangle btnCont = new Rectangle(540, 310, 200, 40);
        DrawButton(btnCont, "CONTINUE", mouseX, mouseY, hasSave);
        if (hasSave && isClick && Raylib.CheckCollisionPointRec(new Vector2(mouseX, mouseY), btnCont)) result = 1;

        Rectangle btnLead = new Rectangle(540, 370, 200, 40);
        DrawButton(btnLead, "LEADERBOARD", mouseX, mouseY);
        if (isClick && Raylib.CheckCollisionPointRec(new Vector2(mouseX, mouseY), btnLead)) result = 2;

        Rectangle btnExit = new Rectangle(540, 430, 200, 40);
        DrawButton(btnExit, "EXIT", mouseX, mouseY);
        if (isClick && Raylib.CheckCollisionPointRec(new Vector2(mouseX, mouseY), btnExit)) result = 3;

        Raylib.EndDrawing();
        return result;
    }

    private void DrawButton(Rectangle rect, string text, int mouseX, int mouseY, bool enabled = true)
    {
        bool isHover = Raylib.CheckCollisionPointRec(new Vector2(mouseX, mouseY), rect);
        Color color = enabled ? (isHover ? Color.Lime : Color.Gray) : Color.DarkGray;

        Raylib.DrawRectangleRec(rect, Color.DarkPurple);
        Raylib.DrawRectangleLinesEx(rect, 2, color);

        int textWidth = Raylib.MeasureText(text, 20);
        Raylib.DrawText(text, (int)(rect.X + rect.Width / 2 - textWidth / 2), (int)(rect.Y + 10), 20, color);
    }
}