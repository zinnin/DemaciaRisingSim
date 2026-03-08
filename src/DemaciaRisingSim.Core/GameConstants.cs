namespace DemaciaRisingSim.Core;

/// <summary>
/// Game constants for board layout and optimization limits.
/// Production values per structure level are defined in <see cref="StructureData"/>.
/// </summary>
public static class GameConstants
{
    // Board layout
    public const int SettlementSlotCount = 6;

    // Optimization limits
    public const int MaxOptimizationIterations = 50;
}
