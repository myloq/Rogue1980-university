using Rogue1980.Domain.Models;
using Rogue1980.Domain.Models.Enums;
using Rogue1980.Domain.ValueObjects;

namespace Rogue1980.Domain.Services;

public class InventoryService
{
    // Использование оружия (индекс 0 - снять оружие, 1-9 - надеть)
    public void UseWeapon(Character player, Level level, int index)
    {
        if (index == 0)
        {
            // Снимаем оружие
            if (player.EquippedWeapon != null)
            {
                DropWeaponToFloor(player.EquippedWeapon, player.Position, level);
                player.EquippedWeapon = null;
            }
            return;
        }

        // Надеваем новое оружие (индекс 1-9)
        if (index > 0 && index <= player.Backpack.Weapons.Count)
        {
            var newWeapon = player.Backpack.Weapons[index - 1];
            
            // Старое оружие падает на пол
            if (player.EquippedWeapon != null)
            {
                DropWeaponToFloor(player.EquippedWeapon, player.Position, level);
            }

            player.EquippedWeapon = newWeapon;
            player.Backpack.Weapons.RemoveAt(index - 1); // Убираем из рюкзака (оно теперь в руках)
        }
    }

    // Использование еды
    public void UseFood(Character player, int index)
    {
        if (index <= 0 || index > player.Backpack.Foods.Count) return;

        var food = player.Backpack.Foods[index - 1];
        player.Health = Math.Min(player.MaxHealth, player.Health + food.HealthValue);
        player.Backpack.Foods.RemoveAt(index - 1);
    }

    // Использование эликсира (временный бафф)
    public void UseElixir(Character player, int index)
    {
        if (index <= 0 || index > player.Backpack.Elixirs.Count) return;

        var elixir = player.Backpack.Elixirs[index - 1];
        
        // Временно повышаем характеристики
        player.TemporaryStrengthBonus += elixir.StrengthValue;
        player.TemporaryDexterityBonus += elixir.DexterityValue;
        
        if (elixir.MaxHealthValue > 0)
        {
            player.MaxHealth += elixir.MaxHealthValue;
            player.Health += elixir.MaxHealthValue; // Здоровье тоже повышается
        }

        player.Backpack.Elixirs.RemoveAt(index - 1);
    }

    // Использование свитка (постоянный бафф)
    public void UseScroll(Character player, int index)
    {
        if (index <= 0 || index > player.Backpack.Scrolls.Count) return;

        var scroll = player.Backpack.Scrolls[index - 1];
        
        player.Strength += scroll.StrengthValue;
        player.Dexterity += scroll.DexterityValue;
        
        if (scroll.MaxHealthValue > 0)
        {
            player.MaxHealth += scroll.MaxHealthValue;
            player.Health += scroll.MaxHealthValue;
        }

        player.Backpack.Scrolls.RemoveAt(index - 1);
    }

    // Выбрасывание оружия на соседнюю клетку
    private void DropWeaponToFloor(Item weapon, Coord playerPos, Level level)
    {
        // Ищем свободную соседнюю клетку
        foreach (var dir in Coord.Offsets4)
        {
            var dropPos = playerPos + dir;
            if (level.IsWalkable(dropPos.X, dropPos.Y) && 
                !level.Items.Any(i => i.Position == dropPos))
            {
                weapon.Position = dropPos;
                level.Items.Add(weapon);
                return;
            }
        }
        
        // Если все занято, падает прямо под ноги
        weapon.Position = playerPos;
        level.Items.Add(weapon);
    }
}