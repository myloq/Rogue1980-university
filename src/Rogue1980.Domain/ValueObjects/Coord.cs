namespace Rogue1980.Domain.ValueObjects;

public readonly record struct Coord(int X, int Y)
{
    public static readonly Coord Up = new(0, -1);
    public static readonly Coord Down = new(0, 1);
    public static readonly Coord Left = new(-1, 0);
    public static readonly Coord Right = new(1, 0);
    
    public static readonly Coord[] Offsets4 = [Up, Down, Left, Right];
    public static Coord operator +(Coord a, Coord b) => new(a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => new(a.X - b.X, a.Y - b.Y);
    public static Coord operator *(Coord a, int k) => new(a.X * k, a.Y * k);
}