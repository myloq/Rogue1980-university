namespace Rogue1980.DataLayer.Dtos;

public class LevelDto
{
    public int Index { get; set; }
    
    // Двумерный массив заменяем на размеры и одномерный массив
    public int MapWidth { get; set; }
    public int MapHeight { get; set; }
    public int[] MapTiles { get; set; } = []; 
    
    public List<RoomDto> Rooms { get; set; } = [];
    public List<EnemyDto> Enemies { get; set; } = [];
    public List<ItemDto> Items { get; set; } = [];
    
    public CoordDto ExitPosition { get; set; } = new();
    
    // Для простоты в рамках сохранения можно сохранить только базовые индексы
    public int StartRoomIndex { get; set; }
    public int EndRoomIndex { get; set; }
}