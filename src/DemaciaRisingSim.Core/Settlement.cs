namespace DemaciaRisingSim.Core;

/// <summary>
/// Represents a single settlement on the board with its environment, structures, and neighbors.
/// </summary>
public class Settlement
{
    private readonly List<string> _neighbors = [];

    /// <summary>The name of this settlement (e.g., "The Great City", "Brookhollow").</summary>
    public string Name { get; }

    /// <summary>The environment type(s) for this settlement.</summary>
    public EnvironmentType Environment { get; }

    /// <summary>True if this settlement is the capital. Only the capital may build a Petricite Mill.</summary>
    public bool IsCapital { get; }

    /// <summary>The names of neighboring settlements that receive marketplace bonuses from this settlement.</summary>
    public IReadOnlyList<string> Neighbors => _neighbors;

    /// <summary>The structures currently placed in this settlement's slots.</summary>
    public Structure[] Structures { get; set; }

    /// <summary>The current production multiplier (affected by Marketplace and Academy structures).</summary>
    public double Multiplier { get; set; } = 1.0;

    public Settlement(string name, EnvironmentType environment,
        bool isCapital = false, int slotCount = GameConstants.SettlementSlotCount)
    {
        Name = name;
        Environment = environment;
        IsCapital = isCapital;
        Structures = new Structure[slotCount];
        Array.Fill(Structures, Structure.Empty);
        Multiplier = 1.0;
    }

    /// <summary>
    /// Adds a directional link from this settlement to the given neighboring settlement.
    /// A Marketplace placed in this settlement will boost the linked settlement's multiplier.
    /// </summary>
    public void AddLink(Settlement other) => _neighbors.Add(other.Name);

    /// <summary>
    /// Returns true if a Petricite Mill may be placed in this settlement.
    /// Petricite Mills can only be constructed in the capital, but multiple may be built.
    /// </summary>
    public bool AllowsPetriciteMill => IsCapital;

    /// <summary>Creates a deep copy of this settlement.</summary>
    public Settlement Clone()
    {
        var clone = new Settlement(Name, Environment, IsCapital, Structures.Length)
        {
            Structures = (Structure[])Structures.Clone(),
            Multiplier = Multiplier,
        };
        foreach (var neighbor in _neighbors)
            clone._neighbors.Add(neighbor);
        return clone;
    }
}
