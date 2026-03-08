namespace DemaciaRisingSim.Core;

/// <summary>A structure instance placed in one settlement slot.</summary>
public record Structure(StructureType Type, int Level = 1)
{
    /// <summary>An empty/unoccupied slot.</summary>
    public static readonly Structure Empty = new(StructureType.Empty, 0);

    public override string ToString() =>
        Type == StructureType.Empty ? "Empty" : $"{Type} L{Level}";
}

/// <summary>
/// Static production and cost data for one structure type at one level,
/// sourced directly from the Demacia Rising structures table.
/// </summary>
public record StructureDefinition(
    StructureType Type,
    int Level,
    int LumberOutput,
    int StoneOutput,
    int MetalOutput,
    int PetriciteOutput,
    int FoodOutput,
    double MarketplaceMultiplier,
    double AcademyMultiplier,
    int CostLumber,
    int CostStone,
    int CostMetal,
    int CostPetricite,
    int BuildTurns,
    bool CapitalOnly = false
);

/// <summary>
/// Lookup table of all structure definitions keyed by (StructureType, Level).
/// All production values, multiplier bonuses, and build costs are sourced from
/// the official Demacia Rising structures table.
/// </summary>
public static class StructureData
{
    private static readonly Dictionary<(StructureType, int), StructureDefinition> _defs = BuildDefs();

    /// <summary>Gets the definition for the given structure type and level.</summary>
    public static StructureDefinition Get(StructureType type, int level) => _defs[(type, level)];

    /// <summary>Returns true if the given type and level combination exists.</summary>
    public static bool Exists(StructureType type, int level) => _defs.ContainsKey((type, level));

    /// <summary>Returns the maximum level for the given structure type (0 for Empty).</summary>
    public static int MaxLevel(StructureType type) => type switch
    {
        StructureType.Lumberyard          => 4,
        StructureType.Farm                => 4,
        StructureType.Quarry              => 4,
        StructureType.Forge               => 4,
        StructureType.Academy             => 3,
        StructureType.Marketplace         => 3,
        StructureType.PetriciteMill       => 3,
        StructureType.Watchtower          => 3,
        StructureType.MilitiaHeadquarters => 3,
        StructureType.Barracks            => 2,
        StructureType.Quartermaster       => 1,
        StructureType.ShrineOfVeiledLady  => 1,
        StructureType.DurandsWorkshop     => 1,
        _                                 => 0,
    };

    /// <summary>All defined structure definitions.</summary>
    public static IEnumerable<StructureDefinition> All => _defs.Values;

    private static Dictionary<(StructureType, int), StructureDefinition> BuildDefs()
    {
        var defs = new Dictionary<(StructureType, int), StructureDefinition>();
        void Add(StructureDefinition d) => defs[(d.Type, d.Level)] = d;

        // --- Empty ---
        Add(new(StructureType.Empty, 0,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 0, CostStone: 0, CostMetal: 0, CostPetricite: 0, BuildTurns: 0));

        // --- Lumberyard (4 levels) ---
        // Produces Lumber every turn, increasing with level.
        Add(new(StructureType.Lumberyard, 1,
            LumberOutput: 15, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 0, CostStone: 0, CostMetal: 0, CostPetricite: 0, BuildTurns: 1));
        Add(new(StructureType.Lumberyard, 2,
            LumberOutput: 30, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 0, CostStone: 100, CostMetal: 10, CostPetricite: 0, BuildTurns: 2));
        Add(new(StructureType.Lumberyard, 3,
            LumberOutput: 75, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 0, CostStone: 300, CostMetal: 100, CostPetricite: 2, BuildTurns: 3));
        Add(new(StructureType.Lumberyard, 4,
            LumberOutput: 150, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 0, CostStone: 1000, CostMetal: 500, CostPetricite: 15, BuildTurns: 3));

        // --- Farm (4 levels) ---
        // Increases Food. First farm in a Heartland settlement grants +1 Food (handled in Simulator).
        Add(new(StructureType.Farm, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 1,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 20, CostStone: 0, CostMetal: 0, CostPetricite: 0, BuildTurns: 1));
        Add(new(StructureType.Farm, 2,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 2,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 200, CostStone: 100, CostMetal: 50, CostPetricite: 0, BuildTurns: 2));
        Add(new(StructureType.Farm, 3,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 3,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 500, CostStone: 300, CostMetal: 0, CostPetricite: 5, BuildTurns: 3));
        Add(new(StructureType.Farm, 4,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 5,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 999, CostStone: 999, CostMetal: 0, CostPetricite: 0, BuildTurns: 2));

        // --- Quarry (4 levels) ---
        // Produces Stone. First Quarry at a Mountain settlement gains +100% Stone (handled in Simulator).
        Add(new(StructureType.Quarry, 1,
            LumberOutput: 0, StoneOutput: 10, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 100, CostStone: 0, CostMetal: 0, CostPetricite: 0, BuildTurns: 1));
        Add(new(StructureType.Quarry, 2,
            LumberOutput: 0, StoneOutput: 20, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 125, CostStone: 0, CostMetal: 20, CostPetricite: 0, BuildTurns: 2));
        Add(new(StructureType.Quarry, 3,
            LumberOutput: 0, StoneOutput: 50, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 500, CostStone: 0, CostMetal: 100, CostPetricite: 4, BuildTurns: 3));
        Add(new(StructureType.Quarry, 4,
            LumberOutput: 0, StoneOutput: 100, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 1750, CostStone: 0, CostMetal: 500, CostPetricite: 15, BuildTurns: 3));

        // --- Barracks (2 levels) ---
        // Allows training of units (not relevant for production sim).
        Add(new(StructureType.Barracks, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 300, CostStone: 150, CostMetal: 0, CostPetricite: 0, BuildTurns: 2));
        Add(new(StructureType.Barracks, 2,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 0, CostStone: 500, CostMetal: 300, CostPetricite: 0, BuildTurns: 3));

        // --- Quartermaster (1 level) ---
        // Grants 25% damage boost to this and neighboring settlements (not relevant for production sim).
        Add(new(StructureType.Quartermaster, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 250, CostStone: 150, CostMetal: 0, CostPetricite: 0, BuildTurns: 3));

        // --- Forge (4 levels) ---
        // Produces Metal. First Forge at a Border settlement gains +100% Metal (handled in Simulator).
        Add(new(StructureType.Forge, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 5, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 100, CostStone: 75, CostMetal: 0, CostPetricite: 0, BuildTurns: 2));
        Add(new(StructureType.Forge, 2,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 10, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 250, CostStone: 150, CostMetal: 0, CostPetricite: 0, BuildTurns: 3));
        Add(new(StructureType.Forge, 3,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 25, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 750, CostStone: 250, CostMetal: 0, CostPetricite: 5, BuildTurns: 4));
        Add(new(StructureType.Forge, 4,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 50, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 1500, CostStone: 1250, CostMetal: 0, CostPetricite: 25, BuildTurns: 4));

        // --- Militia Headquarters (3 levels) ---
        // Spawns units in World Events (not relevant for production sim).
        Add(new(StructureType.MilitiaHeadquarters, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 400, CostStone: 200, CostMetal: 0, CostPetricite: 0, BuildTurns: 3));
        Add(new(StructureType.MilitiaHeadquarters, 2,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 1000, CostStone: 0, CostMetal: 350, CostPetricite: 0, BuildTurns: 4));
        Add(new(StructureType.MilitiaHeadquarters, 3,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 0, CostStone: 0, CostMetal: 750, CostPetricite: 15, BuildTurns: 5));

        // --- Watchtower (3 levels) ---
        // Delays incoming threats (not relevant for production sim).
        Add(new(StructureType.Watchtower, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 400, CostStone: 200, CostMetal: 0, CostPetricite: 0, BuildTurns: 3));
        Add(new(StructureType.Watchtower, 2,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 1000, CostStone: 0, CostMetal: 350, CostPetricite: 0, BuildTurns: 4));
        Add(new(StructureType.Watchtower, 3,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 1600, CostStone: 0, CostMetal: 0, CostPetricite: 20, BuildTurns: 4));

        // --- Shrine of the Veiled Lady (1 level) ---
        // Grants kingdom-wide damage reduction (not relevant for production sim).
        Add(new(StructureType.ShrineOfVeiledLady, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 1000, CostStone: 1000, CostMetal: 1000, CostPetricite: 0, BuildTurns: 5));

        // --- Durand's Workshop (1 level) ---
        // Produces 1 Petricite/turn and grants a 10% reduction in Research costs.
        Add(new(StructureType.DurandsWorkshop, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 1, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 500, CostStone: 1250, CostMetal: 0, CostPetricite: 0, BuildTurns: 5));

        // --- Petricite Mill (3 levels, Capital only) ---
        // Produces Petricite every turn; can only be constructed in the Capital settlement,
        // but multiple Petricite Mills may be built there.
        Add(new(StructureType.PetriciteMill, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 1, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 800, CostStone: 0, CostMetal: 80, CostPetricite: 0, BuildTurns: 5,
            CapitalOnly: true));
        Add(new(StructureType.PetriciteMill, 2,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 2, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 1750, CostStone: 1000, CostMetal: 500, CostPetricite: 0, BuildTurns: 5,
            CapitalOnly: true));
        Add(new(StructureType.PetriciteMill, 3,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 3, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0,
            CostLumber: 7500, CostStone: 3500, CostMetal: 2500, CostPetricite: 0, BuildTurns: 5,
            CapitalOnly: true));

        // --- Marketplace (3 levels) ---
        // Grants % resource bonus to all adjacent (linked) settlements. Effect can stack.
        Add(new(StructureType.Marketplace, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0.05, AcademyMultiplier: 0,
            CostLumber: 1000, CostStone: 0, CostMetal: 500, CostPetricite: 10, BuildTurns: 4));
        Add(new(StructureType.Marketplace, 2,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0.07, AcademyMultiplier: 0,
            CostLumber: 3000, CostStone: 0, CostMetal: 1500, CostPetricite: 20, BuildTurns: 4));
        Add(new(StructureType.Marketplace, 3,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0.10, AcademyMultiplier: 0,
            CostLumber: 12500, CostStone: 0, CostMetal: 5000, CostPetricite: 40, BuildTurns: 4));

        // --- Academy (3 levels) ---
        // Grants % resource bonus to this settlement and all settlements sharing the same
        // environment (Heartland, Mountain, or Border). Effect can stack.
        Add(new(StructureType.Academy, 1,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0.05,
            CostLumber: 0, CostStone: 750, CostMetal: 500, CostPetricite: 10, BuildTurns: 4));
        Add(new(StructureType.Academy, 2,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0.07,
            CostLumber: 0, CostStone: 3000, CostMetal: 1500, CostPetricite: 20, BuildTurns: 4));
        Add(new(StructureType.Academy, 3,
            LumberOutput: 0, StoneOutput: 0, MetalOutput: 0, PetriciteOutput: 0, FoodOutput: 0,
            MarketplaceMultiplier: 0, AcademyMultiplier: 0.10,
            CostLumber: 0, CostStone: 12500, CostMetal: 5000, CostPetricite: 40, BuildTurns: 4));

        return defs;
    }
}
