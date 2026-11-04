using Rogue1980.Domain.Models;

namespace Rogue1980.Domain.Interfaces;

public interface IGameStateLoader
{
    GameSession? LoadSession();
    bool HasSavedSession();
}