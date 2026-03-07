namespace DemaciaRisingSim.Core;

/// <summary>
/// Represents a single settlement on the board with its terrain, tiles, and neighbors.
/// </summary>
public class Settlement
{
    private readonly List<string> _neighbors = [];

    /// <summary>The name of this settlement (e.g., "The Great City", "Brookhollow").</summary>
    public string Name { get; }

    /// <summary>The terrain type(s) for this settlement.</summary>
    public TerrainType Terrain { get; }

    /// <summary>The names of neighboring settlements that receive marketplace bonuses from this settlement.</summary>
    public IReadOnlyList<string> Neighbors => _neighbors;

    /// <summary>
    /// The names of settlements that receive academy bonuses from an academy placed in this settlement.
    /// Typically includes the settlement's own zone plus surrounding settlements.
    /// </summary>
    public IReadOnlyList<string> AcademyBuff { get; }

    /// <summary>The tiles currently placed in this settlement.</summary>
    public TileType[] Tiles { get; set; }

    /// <summary>The current production multiplier (affected by marketplace and academy tiles).</summary>
    public double Multiplier { get; set; } = 1.0;

    public Settlement(string name, TerrainType terrain, IReadOnlyList<string> academyBuff, int tileCount)
    {
        Name = name;
        Terrain = terrain;
        AcademyBuff = academyBuff;
        Tiles = new TileType[tileCount];
        Multiplier = 1.0;
    }

    /// <summary>
    /// Adds a directional link from this settlement to the given neighboring settlement.
    /// A marketplace tile placed in this settlement will boost the linked settlement's multiplier.
    /// </summary>
    public void AddLink(Settlement other) => _neighbors.Add(other.Name);

    /// <summary>
    /// Returns true if petricite tiles are allowed in this settlement.
    /// Only settlements with petricite terrain can produce petricite.
    /// </summary>
    public bool AllowsPetricite => Terrain.HasFlag(TerrainType.Petricite);

    /// <summary>Creates a deep copy of this settlement.</summary>
    public Settlement Clone()
    {
        var clone = new Settlement(Name, Terrain, AcademyBuff, Tiles.Length)
        {
            Tiles = (TileType[])Tiles.Clone(),
            Multiplier = Multiplier,
        };
        foreach (var neighbor in _neighbors)
            clone._neighbors.Add(neighbor);
        return clone;
    }
}
