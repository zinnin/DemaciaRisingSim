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

    /// <summary>
    /// Minimum food units each settlement should produce. Farms are placed in the
    /// lowest-value slots of each settlement to satisfy this target before the remaining
    /// slots are filled with production structures. Default: 2.
    /// </summary>
    public int FoodTargetPerSettlement { get; set; } = 2;

    /// <summary>Total Lumber the player needs to accumulate. Default: 296 300.</summary>
    public int LumberTarget { get; set; } = 296_300;

    /// <summary>Total Stone the player needs to accumulate. Default: 343 400.</summary>
    public int StoneTarget { get; set; } = 343_400;

    /// <summary>Total Metal the player needs to accumulate. Default: 143 650.</summary>
    public int MetalTarget { get; set; } = 143_650;

    /// <summary>Total Petricite the player needs to accumulate. Default: 1 450.</summary>
    public int PetriciteTarget { get; set; } = 1_450;
}
