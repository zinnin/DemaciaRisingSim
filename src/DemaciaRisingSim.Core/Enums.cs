namespace DemaciaRisingSim.Core;

/// <summary>
/// The type of production tile that can be placed in a city.
/// </summary>
public enum TileType
{
    Lumber = 0,
    Stone = 1,
    Metal = 2,
    Marketplace = 3,
    Academy = 4,
    Petricite = 5,
    NonProductive = 6,
    Farm = 7,
}

/// <summary>
/// Terrain types that affect city production and tile restrictions.
/// </summary>
[Flags]
public enum TerrainType
{
    None = 0,
    Heartland = 1,
    Border = 2,
    Mountain = 4,
    Petricite = 8,
}
