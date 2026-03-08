namespace DemaciaRisingSim.Core;

/// <summary>
/// Calculates resource production and scores for board configurations,
/// and provides optimization routines to find high-scoring structure arrangements.
/// </summary>
public static class Simulator
{
    /// <summary>
    /// Calculates the resource output for a single settlement after applying terrain bonuses.
    /// Note: multipliers must already be set on the settlement before calling this method.
    /// </summary>
    public static ResourceOutput SettlementOutput(Settlement settlement)
    {
        int lumber = 0, stone = 0, metal = 0, petricite = 0, food = 0;
        bool firstQuarryBonus = false;
        bool firstForgeBonus = false;
        int heartlandFarmCount = 0;

        foreach (var structure in settlement.Structures)
        {
            if (structure.Type == StructureType.Empty) continue;

            var def = StructureData.Get(structure.Type, structure.Level);
            lumber    += def.LumberOutput;
            stone     += def.StoneOutput;
            metal     += def.MetalOutput;
            petricite += def.PetriciteOutput;
            food      += def.FoodOutput;

            // First Quarry in a Mountain settlement: +100% Stone from that Quarry.
            if (structure.Type == StructureType.Quarry &&
                settlement.Environment.HasFlag(EnvironmentType.Mountain) &&
                !firstQuarryBonus)
            {
                stone += def.StoneOutput;
                firstQuarryBonus = true;
            }

            // First Forge in a Border settlement: +100% Metal from that Forge.
            if (structure.Type == StructureType.Forge &&
                settlement.Environment.HasFlag(EnvironmentType.Border) &&
                !firstForgeBonus)
            {
                metal += def.MetalOutput;
                firstForgeBonus = true;
            }

            // First Farm in a Heartland settlement: +1 Food (Heartland bonus).
            if (structure.Type == StructureType.Farm &&
                settlement.Environment.HasFlag(EnvironmentType.Heartland))
            {
                heartlandFarmCount++;
                if (heartlandFarmCount == 1)
                    food += 1;
            }
        }

        // Apply the production multiplier (from Marketplace and Academy structures).
        // Food is NOT multiplied — it is a capacity resource, not a production resource.
        lumber    = (int)Math.Floor(lumber    * settlement.Multiplier);
        stone     = (int)Math.Floor(stone     * settlement.Multiplier);
        metal     = (int)Math.Floor(metal     * settlement.Multiplier);
        petricite = (int)Math.Floor(petricite * settlement.Multiplier);

        return new ResourceOutput(lumber, stone, metal, petricite, food);
    }

    /// <summary>
    /// Calculates total resource production for the entire board,
    /// applying all Marketplace and Academy multiplier effects first.
    /// </summary>
    public static ResourceOutput BoardOutput(Dictionary<string, Settlement> board)
    {
        // Work on a copy so we don't mutate the caller's board.
        var workingBoard = BoardData.Clone(board);

        // Reset all multipliers to base.
        foreach (var settlement in workingBoard.Values)
            settlement.Multiplier = 1.0;

        // Apply Marketplace and Academy multipliers (level-aware).
        foreach (var settlement in workingBoard.Values)
        {
            foreach (var structure in settlement.Structures)
            {
                if (structure.Type == StructureType.Marketplace)
                {
                    var def = StructureData.Get(structure.Type, structure.Level);
                    foreach (var neighborId in settlement.Neighbors)
                        if (workingBoard.TryGetValue(neighborId, out var neighbor))
                            neighbor.Multiplier += def.MarketplaceMultiplier;
                }
                else if (structure.Type == StructureType.Academy)
                {
                    var def = StructureData.Get(structure.Type, structure.Level);
                    foreach (var buffedId in settlement.AcademyBuff)
                        if (workingBoard.TryGetValue(buffedId, out var buffed))
                            buffed.Multiplier += def.AcademyMultiplier;
                }
            }
        }

        // Sum up settlement outputs.
        var total = ResourceOutput.Zero;
        foreach (var settlement in workingBoard.Values)
            total += SettlementOutput(settlement);

        return total;
    }

    /// <summary>
    /// Generates a single numeric score for a board's resource output.
    /// Higher is better. Rewards balanced production matching the ideal ratios.
    /// Food is not included in the score.
    /// </summary>
    public static double Score(ResourceOutput output)
    {
        if (output.Lumber == 0) return 0;

        double realLumber    = (double)output.Lumber    / GameConstants.LumberTileValue;
        double realStone     = (double)output.Stone     / GameConstants.StoneTileValue;
        double realMetal     = (double)output.Metal     / GameConstants.MetalTileValue;
        double realPetricite = (double)output.Petricite / GameConstants.PetriciteTileValue;

        double stoneRelative     = realStone     / realLumber;
        double metalRelative     = realMetal     / realLumber;
        double petriciteRelative = realPetricite / realLumber;

        double ratioPenalty = Math.Exp(-1.0 * (
            Math.Pow((Math.Abs(stoneRelative     - GameConstants.StoneRatio)     / GameConstants.StoneRatio),     2) +
            Math.Pow((Math.Abs(metalRelative     - GameConstants.MetalRatio)     / GameConstants.MetalRatio),     2) +
            Math.Pow((Math.Abs(petriciteRelative - GameConstants.PetriciteRatio) / GameConstants.PetriciteRatio), 2)));

        return (realLumber + realStone + realMetal + realPetricite) * ratioPenalty;
    }

    /// <summary>Scores a board configuration directly.</summary>
    public static double Score(Dictionary<string, Settlement> board) => Score(BoardOutput(board));

    /// <summary>
    /// Performs one pass over every optimizable slot on the board, trying each valid
    /// structure type and level, keeping any change that improves the score.
    /// </summary>
    public static Dictionary<string, Settlement> Permutate(
        Dictionary<string, Settlement> board,
        SimulationSettings? settings = null,
        HashSet<(string settlementName, int slotIndex)>? fixedSlots = null)
    {
        settings   ??= new SimulationSettings();
        fixedSlots ??= [];

        var current   = BoardData.Clone(board);
        double highScore = Score(current);

        foreach (var settlementName in current.Keys.ToList())
        {
            if (settings.LockedSettlements.Contains(settlementName)) continue;

            var settlement = current[settlementName];
            var candidates = GetCandidates(settlement, settings.MaxBuildingLevel);

            for (int slotIndex = 0; slotIndex < settlement.Structures.Length; slotIndex++)
            {
                if (fixedSlots.Contains((settlementName, slotIndex))) continue;

                var originalStructure = settlement.Structures[slotIndex];

                foreach (var candidate in candidates)
                {
                    settlement.Structures[slotIndex] = candidate;
                    double candidateScore = Score(current);
                    if (candidateScore > highScore)
                    {
                        highScore = candidateScore;
                        board = BoardData.Clone(current);
                    }
                    else
                    {
                        settlement.Structures[slotIndex] = originalStructure;
                        current    = BoardData.Clone(board);
                        settlement = current[settlementName];
                    }
                }
            }
        }

        return board;
    }

    /// <summary>
    /// Iteratively calls Permutate until no further improvement is found, up to a
    /// maximum of <see cref="GameConstants.MaxOptimizationIterations"/> iterations.
    /// Required structures from <paramref name="settings"/> are pre-allocated before
    /// the first pass and are not changed during optimization.
    /// </summary>
    public static Dictionary<string, Settlement> OptimizeBoard(
        Dictionary<string, Settlement> board,
        SimulationSettings? settings = null)
    {
        settings ??= new SimulationSettings();

        var prepared   = BoardData.Clone(board);
        var fixedSlots = PreAllocateRequired(prepared, settings);

        var solution   = Permutate(prepared, settings, fixedSlots);
        int iterations = GameConstants.MaxOptimizationIterations;

        while (iterations-- > 0 && !BoardsEqual(solution, prepared))
        {
            prepared = BoardData.Clone(solution);
            solution = Permutate(solution, settings, fixedSlots);
        }

        return solution;
    }

    /// <summary>
    /// Generates a formatted summary of a board's structure layout and production output.
    /// </summary>
    public static string FullReport(Dictionary<string, Settlement> board)
    {
        var output = BoardOutput(board);
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("Board Layout:");
        foreach (var kv in board)
        {
            var structures = string.Join(", ", kv.Value.Structures.Select(s => s.ToString()));
            sb.AppendLine($"  {kv.Key}: [{structures}]");
        }

        sb.AppendLine();
        sb.AppendLine($"Total Production: {output}");
        sb.AppendLine();

        double realLumber    = (double)output.Lumber    / GameConstants.LumberTileValue;
        double realStone     = (double)output.Stone     / GameConstants.StoneTileValue;
        double realMetal     = (double)output.Metal     / GameConstants.MetalTileValue;
        double realPetricite = (double)output.Petricite / GameConstants.PetriciteTileValue;

        sb.AppendLine("Adjusted Output (in structure-equivalents):");
        sb.AppendLine($"  Lumber:    {realLumber:F4}");
        sb.AppendLine($"  Stone:     {realStone:F4}");
        sb.AppendLine($"  Metal:     {realMetal:F4}");
        sb.AppendLine($"  Petricite: {realPetricite:F4}");
        sb.AppendLine($"  Food:      {output.Food}");

        if (realLumber > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Ratios (relative to lumber):");
            sb.AppendLine($"  Stone:     {realStone / realLumber:F4} (target: {GameConstants.StoneRatio})");
            sb.AppendLine($"  Metal:     {realMetal / realLumber:F4} (target: {GameConstants.MetalRatio})");
            sb.AppendLine($"  Petricite: {realPetricite / realLumber:F4} (target: {GameConstants.PetriciteRatio})");
        }

        sb.AppendLine();
        sb.AppendLine($"Score: {Score(output):F6}");

        return sb.ToString();
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Pre-places required structures and returns the set of slots that must not
    /// be changed during optimization.
    /// </summary>
    private static HashSet<(string, int)> PreAllocateRequired(
        Dictionary<string, Settlement> board,
        SimulationSettings settings)
    {
        var fixedSlots = new HashSet<(string, int)>();

        // Slots 4 and 5 in The Great City are reserved for required non-production structures
        const int CapitalSlotA = 4;
        const int CapitalSlotB = 5;
        const int QuartermasterSlot = 4;

        if ((settings.RequireDurandsWorkshop || settings.RequireShrineOfVeiledLady) &&
            board.TryGetValue("The Great City", out var tgc))
        {
            if (settings.RequireDurandsWorkshop)
            {
                tgc.Structures[CapitalSlotA] = new Structure(StructureType.DurandsWorkshop, 1);
                fixedSlots.Add(("The Great City", CapitalSlotA));
            }
            if (settings.RequireShrineOfVeiledLady)
            {
                tgc.Structures[CapitalSlotB] = new Structure(StructureType.ShrineOfVeiledLady, 1);
                fixedSlots.Add(("The Great City", CapitalSlotB));
            }
        }

        if (settings.RequireQuartermaster && board.TryGetValue("High Silvermere", out var hs))
        {
            hs.Structures[QuartermasterSlot] = new Structure(StructureType.Quartermaster, 1);
            fixedSlots.Add(("High Silvermere", QuartermasterSlot));
        }

        return fixedSlots;
    }

    /// <summary>
    /// Returns the list of candidate structures the optimizer may try in a given
    /// settlement slot. Only production-relevant structures are included.
    /// </summary>
    private static IEnumerable<Structure> GetCandidates(Settlement settlement, int maxLevel)
    {
        yield return Structure.Empty;

        int l4Max = Math.Min(4, maxLevel);
        int l3Max = Math.Min(3, maxLevel);

        // Structures available in every settlement
        for (int level = 1; level <= l4Max; level++)
        {
            yield return new Structure(StructureType.Lumberyard, level);
            yield return new Structure(StructureType.Quarry,     level);
            yield return new Structure(StructureType.Forge,      level);
            yield return new Structure(StructureType.Farm,       level);
        }

        for (int level = 1; level <= l3Max; level++)
        {
            yield return new Structure(StructureType.Academy,     level);
            yield return new Structure(StructureType.Marketplace, level);
        }

        // Durand's Workshop produces 1 Petricite/turn — valid everywhere
        yield return new Structure(StructureType.DurandsWorkshop, 1);

        // Petricite Mill is restricted to the capital
        if (settlement.AllowsPetriciteMill)
        {
            for (int level = 1; level <= l3Max; level++)
                yield return new Structure(StructureType.PetriciteMill, level);
        }
    }

    /// <summary>Checks whether two boards have identical structure assignments.</summary>
    private static bool BoardsEqual(Dictionary<string, Settlement> a, Dictionary<string, Settlement> b)
    {
        if (a.Count != b.Count) return false;
        foreach (var kv in a)
        {
            if (!b.TryGetValue(kv.Key, out var bSettlement)) return false;
            if (!kv.Value.Structures.SequenceEqual(bSettlement.Structures)) return false;
        }
        return true;
    }
}
