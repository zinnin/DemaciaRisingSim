namespace DemaciaRisingSim.Core;

/// <summary>
/// Controls which structures the optimizer must include and which settlements it may touch.
/// </summary>
public class SimulationSettings
{
    /// <summary>
    /// When true the optimizer reserves a slot in The Great City for Durand's Workshop
    /// (produces 1 Petricite/turn). Default: true.
    /// </summary>
    public bool RequireDurandsWorkshop { get; set; } = true;

    /// <summary>
    /// When true the optimizer reserves a slot in The Great City for the Shrine of the
    /// Veiled Lady (kingdom-wide damage reduction). Default: true.
    /// </summary>
    public bool RequireShrineOfVeiledLady { get; set; } = true;

    /// <summary>
    /// When true the optimizer reserves a slot in High Silvermere for a Quartermaster
    /// (25% combat damage boost to this and neighboring settlements). Default: true.
    /// </summary>
    public bool RequireQuartermaster { get; set; } = true;

    /// <summary>
    /// Settlement names that are locked (not yet unlocked in the player's game).
    /// The optimizer will not change structure assignments in locked settlements.
    /// Default: empty (all settlements are unlocked).
    /// </summary>
    public HashSet<string> LockedSettlements { get; set; } = [];

    /// <summary>
    /// Maximum building level the optimizer is allowed to try (1–4). Default: 4.
    /// </summary>
    public int MaxBuildingLevel { get; set; } = 4;
}
