namespace DemaciaRisingSim.Core;

/// <summary>
/// Represents a single city (settlement) on the board with its terrain, tiles, and neighbors.
/// </summary>
public class City
{
    /// <summary>The unique identifier for this city (e.g., "A", "B").</summary>
    public string Id { get; }

    /// <summary>The terrain type(s) for this city.</summary>
    public TerrainType Terrain { get; }

    /// <summary>The IDs of neighboring cities that receive marketplace bonuses from this city.</summary>
    public IReadOnlyList<string> Neighbors { get; }

    /// <summary>
    /// The IDs of cities that receive academy bonuses from an academy placed in this city.
    /// Typically includes the city's own zone plus surrounding cities.
    /// </summary>
    public IReadOnlyList<string> AcademyBuff { get; }

    /// <summary>The tiles currently placed in this city.</summary>
    public TileType[] Tiles { get; set; }

    /// <summary>The current production multiplier (affected by marketplace and academy tiles).</summary>
    public double Multiplier { get; set; } = 1.0;

    public City(string id, TerrainType terrain, IReadOnlyList<string> neighbors, IReadOnlyList<string> academyBuff, int tileCount)
    {
        Id = id;
        Terrain = terrain;
        Neighbors = neighbors;
        AcademyBuff = academyBuff;
        Tiles = new TileType[tileCount];
        Multiplier = 1.0;
    }

    /// <summary>
    /// Returns true if petricite tiles are allowed in this city.
    /// Only cities with petricite terrain can produce petricite.
    /// </summary>
    public bool AllowsPetricite => Terrain.HasFlag(TerrainType.Petricite);

    /// <summary>Creates a deep copy of this city.</summary>
    public City Clone()
    {
        var clone = new City(Id, Terrain, Neighbors, AcademyBuff, Tiles.Length)
        {
            Tiles = (TileType[])Tiles.Clone(),
            Multiplier = Multiplier,
        };
        return clone;
    }
}
