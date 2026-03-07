namespace DemaciaRisingSim.Core;

/// <summary>
/// Provides the default board configuration for Demacia Rising with all 16 cities (A-P).
/// Each city has its terrain, tile count, neighbor list, and academy buff zone defined.
/// </summary>
public static class BoardData
{
    /// <summary>
    /// Creates a fresh default board with all 16 cities initialized to Lumber tiles.
    /// </summary>
    public static Dictionary<string, City> CreateDefaultBoard()
    {
        return new Dictionary<string, City>
        {
            ["A"] = new City("A",
                terrain: TerrainType.Petricite,
                neighbors: ["D", "I", "N"],
                academyBuff: ["A"],
                tileCount: 5),

            ["B"] = new City("B",
                terrain: TerrainType.Heartland,
                neighbors: ["E", "M", "N", "P"],
                academyBuff: ["B", "H", "J", "N", "P"],
                tileCount: 6),

            ["C"] = new City("C",
                terrain: TerrainType.Border,
                neighbors: ["H", "J", "M", "N"],
                academyBuff: ["C", "D", "F", "K", "M"],
                tileCount: 5),

            ["D"] = new City("D",
                terrain: TerrainType.Petricite | TerrainType.Border,
                neighbors: ["A", "L", "P"],
                academyBuff: ["C", "D", "F", "K", "M"],
                tileCount: 5),

            ["E"] = new City("E",
                terrain: TerrainType.Mountain,
                neighbors: ["B", "P"],
                academyBuff: ["E", "G", "I", "L", "O"],
                tileCount: 5),

            ["F"] = new City("F",
                terrain: TerrainType.Border,
                neighbors: ["I", "L"],
                academyBuff: ["C", "D", "F", "K", "M"],
                tileCount: 5),

            ["G"] = new City("G",
                terrain: TerrainType.Mountain,
                neighbors: ["I", "O"],
                academyBuff: ["E", "G", "I", "L", "O"],
                tileCount: 5),

            ["H"] = new City("H",
                terrain: TerrainType.Heartland,
                neighbors: ["C", "J"],
                academyBuff: ["B", "H", "J", "N", "P"],
                tileCount: 6),

            ["I"] = new City("I",
                terrain: TerrainType.Petricite | TerrainType.Mountain,
                neighbors: ["A", "F", "G", "J", "L", "O"],
                academyBuff: ["E", "G", "I", "L", "O"],
                tileCount: 5),

            ["J"] = new City("J",
                terrain: TerrainType.Heartland,
                neighbors: ["C", "H", "I", "K", "N"],
                academyBuff: ["B", "H", "J", "N", "P"],
                tileCount: 6),

            ["K"] = new City("K",
                terrain: TerrainType.Border,
                neighbors: ["J", "O"],
                academyBuff: ["C", "D", "F", "K", "M"],
                tileCount: 5),

            ["L"] = new City("L",
                terrain: TerrainType.Mountain,
                neighbors: ["D", "F", "I"],
                academyBuff: ["E", "G", "I", "L", "O"],
                tileCount: 5),

            ["M"] = new City("M",
                terrain: TerrainType.Border,
                neighbors: ["B", "C"],
                academyBuff: ["C", "D", "F", "K", "M"],
                tileCount: 5),

            ["N"] = new City("N",
                terrain: TerrainType.Petricite | TerrainType.Heartland,
                neighbors: ["A", "B", "C", "J"],
                academyBuff: ["B", "H", "J", "N", "P"],
                tileCount: 6),

            ["O"] = new City("O",
                terrain: TerrainType.Mountain,
                neighbors: ["G", "I", "K"],
                academyBuff: ["E", "G", "I", "L", "O"],
                tileCount: 5),

            ["P"] = new City("P",
                terrain: TerrainType.Heartland,
                neighbors: ["B", "D", "E"],
                academyBuff: ["B", "H", "J", "N", "P"],
                tileCount: 6),
        };
    }

    /// <summary>Creates a deep copy of a board.</summary>
    public static Dictionary<string, City> Clone(Dictionary<string, City> board) =>
        board.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());
}
