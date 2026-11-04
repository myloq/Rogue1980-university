using System.Text.Json;
using Rogue1980.Domain.Interfaces;
using Rogue1980.Domain.Models;

namespace Rogue1980.DataLayer.Repositories;

public class JsonLeaderboardRepository : ILeaderboardProvider
{
    private readonly string _leaderboardFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonLeaderboardRepository(string leaderboardFilePath = "leaderboard.json")
    {
        _leaderboardFilePath = leaderboardFilePath;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public void SaveResult(RunStats stats)
    {
        var currentBoard = GetLeaderboard().ToList();
        currentBoard.Add(stats);
        
        var sortedBoard = currentBoard.OrderByDescending(s => s.TreasuresCollected).ToList();
        
        string json = JsonSerializer.Serialize(sortedBoard, _jsonOptions);
        File.WriteAllText(_leaderboardFilePath, json);
    }

    public IEnumerable<RunStats> GetLeaderboard()
    {
        if (!File.Exists(_leaderboardFilePath)) return [];
        
        string json = File.ReadAllText(_leaderboardFilePath);
        return JsonSerializer.Deserialize<List<RunStats>>(json, _jsonOptions) ?? [];
    }
}