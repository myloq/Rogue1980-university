using Rogue1980.Domain.Models;

namespace Rogue1980.Domain.Interfaces;

public interface IGameStateRepository
{
    void SaveSession(GameSession session);
    void DeleteSession();
}