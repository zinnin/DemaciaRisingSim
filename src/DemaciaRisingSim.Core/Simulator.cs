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

            // First two Farms in a Heartland settlement each grant +1 Food.
            if (structure.Type == StructureType.Farm &&
                settlement.Environment.HasFlag(EnvironmentType.Heartland))
            {
                heartlandFarmCount++;
                if (heartlandFarmCount <= 2)
                    food += 1;
            }
        }

        // Woodland terrain bonus: 25% increased lumber production.
        // Applied before the academy/marketplace multiplier.
        double lumberMultiplier = settlement.Multiplier;
        if (settlement.Environment.HasFlag(EnvironmentType.Woodland))
            lumberMultiplier *= 1.25;

        // Apply the production multiplier (from Marketplace and Academy structures).
        // Food is NOT multiplied — it is a capacity resource, not a production resource.
        lumber    = (int)Math.Floor(lumber    * lumberMultiplier);
        stone     = (int)Math.Floor(stone     * settlement.Multiplier);
        metal     = (int)Math.Floor(metal     * settlement.Multiplier);
        petricite = (int)Math.Floor(petricite * settlement.Multiplier);

        return new ResourceOutput(lumber, stone, metal, petricite, food);
    }

    /// <summary>
    /// Returns the total number of structure slots that would receive a production buff
    /// if a Marketplace were placed in the given settlement.
    /// A Marketplace buffs all neighboring settlements' slots.
    /// </summary>
    public static int CountMarketplaceInfluence(Settlement settlement) =>
        settlement.Neighbors.Count * GameConstants.SettlementSlotCount;

    /// <summary>
    /// Returns the total number of structure slots that would receive a production buff
    /// if an Academy were placed in the given settlement (including the settlement itself).
    /// An Academy buffs all settlements sharing the same primary environment
    /// (Heartland, Mountain, or Border). Petricite/Woodland-only settlements buff only themselves.
    /// </summary>
    public static int CountAcademyInfluence(Settlement settlement, Dictionary<string, Settlement> board)
    {
        const EnvironmentType PrimaryMask = EnvironmentType.Heartland | EnvironmentType.Mountain | EnvironmentType.Border;
        var sourcePrimary = settlement.Environment & PrimaryMask;
        int count = 0;
        foreach (var target in board.Values)
        {
            bool isSelf    = target.Name == settlement.Name;
            bool sharesEnv = sourcePrimary != EnvironmentType.None &&
                             (target.Environment & sourcePrimary) != EnvironmentType.None;
            if (isSelf || sharesEnv) count++;
        }
        return count * GameConstants.SettlementSlotCount;
    }

    /// <summary>
    /// Calculates total resource production for the entire board,
    /// applying all Marketplace and Academy multiplier effects first.
    /// </summary>
    public static ResourceOutput BoardOutput(Dictionary<string, Settlement> board)
    {
        // Work on a copy so we don't mutate the caller's board.
        var workingBoard = BoardData.Clone(board);
        ApplyMultipliers(workingBoard);

        // Sum up settlement outputs.
        var total = ResourceOutput.Zero;
        foreach (var settlement in workingBoard.Values)
            total += SettlementOutput(settlement);

        return total;
    }

    /// <summary>
    /// Returns the number of turns needed to accumulate <paramref name="target"/> of a resource
    /// given <paramref name="production"/> per turn.
    /// Returns 0 when the target is 0 (already met), and <see cref="int.MaxValue"/> when
    /// production is 0 but the target is non-zero (unreachable).
    /// </summary>
    private static int TurnsForResource(int production, int target)
    {
        if (target <= 0)       return 0;
        if (production <= 0)   return int.MaxValue;
        return (target + production - 1) / production;
    }

    /// <summary>
    /// Calculates the number of turns required to accumulate each resource target given the
    /// board's per-turn output. A value of <see cref="int.MaxValue"/> indicates zero production
    /// for a resource that has a non-zero target.
    /// </summary>
    public static TurnsBreakdown TurnsToComplete(ResourceOutput output, SimulationSettings? settings = null)
    {
        settings ??= new SimulationSettings();
        return new TurnsBreakdown(
            Lumber:    TurnsForResource(output.Lumber,    settings.LumberTarget),
            Stone:     TurnsForResource(output.Stone,     settings.StoneTarget),
            Metal:     TurnsForResource(output.Metal,     settings.MetalTarget),
            Petricite: TurnsForResource(output.Petricite, settings.PetriciteTarget));
    }

    /// <summary>
    /// Generates a single numeric score for a board's resource output given the provided
    /// resource targets. Higher is better.
    /// <para>
    /// Scoring uses two phases so that <see cref="Permutate"/> can make incremental progress
    /// from any board state, including the sparse board produced by <see cref="SmartAllocateBoard"/>:
    /// </para>
    /// <list type="bullet">
    ///   <item><b>Phase 1 — coverage</b> (any targeted resource has zero production):
    ///     returns <c>covered / total</c> in <c>[0, 1)</c>, where <c>covered</c> is the number
    ///     of targeted resources that have at least 1 unit of production.  This provides a
    ///     gradient that rewards adding a new resource type even when others are still missing.</item>
    ///   <item><b>Phase 2 — efficiency</b> (all targeted resources have non-zero production):
    ///     returns <c>1 + 1/maxTurns + 0.01/sumOfTurns</c>.  The <c>1/maxTurns</c> primary term
    ///     strongly prioritises reducing the bottleneck resource — any change that lowers the
    ///     maximum turns is always preferred over any change that doesn't, regardless of how much
    ///     the total improves.  The <c>0.01/sumOfTurns</c> secondary term is a tiebreaker that
    ///     provides a gradient when two arrangements share the same maximum, steering the
    ///     optimizer toward overall balance rather than over-producing non-bottleneck resources.
    ///     The weight 0.01 is provably small enough (ε < 1) that it never overrides the primary
    ///     term: the benefit of reducing maxTurns by 1 always exceeds the benefit of reducing
    ///     sumOfTurns by 1.</item>
    /// </list>
    /// Food is not included in the score.
    /// </summary>
    public static double Score(ResourceOutput output, SimulationSettings? settings = null)
    {
        settings ??= new SimulationSettings();
        var turns    = TurnsToComplete(output, settings);
        int maxTurns = turns.Max;

        // All targets already met (or all targets are 0).
        if (maxTurns == 0) return 1e10;

        // Phase 1: at least one targeted resource has zero production.
        // Return a fractional coverage score in [0, 1) to guide the optimizer toward
        // filling all resource types before worrying about quantities.
        if (maxTurns == int.MaxValue)
        {
            int covered = 0, total = 0;
            if (settings.LumberTarget    > 0) { total++; if (output.Lumber    > 0) covered++; }
            if (settings.StoneTarget     > 0) { total++; if (output.Stone     > 0) covered++; }
            if (settings.MetalTarget     > 0) { total++; if (output.Metal     > 0) covered++; }
            if (settings.PetriciteTarget > 0) { total++; if (output.Petricite > 0) covered++; }
            return total > 0 ? (double)covered / total : 0.0;
        }

        // Phase 2: all resources producing.
        // Primary term 1/maxTurns: strongly prefer reducing the bottleneck resource.
        // Tiebreaker 0.01/sumTurns: when maxTurns ties, prefer balanced (lower total) production.
        // The weight 0.01 is provably safe: for all realistic turn counts,
        //   1/(m*(m-1)) > 0.01/(s*(s-1))  where m = maxTurns, s = sumTurns ≥ m.
        // This guarantees the primary term always dominates.
        long sumTurns = (long)turns.Lumber + turns.Stone + turns.Metal + turns.Petricite;
        return 1.0 + 1.0 / maxTurns + 0.01 / sumTurns;
    }

    /// <summary>Scores a board configuration directly.</summary>
    public static double Score(Dictionary<string, Settlement> board, SimulationSettings? settings = null)
        => Score(BoardOutput(board), settings);

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
        double highScore = Score(current, settings);

        foreach (var settlementName in current.Keys.ToList())
        {
            if (settings.LockedSettlements.Contains(settlementName)) continue;

            var settlement = current[settlementName];
            var candidates = GetCandidates(settlement, settings.MaxBuildingLevel);

            for (int slotIndex = 0; slotIndex < settlement.Structures.Length; slotIndex++)
            {
                if (fixedSlots.Contains((settlementName, slotIndex))) continue;

                var originalStructure = settlement.Structures[slotIndex];

                // Pre-compute which unique structure types already occupy a different slot on the
                // committed board. Checked once per slot (not per candidate) for efficiency.
                var presentUniqueElsewhere = new HashSet<StructureType>();
                foreach (var s in board.Values)
                    for (int i = 0; i < s.Structures.Length; i++)
                    {
                        if (s.Name == settlementName && i == slotIndex) continue;
                        if (UniqueStructureTypes.Contains(s.Structures[i].Type))
                            presentUniqueElsewhere.Add(s.Structures[i].Type);
                    }

                foreach (var candidate in candidates)
                {
                    // Unique structures may appear at most once on the board.
                    if (candidate.Type != StructureType.Empty &&
                        presentUniqueElsewhere.Contains(candidate.Type))
                        continue;

                    settlement.Structures[slotIndex] = candidate;
                    double candidateScore = Score(current, settings);
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
    /// Allocates structures to the board using an influence-driven approach:
    /// <list type="number">
    ///   <item>Places Marketplace or Academy in the settlement where it buffs the most other slots,
    ///         choosing Marketplace when it influences more slots than Academy (or vice-versa),
    ///         and only placing a buff structure where the influenced-slot count exceeds the
    ///         break-even threshold (1 / multiplier_at_max_level).</item>
    ///   <item>Places required structures (Durand's Workshop, Shrine, Quartermaster) in the
    ///         globally lowest-value remaining slots, preferring settlements with few connections
    ///         and low Academy coverage over high-value terrain or capital slots.</item>
    ///   <item>Meets the per-settlement food target by placing Farms in each non-capital
    ///         settlement's lowest-value remaining slots. The capital is exempt because its
    ///         slots are more valuable as PetriciteMills than as farms.</item>
    ///   <item>Leaves all other slots empty for further optimization via <see cref="Permutate"/>.</item>
    /// </list>
    /// </summary>
    /// <param name="board">Source board (not mutated).</param>
    /// <param name="settings">Optimizer settings.</param>
    /// <param name="fixedSlots">Output: the set of slots that must not be changed during subsequent optimization passes.</param>
    public static Dictionary<string, Settlement> SmartAllocateBoard(
        Dictionary<string, Settlement> board,
        SimulationSettings? settings,
        out HashSet<(string, int)> fixedSlots)
    {
        settings   ??= new SimulationSettings();
        fixedSlots   = [];

        var work     = BoardData.Clone(board);
        int maxLevel = settings.MaxBuildingLevel;
        // Buff structures (Academy/Marketplace) max at level 3; clamp accordingly.
        int buffLevel = Math.Min(3, maxLevel);

        // Reset non-locked settlements to empty.
        foreach (var s in work.Values)
            if (!settings.LockedSettlements.Contains(s.Name))
                Array.Fill(s.Structures, Structure.Empty);

        // --- Step 1: Calculate influence counts for each settlement ---
        var mpInfluence   = work.Values.ToDictionary(s => s.Name, s => CountMarketplaceInfluence(s));
        var acadInfluence = work.Values.ToDictionary(s => s.Name, s => CountAcademyInfluence(s, work));

        // --- Step 2: Compute break-even threshold ---
        // A buff structure uses one slot. It pays off when:
        //   multiplier × influenced_slots × avg_slot_value ≥ avg_slot_value
        // → influenced_slots ≥ 1 / multiplier
        double buffMult  = StructureData.Get(StructureType.Academy, buffLevel).AcademyMultiplier;
        double breakEven = 1.0 / buffMult;

        // --- Step 3: Place one buff structure in each settlement where the influence exceeds break-even ---
        // Prefer Marketplace if it buffs more slots; otherwise Academy.
        // "If a settlement doesn't have many connecting nodes, Marketplace is not very valuable,
        //  but potentially Academy is."
        foreach (var s in work.Values.OrderByDescending(s => Math.Max(mpInfluence[s.Name], acadInfluence[s.Name])))
        {
            if (settings.LockedSettlements.Contains(s.Name)) continue;

            int mpInf   = mpInfluence[s.Name];
            int acadInf = acadInfluence[s.Name];

            if (mpInf < breakEven && acadInf < breakEven) continue;

            StructureType buffType = (mpInf >= acadInf && mpInf >= breakEven)
                ? StructureType.Marketplace
                : StructureType.Academy;

            // Place in the first empty slot.
            for (int i = 0; i < s.Structures.Length; i++)
            {
                if (s.Structures[i].Type == StructureType.Empty)
                {
                    s.Structures[i] = new Structure(buffType, buffLevel);
                    fixedSlots.Add((s.Name, i));
                    break;
                }
            }
        }

        // --- Step 4: Apply multipliers from the placed buff structures ---
        ApplyMultipliers(work);

        // --- Step 5: Rank all remaining empty slots by production opportunity cost (ascending) ---
        // Opportunity cost = value of the best production structure that could go in that slot,
        // scaled by the settlement's current multiplier.
        var allSlots = new List<(string Name, int Slot, double Value)>();
        foreach (var s in work.Values)
        {
            if (settings.LockedSettlements.Contains(s.Name)) continue;
            for (int i = 0; i < s.Structures.Length; i++)
            {
                if (s.Structures[i].Type == StructureType.Empty)
                    allSlots.Add((s.Name, i, ComputeSlotValue(s, i, maxLevel, settings)));
            }
        }
        var sortedByValue = allSlots.OrderBy(x => x.Value).ToList();
        var usedSlotSet   = new HashSet<(string, int)>(fixedSlots);

        // --- Step 6: Place required structures in the globally lowest-value slots ---
        // Secondary sort key: connection count ascending, so that required (non-production)
        // structures land in low-connection settlements first.  High-connection settlements
        // benefit more from Marketplace amplification and should be preserved for production.
        var required = new List<Structure>();
        if (settings.RequireDurandsWorkshop)     required.Add(new Structure(StructureType.DurandsWorkshop,    1));
        if (settings.RequireShrineOfVeiledLady)  required.Add(new Structure(StructureType.ShrineOfVeiledLady, 1));
        if (settings.RequireQuartermaster)        required.Add(new Structure(StructureType.Quartermaster,       1));

        var reqSortedSlots = allSlots
            .OrderBy(x => x.Value)
            .ThenBy(x => work.TryGetValue(x.Name, out var s) ? s.Neighbors.Count : 0)
            .ToList();

        int reqIdx = 0;
        foreach (var req in required)
        {
            while (reqIdx < reqSortedSlots.Count && usedSlotSet.Contains((reqSortedSlots[reqIdx].Name, reqSortedSlots[reqIdx].Slot)))
                reqIdx++;
            if (reqIdx >= reqSortedSlots.Count) break;

            var (name, slot, _) = reqSortedSlots[reqIdx++];
            work[name].Structures[slot] = req;
            fixedSlots.Add((name, slot));
            usedSlotSet.Add((name, slot));
        }

        // --- Step 7: Meet the global food target using the fewest possible farm slots ---
        // FoodTargetPerSettlement × eligible-settlement-count gives the total food goal.
        // Farm L4 (maxLevel) is used everywhere so each slot provides the most food possible
        // (5 food in a normal settlement, 6 in Heartland on the first two farms).
        // Farms are placed in the globally cheapest available slots first and stop as soon
        // as the total food across all eligible settlements reaches the target.  This ensures
        // no more farm slots are consumed than necessary, leaving surplus slots for the
        // production structures that actually drive the turn count.
        // The capital is exempt: its slots are reserved for PetriciteMills.
        if (settings.FoodTargetPerSettlement > 0)
        {
            // Farm structures go up to level 4 (unlike buff structures which cap at 3).
            int maxFarmLevel = Math.Min(4, maxLevel);

            // Determine eligible settlements (non-capital, non-locked).
            var eligibleSettlements = work.Values
                .Where(s => !settings.LockedSettlements.Contains(s.Name) && !s.AllowsPetriciteMill)
                .ToList();

            int totalFoodTarget = settings.FoodTargetPerSettlement * eligibleSettlements.Count;

            // Current food already present across all eligible settlements.
            int currentFood = eligibleSettlements.Sum(s => SettlementOutput(s).Food);

            if (currentFood < totalFoodTarget)
            {
                // Cheapest slots globally across all eligible settlements.
                var globalFarmSlots = sortedByValue
                    .Where(sv =>
                    {
                        if (!work.TryGetValue(sv.Name, out var s)) return false;
                        if (settings.LockedSettlements.Contains(s.Name)) return false;
                        if (s.AllowsPetriciteMill) return false;
                        return !usedSlotSet.Contains((sv.Name, sv.Slot));
                    })
                    .ToList();

                foreach (var (name, slot, _) in globalFarmSlots)
                {
                    if (currentFood >= totalFoodTarget) break;

                    var s = work[name];
                    // Capture food before placement so the Heartland bonus on subsequent
                    // farms in the same settlement is computed correctly via SettlementOutput.
                    int foodBefore = SettlementOutput(s).Food;
                    s.Structures[slot] = new Structure(StructureType.Farm, maxFarmLevel);
                    // SettlementOutput re-evaluates the settlement (6 structure slots) so the
                    // Heartland +1 bonus on the first two farms is always counted accurately.
                    currentFood += SettlementOutput(s).Food - foodBefore;
                    fixedSlots.Add((name, slot));
                    usedSlotSet.Add((name, slot));
                }
            }
        }

        return work;
    }

    /// <summary>
    /// Calls <see cref="SmartAllocateBoard(Dictionary{string,Settlement},SimulationSettings?,out HashSet{ValueTuple{string,int}})"/>
    /// and discards the fixed-slots output. Convenient for callers that only need the board.
    /// </summary>
    public static Dictionary<string, Settlement> SmartAllocateBoard(
        Dictionary<string, Settlement> board,
        SimulationSettings? settings = null) => SmartAllocateBoard(board, settings, out _);

    /// <summary>
    /// Uses <see cref="SmartAllocateBoard"/> to create an informed starting layout, then
    /// iteratively calls <see cref="Permutate"/> until no further improvement is found, up to a
    /// maximum of <see cref="GameConstants.MaxOptimizationIterations"/> iterations.
    /// </summary>
    public static Dictionary<string, Settlement> OptimizeBoard(
        Dictionary<string, Settlement> board,
        SimulationSettings? settings = null)
    {
        settings ??= new SimulationSettings();

        var prepared   = SmartAllocateBoard(board, settings, out var fixedSlots);
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
    /// Generates a formatted summary of a board's structure layout, per-turn production output,
    /// and the number of turns required to accumulate each resource target.
    /// </summary>
    public static string FullReport(Dictionary<string, Settlement> board, SimulationSettings? settings = null)
    {
        settings ??= new SimulationSettings();
        var output = BoardOutput(board);
        var turns  = TurnsToComplete(output, settings);
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("Board Layout:");
        foreach (var kv in board)
        {
            var structures = string.Join(", ", kv.Value.Structures.Select(s => s.ToString()));
            sb.AppendLine($"  {kv.Key}: [{structures}]");
        }

        sb.AppendLine();
        sb.AppendLine($"Total Production (per turn): {output}");

        sb.AppendLine();
        sb.AppendLine("Turns to hit targets:");
        sb.AppendLine($"  Lumber:    {TurnsBreakdown.Format(turns.Lumber),6}  (target: {settings.LumberTarget})");
        sb.AppendLine($"  Stone:     {TurnsBreakdown.Format(turns.Stone),6}  (target: {settings.StoneTarget})");
        sb.AppendLine($"  Metal:     {TurnsBreakdown.Format(turns.Metal),6}  (target: {settings.MetalTarget})");
        sb.AppendLine($"  Petricite: {TurnsBreakdown.Format(turns.Petricite),6}  (target: {settings.PetriciteTarget})");
        sb.AppendLine($"  Food:      {output.Food}");
        sb.AppendLine();
        sb.AppendLine($"Max Turns: {TurnsBreakdown.Format(turns.Max)}");

        return sb.ToString();
    }

    // Structure types that may appear at most once on the entire board.
    private static readonly HashSet<StructureType> UniqueStructureTypes =
    [
        StructureType.DurandsWorkshop,
        StructureType.ShrineOfVeiledLady,
        StructureType.Quartermaster,
    ];

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resets all settlement multipliers to 1.0 then applies every Marketplace and
    /// Academy structure on the board. Mutates the provided board in place.
    /// Academy buff targets are derived from shared primary environment (Heartland / Mountain / Border).
    /// Petricite and Woodland are not primary environments for academy grouping; settlements with only
    /// those environments (e.g. The Great City) only buff themselves.
    /// </summary>
    private static void ApplyMultipliers(Dictionary<string, Settlement> board)
    {
        const EnvironmentType PrimaryMask = EnvironmentType.Heartland | EnvironmentType.Mountain | EnvironmentType.Border;

        foreach (var settlement in board.Values)
            settlement.Multiplier = 1.0;

        foreach (var settlement in board.Values)
        {
            foreach (var structure in settlement.Structures)
            {
                if (structure.Type == StructureType.Marketplace)
                {
                    var def = StructureData.Get(structure.Type, structure.Level);
                    foreach (var neighborId in settlement.Neighbors)
                        if (board.TryGetValue(neighborId, out var neighbor))
                            neighbor.Multiplier += def.MarketplaceMultiplier;
                }
                else if (structure.Type == StructureType.Academy)
                {
                    var def = StructureData.Get(structure.Type, structure.Level);
                    var sourcePrimary = settlement.Environment & PrimaryMask;
                    foreach (var target in board.Values)
                    {
                        bool isSelf    = target.Name == settlement.Name;
                        bool sharesEnv = sourcePrimary != EnvironmentType.None &&
                                         (target.Environment & sourcePrimary) != EnvironmentType.None;
                        if (isSelf || sharesEnv)
                            target.Multiplier += def.AcademyMultiplier;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Estimates the production opportunity cost for a slot: the normalised output value
    /// of the best production structure that could be placed in this slot, accounting for
    /// terrain bonuses already claimed by earlier slots and the settlement's multiplier.
    /// <para>
    /// When <paramref name="settings"/> are supplied, each resource is normalised by its
    /// accumulation <em>target</em> (progress per turn per slot) so that scarce / bottleneck
    /// resources — such as Petricite, whose target is much smaller than Lumber — are correctly
    /// ranked as more expensive to give up.  This ensures that required structures and food
    /// farms are placed in the slots of settlements that have the fewest connections and the
    /// least Academy coverage (i.e., where a production structure would contribute least).
    /// Without settings the method falls back to max-production normalisation.
    /// </para>
    /// </summary>
    private static double ComputeSlotValue(Settlement settlement, int slotIndex, int maxLevel,
        SimulationSettings? settings = null)
    {
        int effectiveLevel = Math.Max(1, Math.Min(4, maxLevel));

        // Normalise by resource target (units of "progress per slot per turn") when targets
        // are provided, so bottleneck resources weigh proportionally more than plentiful ones.
        // Fall back to max level-4 production when no targets are set.
        double lumberDenom = settings?.LumberTarget    > 0 ? settings.LumberTarget    : (double)StructureData.Get(StructureType.Lumberyard, 4).LumberOutput;
        double stoneDenom  = settings?.StoneTarget     > 0 ? settings.StoneTarget     : (double)StructureData.Get(StructureType.Quarry,     4).StoneOutput;
        double metalDenom  = settings?.MetalTarget     > 0 ? settings.MetalTarget     : (double)StructureData.Get(StructureType.Forge,      4).MetalOutput;

        // Lumberyard value (always available everywhere)
        double lumberVal = StructureData.Get(StructureType.Lumberyard, effectiveLevel).LumberOutput / lumberDenom;
        if (settlement.Environment.HasFlag(EnvironmentType.Woodland))
            lumberVal *= 1.25;

        // Quarry value: first Quarry in a Mountain settlement earns the terrain double
        bool hasPriorQuarry = settlement.Environment.HasFlag(EnvironmentType.Mountain) &&
                              settlement.Structures.Take(slotIndex).Any(s => s.Type == StructureType.Quarry);
        double stoneVal = StructureData.Get(StructureType.Quarry, effectiveLevel).StoneOutput / stoneDenom;
        if (settlement.Environment.HasFlag(EnvironmentType.Mountain) && !hasPriorQuarry)
            stoneVal *= 2.0;

        // Forge value: first Forge in a Border settlement earns the terrain double
        bool hasPriorForge = settlement.Environment.HasFlag(EnvironmentType.Border) &&
                             settlement.Structures.Take(slotIndex).Any(s => s.Type == StructureType.Forge);
        double metalVal = StructureData.Get(StructureType.Forge, effectiveLevel).MetalOutput / metalDenom;
        if (settlement.Environment.HasFlag(EnvironmentType.Border) && !hasPriorForge)
            metalVal *= 2.0;

        // PetriciteMill value: available only in the capital settlement
        double petriciteVal = 0;
        if (settlement.AllowsPetriciteMill)
        {
            double petDenom = settings?.PetriciteTarget > 0 ? settings.PetriciteTarget : (double)StructureData.Get(StructureType.PetriciteMill, 3).PetriciteOutput;
            int millLevel = Math.Max(1, Math.Min(3, maxLevel));
            petriciteVal = StructureData.Get(StructureType.PetriciteMill, millLevel).PetriciteOutput / petDenom;
        }

        double best = Math.Max(lumberVal, Math.Max(stoneVal, Math.Max(metalVal, petriciteVal)));
        return best * settlement.Multiplier;
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
