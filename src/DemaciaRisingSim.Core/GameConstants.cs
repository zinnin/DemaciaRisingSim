namespace DemaciaRisingSim.Core;

/// <summary>
/// Game constants used for score normalization, resource ratios, and optimization limits.
/// Production values per structure level are defined in <see cref="StructureData"/>.
/// </summary>
public static class GameConstants
{
    // Score normalization — max-level output values used to put resources on a common scale
    public const int LumberTileValue   = 150;  // Lumberyard L4 output
    public const int StoneTileValue    = 100;  // Quarry L4 output
    public const int MetalTileValue    = 50;   // Forge L4 output
    public const int PetriciteTileValue = 3;   // Petricite Mill L3 output

    // Ideal resource ratios relative to lumber (targets for the score function)
    public const double StoneRatio     = 1.25;
    public const double MetalRatio     = 1.5;
    public const double PetriciteRatio = 0.3;

    // Board layout
    public const int SettlementSlotCount = 6;

    // Optimization limits
    public const int MaxOptimizationIterations = 50;
}
