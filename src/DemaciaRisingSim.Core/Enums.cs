namespace DemaciaRisingSim.Core;

/// <summary>
/// The type of structure that can be placed in a settlement slot.
/// </summary>
public enum StructureType
{
    Empty = 0,
    Lumberyard,
    Farm,
    Quarry,
    Forge,
    Academy,
    Marketplace,
    PetriciteMill,
    Watchtower,
    MilitiaHeadquarters,
    Barracks,
    Quartermaster,
    ShrineOfVeiledLady,
    DurandsWorkshop,
}

/// <summary>
/// Environment types that affect settlement production and structure bonuses.
/// Academy bonuses apply to Heartland, Mountain, and Border environments only.
/// </summary>
[Flags]
public enum EnvironmentType
{
    None = 0,
    Heartland = 1,
    Border = 2,
    Mountain = 4,
    Petricite = 8,
    Woodland = 16,
}
