namespace DemaciaRisingSim.Core;

/// <summary>
/// Represents a single city (settlement) on the board with its terrain, tiles, and neighbors.
/// </summary>
public class City
{
    private readonly List<string> _neighbors = [];

    /// <summary>The name of this city (e.g., "The Great City", "Brookhollow").</summary>
    public string Name { get; }

    /// <summary>The terrain type(s) for this city.</summary>
    public TerrainType Terrain { get; }

    /// <summary>The names of neighboring cities that receive marketplace bonuses from this city.</summary>
    public IReadOnlyList<string> Neighbors => _neighbors;

    /// <summary>
    /// The names of cities that receive academy bonuses from an academy placed in this city.
    /// Typically includes the city's own zone plus surrounding cities.
    /// </summary>
    public IReadOnlyList<string> AcademyBuff { get; }

    /// <summary>The tiles currently placed in this city.</summary>
    public TileType[] Tiles { get; set; }

    /// <summary>The current production multiplier (affected by marketplace and academy tiles).</summary>
    public double Multiplier { get; set; } = 1.0;

    public City(string name, TerrainType terrain, IReadOnlyList<string> academyBuff, int tileCount)
    {
        Name = name;
        Terrain = terrain;
        AcademyBuff = academyBuff;
        Tiles = new TileType[tileCount];
        Multiplier = 1.0;
    }

    /// <summary>
    /// Adds a directional link from this city to the given neighboring city.
    /// A marketplace tile placed in this city will boost the linked city's multiplier.
    /// </summary>
    public void AddLink(City other) => _neighbors.Add(other.Name);

    /// <summary>
    /// Returns true if petricite tiles are allowed in this city.
    /// Only cities with petricite terrain can produce petricite.
    /// </summary>
    public bool AllowsPetricite => Terrain.HasFlag(TerrainType.Petricite);

    /// <summary>Creates a deep copy of this city.</summary>
    public City Clone()
    {
        var clone = new City(Name, Terrain, AcademyBuff, Tiles.Length)
        {
            Tiles = (TileType[])Tiles.Clone(),
            Multiplier = Multiplier,
        };
        foreach (var neighbor in _neighbors)
            clone._neighbors.Add(neighbor);
        return clone;
    }
}
