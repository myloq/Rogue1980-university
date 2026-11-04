using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Rogue1980.DataLayer.Dtos;
using Rogue1980.DataLayer.Mapping;
using Rogue1980.Domain.Interfaces;
using Rogue1980.Domain.Models;

namespace Rogue1980.DataLayer.Repositories;

public class JsonGameRepository : IGameStateRepository, IGameStateLoader
{
    private readonly string _saveFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonGameRepository(string saveFilePath = "savegame.json")
    {
        _saveFilePath = saveFilePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
    }

    public void SaveSession(GameSession session)
    {
        var dto = DataMapper.ToDto(session);
        string json = JsonSerializer.Serialize(dto, _jsonOptions);
        File.WriteAllText(_saveFilePath, json);
    }

    public GameSession? LoadSession()
    {
        if (!HasSavedSession()) return null;
        
        string json = File.ReadAllText(_saveFilePath);
        var dto = JsonSerializer.Deserialize<GameSessionDto>(json, _jsonOptions);
        
        if (dto == null) return null;
        
        return DataMapper.ToDomain(dto); 
    }

    public bool HasSavedSession()
    {
        return File.Exists(_saveFilePath);
    }

    public void DeleteSession()
    {
        if (HasSavedSession())
        {
            File.Delete(_saveFilePath);
        }
    }
}