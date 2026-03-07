namespace DemaciaRisingSim.Core;

/// <summary>
/// Game constants for tile production values, resource ratios, and building multipliers.
/// </summary>
public static class GameConstants
{
    // Base production per tile
    public const int LumberTileValue = 150;
    public const int StoneTileValue = 100;
    public const int MetalTileValue = 50;
    public const int PetriciteTileValue = 3;
    public const int FarmTileValue = 5;
    public const int HeartlandFarmBonusValue = 6; // first two farms on a heartland are worth 6

    // Ideal resource ratios relative to lumber
    public const double StoneRatio = 1.25;
    public const double MetalRatio = 1.5;
    public const double PetriciteRatio = 0.3;

    // Multiplier bonuses per marketplace/academy tile
    public const double MarketplaceMultiplier = 0.1;
    public const double AcademyMultiplier = 0.1;

    // Terrain production bonuses
    public const int MountainStoneBonusTiles = 2;  // +2 stone tiles on mountain
    public const int BorderMetalBonusTiles = 2;    // +2 metal tiles on border
    public const double HeartlandLumberBonus = 0.25; // +25% lumber multiplier on heartland

    // Optimization limits
    public const int MaxNonProductiveBuildings = 3;
    public const int MaxOptimizationIterations = 50;
}
