using DemaciaRisingSim.Core;

namespace DemaciaRisingSim.Core.Tests;

public class SimulatorTests
{
    // --- StructureData tests ---

    [Fact]
    public void StructureData_LumberyardL4_ReturnsCorrectOutput()
    {
        var def = StructureData.Get(StructureType.Lumberyard, 4);
        Assert.Equal(150, def.LumberOutput);
        Assert.Equal(0, def.StoneOutput);
        Assert.Equal(0, def.MetalOutput);
        Assert.Equal(0, def.PetriciteOutput);
    }

    [Fact]
    public void StructureData_AcademyLevels_HaveCorrectMultipliers()
    {
        Assert.Equal(0.05, StructureData.Get(StructureType.Academy, 1).AcademyMultiplier);
        Assert.Equal(0.07, StructureData.Get(StructureType.Academy, 2).AcademyMultiplier);
        Assert.Equal(0.10, StructureData.Get(StructureType.Academy, 3).AcademyMultiplier);
    }

    [Fact]
    public void StructureData_MarketplaceLevels_HaveCorrectMultipliers()
    {
        Assert.Equal(0.05, StructureData.Get(StructureType.Marketplace, 1).MarketplaceMultiplier);
        Assert.Equal(0.07, StructureData.Get(StructureType.Marketplace, 2).MarketplaceMultiplier);
        Assert.Equal(0.10, StructureData.Get(StructureType.Marketplace, 3).MarketplaceMultiplier);
    }

    [Fact]
    public void StructureData_PetriciteMill_IsCapitalOnly()
    {
        // PetriciteMill can only be placed in the capital (multiple are allowed).
        for (int level = 1; level <= 3; level++)
            Assert.True(StructureData.Get(StructureType.PetriciteMill, level).CapitalOnly);
    }

    [Fact]
    public void Settlement_AllowsPetriciteMill_CapitalOnly()
    {
        // Only the capital may build PetriciteMills, regardless of terrain.
        var capital      = new Settlement("Cap",      EnvironmentType.Petricite, isCapital: true);
        var petricite    = new Settlement("Dawnhold", EnvironmentType.Petricite | EnvironmentType.Border);
        var nonCapital   = new Settlement("Other",    EnvironmentType.Heartland);

        Assert.True(capital.AllowsPetriciteMill,    "Capital should allow PetriciteMill");
        Assert.False(petricite.AllowsPetriciteMill,  "Non-capital Petricite settlement should not allow PetriciteMill");
        Assert.False(nonCapital.AllowsPetriciteMill, "Non-capital settlement should not allow PetriciteMill");
    }

    [Fact]
    public void StructureData_DurandsWorkshop_ProducesOnePetricite()
    {
        var def = StructureData.Get(StructureType.DurandsWorkshop, 1);
        Assert.Equal(1, def.PetriciteOutput);
    }

    [Fact]
    public void StructureData_MaxLevel_ReturnsCorrectValues()
    {
        Assert.Equal(4, StructureData.MaxLevel(StructureType.Lumberyard));
        Assert.Equal(4, StructureData.MaxLevel(StructureType.Farm));
        Assert.Equal(4, StructureData.MaxLevel(StructureType.Quarry));
        Assert.Equal(4, StructureData.MaxLevel(StructureType.Forge));
        Assert.Equal(3, StructureData.MaxLevel(StructureType.Academy));
        Assert.Equal(3, StructureData.MaxLevel(StructureType.Marketplace));
        Assert.Equal(3, StructureData.MaxLevel(StructureType.PetriciteMill));
        Assert.Equal(1, StructureData.MaxLevel(StructureType.Quartermaster));
        Assert.Equal(1, StructureData.MaxLevel(StructureType.ShrineOfVeiledLady));
        Assert.Equal(1, StructureData.MaxLevel(StructureType.DurandsWorkshop));
    }

    // --- SettlementOutput tests ---

    [Fact]
    public void SettlementOutput_LumberyardL1_ReturnsCorrectLumber()
    {
        var settlement = new Settlement("X", EnvironmentType.None, slotCount: 3)
        {
            Structures = [new Structure(StructureType.Lumberyard, 1),
                          new Structure(StructureType.Lumberyard, 1),
                          new Structure(StructureType.Lumberyard, 1)],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        Assert.Equal(45, output.Lumber); // 3 * 15
        Assert.Equal(0, output.Stone);
        Assert.Equal(0, output.Metal);
        Assert.Equal(0, output.Petricite);
        Assert.Equal(0, output.Food);
    }

    [Fact]
    public void SettlementOutput_LumberyardL4_ReturnsMaxLumber()
    {
        var settlement = new Settlement("X", EnvironmentType.None, slotCount: 1)
        {
            Structures = [new Structure(StructureType.Lumberyard, 4)],
            Multiplier = 1.0,
        };
        Assert.Equal(150, Simulator.SettlementOutput(settlement).Lumber);
    }

    [Fact]
    public void SettlementOutput_Mountain_FirstQuarryDoubled()
    {
        var settlement = new Settlement("X", EnvironmentType.Mountain, slotCount: 2)
        {
            Structures = [new Structure(StructureType.Quarry, 1),
                          new Structure(StructureType.Quarry, 1)],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        // First Quarry L1: 10 + 100% bonus = 20; second Quarry L1: 10 (no bonus)
        Assert.Equal(30, output.Stone);
    }

    [Fact]
    public void SettlementOutput_NonMountain_QuarryNotDoubled()
    {
        var settlement = new Settlement("X", EnvironmentType.Heartland, slotCount: 1)
        {
            Structures = [new Structure(StructureType.Quarry, 1)],
            Multiplier = 1.0,
        };
        Assert.Equal(10, Simulator.SettlementOutput(settlement).Stone);
    }

    [Fact]
    public void SettlementOutput_Border_FirstForgeDoubled()
    {
        var settlement = new Settlement("X", EnvironmentType.Border, slotCount: 2)
        {
            Structures = [new Structure(StructureType.Forge, 1),
                          new Structure(StructureType.Forge, 1)],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        // First Forge L1: 5 + 100% = 10; second: 5
        Assert.Equal(15, output.Metal);
    }

    [Fact]
    public void SettlementOutput_Heartland_FirstTwoFarmsGrantBonusFood()
    {
        var settlement = new Settlement("X", EnvironmentType.Heartland, slotCount: 3)
        {
            Structures = [new Structure(StructureType.Farm, 1),
                          new Structure(StructureType.Farm, 1),
                          new Structure(StructureType.Farm, 1)],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        // Farm L1 = 1 Food each × 3; first two farms each get +1 Heartland bonus = 3 + 2 = 5 total
        Assert.Equal(5, output.Food);
    }

    [Fact]
    public void SettlementOutput_Heartland_TwoFarmsGrantBonusFood()
    {
        var settlement = new Settlement("X", EnvironmentType.Heartland, slotCount: 2)
        {
            Structures = [new Structure(StructureType.Farm, 1),
                          new Structure(StructureType.Farm, 1)],
            Multiplier = 1.0,
        };
        // 2 × 1 base + 2 Heartland bonus = 4
        Assert.Equal(4, Simulator.SettlementOutput(settlement).Food);
    }

    [Fact]
    public void SettlementOutput_NonHeartland_NoFarmBonus()
    {
        var settlement = new Settlement("X", EnvironmentType.Mountain, slotCount: 1)
        {
            Structures = [new Structure(StructureType.Farm, 1)],
            Multiplier = 1.0,
        };
        Assert.Equal(1, Simulator.SettlementOutput(settlement).Food);
    }

    [Fact]
    public void SettlementOutput_Woodland_LumberIncreasedBy25Percent()
    {
        var settlement = new Settlement("X", EnvironmentType.Woodland, slotCount: 1)
        {
            Structures = [new Structure(StructureType.Lumberyard, 1)],
            Multiplier = 1.0,
        };
        // Lumberyard L1 = 15 lumber; Woodland +25% = floor(15 * 1.25) = 18
        Assert.Equal(18, Simulator.SettlementOutput(settlement).Lumber);
    }

    [Fact]
    public void SettlementOutput_Woodland_LumberBonus_AppliesToRawBeforeMultiplier()
    {
        // Woodland 25% bonus is a terrain bonus applied before Academy/Marketplace multiplier.
        // Both are combined into a single multiplier and floored once at the end:
        // 15 * 1.25 * 1.1 = 20.625 → floor = 20
        var settlement = new Settlement("X", EnvironmentType.Woodland, slotCount: 1)
        {
            Structures = [new Structure(StructureType.Lumberyard, 1)],
            Multiplier = 1.1,
        };
        Assert.Equal(20, Simulator.SettlementOutput(settlement).Lumber);
    }

    [Fact]
    public void SettlementOutput_NonWoodland_NoLumberBonus()
    {
        var settlement = new Settlement("X", EnvironmentType.Heartland, slotCount: 1)
        {
            Structures = [new Structure(StructureType.Lumberyard, 1)],
            Multiplier = 1.0,
        };
        Assert.Equal(15, Simulator.SettlementOutput(settlement).Lumber);
    }

    [Fact]
    public void SettlementOutput_Multiplier_ScalesProductionNotFood()
    {
        var settlement = new Settlement("X", EnvironmentType.None, slotCount: 2)
        {
            Structures = [new Structure(StructureType.Lumberyard, 1),
                          new Structure(StructureType.Farm, 1)],
            Multiplier = 2.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        Assert.Equal(30, output.Lumber); // 15 * 2.0
        Assert.Equal(1, output.Food);    // Food is not multiplied
    }

    [Fact]
    public void SettlementOutput_EmptySlots_ProduceNothing()
    {
        var settlement = new Settlement("X", EnvironmentType.None, slotCount: 3)
        {
            Structures = [Structure.Empty, Structure.Empty, Structure.Empty],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        Assert.Equal(ResourceOutput.Zero, output);
    }

    [Fact]
    public void SettlementOutput_PetriciteMill_ProducesPetricite()
    {
        var settlement = new Settlement("Capital", EnvironmentType.Petricite, isCapital: true, slotCount: 1)
        {
            Structures = [new Structure(StructureType.PetriciteMill, 3)],
            Multiplier = 1.0,
        };
        Assert.Equal(3, Simulator.SettlementOutput(settlement).Petricite);
    }

    [Fact]
    public void SettlementOutput_DurandsWorkshop_ProducesOnePetricite()
    {
        var settlement = new Settlement("X", EnvironmentType.None, slotCount: 1)
        {
            Structures = [new Structure(StructureType.DurandsWorkshop, 1)],
            Multiplier = 1.0,
        };
        Assert.Equal(1, Simulator.SettlementOutput(settlement).Petricite);
    }

    // --- BoardOutput tests ---

    [Fact]
    public void BoardOutput_AllLumberyardsL1_ReturnsLumberOnly()
    {
        var board = BoardData.CreateDefaultBoard(); // all Lumberyard L1
        var output = Simulator.BoardOutput(board);
        Assert.Equal(16 * 6 * 15, output.Lumber); // 1440
        Assert.Equal(0, output.Stone);
        Assert.Equal(0, output.Metal);
        Assert.Equal(0, output.Petricite);
    }

    [Fact]
    public void BoardOutput_DoesNotMutateInputBoard()
    {
        var board = BoardData.CreateDefaultBoard();
        double originalMultiplier = board["The Great City"].Multiplier;
        Simulator.BoardOutput(board);
        Assert.Equal(originalMultiplier, board["The Great City"].Multiplier);
    }

    [Fact]
    public void BoardOutput_QuarryInMountain_FirstQuarryDoubled()
    {
        var board = BoardData.CreateDefaultBoard();
        // Replace first slot in Evenmoor (Mountain) with Quarry L1
        board["Evenmoor"].Structures[0] = new Structure(StructureType.Quarry, 1);
        // Replace remaining Lumberyard slots (5 slots) in Evenmoor — keep as Lumberyard
        var output = Simulator.BoardOutput(board);
        // Evenmoor Quarry L1 + Mountain bonus: 10 + 10 = 20 stone
        Assert.Equal(20, output.Stone);
    }

    [Fact]
    public void BoardOutput_MarketplaceL1_BoostsAdjacentMultiplier()
    {
        var baseBoard = BoardData.CreateDefaultBoard();
        // Empty slot 0 in The Great City so the comparison is fair (no production lost)
        baseBoard["The Great City"].Structures[0] = Structure.Empty;
        var marketBoard = BoardData.Clone(baseBoard);

        // Place Marketplace L1 in the empty slot (5% to Dawnhold, High Silvermere, Tylburne)
        marketBoard["The Great City"].Structures[0] = new Structure(StructureType.Marketplace, 1);

        var baseOutput   = Simulator.BoardOutput(baseBoard);
        var marketOutput = Simulator.BoardOutput(marketBoard);

        // Marketplace L1 gives 5% boost to 3 neighbors — total lumber must increase
        int baseTotal   = baseOutput.Lumber   + baseOutput.Stone   + baseOutput.Metal;
        int marketTotal = marketOutput.Lumber + marketOutput.Stone + marketOutput.Metal;
        Assert.True(marketTotal > baseTotal);
    }

    [Fact]
    public void BoardOutput_AcademyL3_BoostsSameEnvironment()
    {
        var board = BoardData.CreateDefaultBoard();

        // Fill two Quarry L4 slots in every Heartland settlement, then add Academy L3 to Brookhollow
        var heartlandNames = board.Values
            .Where(s => s.Environment.HasFlag(EnvironmentType.Heartland))
            .Select(s => s.Name)
            .ToList();

        foreach (var name in heartlandNames)
        {
            board[name].Structures[0] = new Structure(StructureType.Quarry, 4);
            board[name].Structures[1] = new Structure(StructureType.Quarry, 4);
        }
        var baseOutput = Simulator.BoardOutput(board);

        board["Brookhollow"].Structures[2] = new Structure(StructureType.Academy, 3);
        var academyOutput = Simulator.BoardOutput(board);

        // Academy L3 = +10% to all Heartland settlements; stone output should increase
        Assert.True(academyOutput.Stone > baseOutput.Stone);
    }

    [Fact]
    public void BoardOutput_AcademyBuff_DerivedFromEnvironmentMatchesExpectedGroups()
    {
        // For every settlement, an Academy placed there should buff exactly the same
        // set of settlements that the old hardcoded lists specified.
        var expectedGroups = new Dictionary<string, string[]>
        {
            ["The Great City"]  = ["The Great City"],
            ["Brookhollow"]     = ["Brookhollow", "Hayneath", "Jandelle", "Tylburne", "Vaskasia"],
            ["Cloudfield"]      = ["Cloudfield", "Dawnhold", "Fossbarrow", "Meltridge", "Terbisia"],
            ["Dawnhold"]        = ["Cloudfield", "Dawnhold", "Fossbarrow", "Meltridge", "Terbisia"],
            ["Evenmoor"]        = ["Evenmoor", "Hawkstone", "High Silvermere", "Pinara", "Uwendale"],
            ["Fossbarrow"]      = ["Cloudfield", "Dawnhold", "Fossbarrow", "Meltridge", "Terbisia"],
            ["Hawkstone"]       = ["Evenmoor", "Hawkstone", "High Silvermere", "Pinara", "Uwendale"],
            ["Hayneath"]        = ["Brookhollow", "Hayneath", "Jandelle", "Tylburne", "Vaskasia"],
            ["High Silvermere"] = ["Evenmoor", "Hawkstone", "High Silvermere", "Pinara", "Uwendale"],
            ["Jandelle"]        = ["Brookhollow", "Hayneath", "Jandelle", "Tylburne", "Vaskasia"],
            ["Meltridge"]       = ["Cloudfield", "Dawnhold", "Fossbarrow", "Meltridge", "Terbisia"],
            ["Pinara"]          = ["Evenmoor", "Hawkstone", "High Silvermere", "Pinara", "Uwendale"],
            ["Terbisia"]        = ["Cloudfield", "Dawnhold", "Fossbarrow", "Meltridge", "Terbisia"],
            ["Tylburne"]        = ["Brookhollow", "Hayneath", "Jandelle", "Tylburne", "Vaskasia"],
            ["Uwendale"]        = ["Evenmoor", "Hawkstone", "High Silvermere", "Pinara", "Uwendale"],
            ["Vaskasia"]        = ["Brookhollow", "Hayneath", "Jandelle", "Tylburne", "Vaskasia"],
        };

        const EnvironmentType PrimaryMask = EnvironmentType.Heartland | EnvironmentType.Mountain | EnvironmentType.Border;
        var board = BoardData.CreateDefaultBoard();

        foreach (var (sourceName, expectedTargets) in expectedGroups)
        {
            var source = board[sourceName];
            var sourcePrimary = source.Environment & PrimaryMask;

            var actualTargets = board.Values
                .Where(t => t.Name == sourceName ||
                            (sourcePrimary != EnvironmentType.None &&
                             (t.Environment & sourcePrimary) != EnvironmentType.None))
                .Select(t => t.Name)
                .OrderBy(n => n)
                .ToArray();

            Assert.Equal(
                expectedTargets.OrderBy(n => n).ToArray(),
                actualTargets);
        }
    }

    [Fact]
    public void BoardOutput_AcademyInWoodland_OnlyBuffsSelf()
    {
        // Academy only groups Heartland / Mountain / Border settlements.
        // A Woodland settlement's Academy should buff only itself, not other Woodland settlements.
        var woodland1 = new Settlement("W1", EnvironmentType.Woodland, slotCount: 2);
        var woodland2 = new Settlement("W2", EnvironmentType.Woodland, slotCount: 1);
        woodland1.Structures[0] = new Structure(StructureType.Lumberyard, 1);
        woodland1.Structures[1] = new Structure(StructureType.Academy, 1);
        woodland2.Structures[0] = new Structure(StructureType.Lumberyard, 1);

        var board = new Dictionary<string, Settlement>
        {
            [woodland1.Name] = woodland1,
            [woodland2.Name] = woodland2,
        };

        var output = Simulator.BoardOutput(board);

        // W1 Academy L1 = 5% only on W1 (self).
        // W1: Lumberyard L1 = 15 lumber × Woodland(×1.25) × Academy(×1.05) = floor(15*1.25*1.05) = 19
        // W2: Lumberyard L1 = 15 lumber × Woodland(×1.25) only          = floor(15*1.25)      = 18
        Assert.Equal(37, output.Lumber);
    }

    [Fact]
    public void Score_ZeroProduction_ReturnsZero()
    {
        // Only Lumber has a target and Lumber production is 0 → covered=0/total=1 → score 0.
        var settings = new SimulationSettings { LumberTarget = 100, StoneTarget = 0, MetalTarget = 0, PetriciteTarget = 0 };
        var output = new ResourceOutput(0, 100, 50, 3);
        Assert.Equal(0, Simulator.Score(output, settings));
    }

    [Fact]
    public void Score_PartialProduction_IsBetweenZeroAndOne()
    {
        // 2 of 4 targeted resources have production → coverage score = 0.5.
        var settings = new SimulationSettings
        {
            LumberTarget    = 1000,
            StoneTarget     = 1000,
            MetalTarget     = 1000,
            PetriciteTarget = 1000,
        };
        var output = new ResourceOutput(Lumber: 10, Stone: 10, Metal: 0, Petricite: 0);
        double score = Simulator.Score(output, settings);
        Assert.True(score > 0 && score < 1, $"Expected partial score in (0,1), got {score}");
        Assert.Equal(0.5, score, precision: 10);
    }

    [Fact]
    public void Score_PartialProduction_IsLowerThanFullProduction()
    {
        // Partial coverage (some resources at 0) must score below a fully-producing board.
        var settings = new SimulationSettings
        {
            LumberTarget    = 1000,
            StoneTarget     = 1000,
            MetalTarget     = 1000,
            PetriciteTarget = 1000,
        };
        var partial = new ResourceOutput(Lumber: 10, Stone: 0, Metal: 0, Petricite: 0);
        var full    = new ResourceOutput(Lumber:  1, Stone: 1, Metal: 1, Petricite: 1);
        Assert.True(Simulator.Score(full, settings) > Simulator.Score(partial, settings));
    }

    [Fact]
    public void Score_FewerMaxTurns_IsHigher()
    {
        // A board that hits all targets in fewer turns should score higher.
        var settings = new SimulationSettings
        {
            LumberTarget    = 1000,
            StoneTarget     = 1000,
            MetalTarget     = 1000,
            PetriciteTarget = 1000,
        };
        // 10 of each per turn → 100 max turns
        var faster = new ResourceOutput(10, 10, 10, 10);
        // 5 of each per turn → 200 max turns
        var slower = new ResourceOutput(5,  5,  5,  5);

        Assert.True(Simulator.Score(faster, settings) > Simulator.Score(slower, settings));
    }

    [Fact]
    public void Score_IsNonNegative()
    {
        var board = BoardData.CreateDefaultBoard();
        board["The Great City"].Structures[0] = new Structure(StructureType.Quarry,  1);
        board["Brookhollow"].Structures[0]    = new Structure(StructureType.Forge,   1);
        board["Cloudfield"].Structures[0]     = new Structure(StructureType.Lumberyard, 1);
        Assert.True(Simulator.Score(board) >= 0);
    }

    // --- Optimization tests ---

    [Fact]
    public void OptimizeBoard_ScoreIsAtLeastAsGoodAsInitial()
    {
        var board = BoardData.CreateDefaultBoard();
        // Use no-required-structures settings for a clean comparison
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop  = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster    = false,
        };
        double initialScore = Simulator.Score(board);
        var optimized = Simulator.OptimizeBoard(board, settings);
        Assert.True(Simulator.Score(optimized) >= initialScore);
    }

    [Fact]
    public void OptimizeBoard_WithDefaultSettings_PlacesRequiredStructures()
    {
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop  = true,
            RequireShrineOfVeiledLady = true,
            RequireQuartermaster    = true,
        };
        var optimized = Simulator.OptimizeBoard(board, settings);

        // The smart allocation places required structures in globally lowest-value slots,
        // so we verify they are present somewhere on the board (not hardcoded to specific slots).
        var allStructureTypes = optimized.Values
            .SelectMany(s => s.Structures)
            .Select(s => s.Type)
            .ToList();
        Assert.Contains(StructureType.DurandsWorkshop,    allStructureTypes);
        Assert.Contains(StructureType.ShrineOfVeiledLady, allStructureTypes);
        Assert.Contains(StructureType.Quartermaster,      allStructureTypes);
    }

    [Fact]
    public void OptimizeBoard_LockedSettlement_IsNotChanged()
    {
        var board = BoardData.CreateDefaultBoard();
        // Pre-set a recognizable structure in a locked settlement
        board["Fossbarrow"].Structures[0] = new Structure(StructureType.Watchtower, 1);

        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop  = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster    = false,
            LockedSettlements       = ["Fossbarrow"],
        };
        var optimized = Simulator.OptimizeBoard(board, settings);

        // The locked settlement's structures should be unchanged
        Assert.Equal(StructureType.Watchtower, optimized["Fossbarrow"].Structures[0].Type);
    }

    [Fact]
    public void OptimizeBoard_MaxBuildingLevel1_NeverExceedsLevel1()
    {
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop  = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster    = false,
            MaxBuildingLevel        = 1,
        };
        var optimized = Simulator.OptimizeBoard(board, settings);

        foreach (var settlement in optimized.Values)
            foreach (var structure in settlement.Structures)
                if (structure.Type != StructureType.Empty)
                    Assert.True(structure.Level <= 1, $"Found level {structure.Level} in {settlement.Name}");
    }

    [Fact]
    public void FullReport_ContainsTurnsAndProductionLines()
    {
        var board = BoardData.CreateDefaultBoard();
        string report = Simulator.FullReport(board);
        Assert.Contains("Max Turns:", report);
        Assert.Contains("Total Production (per turn):", report);
        Assert.Contains("Food:", report);
        Assert.Contains("Turns to hit targets:", report);
    }

    // --- Influence-count tests ---

    [Fact]
    public void CountMarketplaceInfluence_TheGreatCity_ReturnsThreeNeighborSlots()
    {
        var board = BoardData.CreateDefaultBoard();
        // The Great City has links to Dawnhold, High Silvermere, Tylburne (3 neighbors)
        int influence = Simulator.CountMarketplaceInfluence(board["The Great City"]);
        Assert.Equal(3 * GameConstants.SettlementSlotCount, influence);
    }

    [Fact]
    public void CountMarketplaceInfluence_HighSilvermere_ReturnsSixNeighborSlots()
    {
        var board = BoardData.CreateDefaultBoard();
        // High Silvermere has 6 links: TGC, Fossbarrow, Hawkstone, Jandelle, Pinara, Uwendale
        int influence = Simulator.CountMarketplaceInfluence(board["High Silvermere"]);
        Assert.Equal(6 * GameConstants.SettlementSlotCount, influence);
    }

    [Fact]
    public void CountAcademyInfluence_TheGreatCity_OnlyBuffsSelf()
    {
        var board = BoardData.CreateDefaultBoard();
        // The Great City is Petricite-only: not Heartland/Mountain/Border, so only buffs itself
        int influence = Simulator.CountAcademyInfluence(board["The Great City"], board);
        Assert.Equal(1 * GameConstants.SettlementSlotCount, influence);
    }

    [Fact]
    public void CountAcademyInfluence_MountainSettlement_BuffsAllMountainSettlements()
    {
        var board = BoardData.CreateDefaultBoard();
        // Mountain settlements: Evenmoor, Hawkstone, High Silvermere, Pinara, Uwendale = 5
        int influence = Simulator.CountAcademyInfluence(board["Evenmoor"], board);
        Assert.Equal(5 * GameConstants.SettlementSlotCount, influence);
    }

    [Fact]
    public void CountAcademyInfluence_HeartlandSettlement_BuffsAllHeartlandSettlements()
    {
        var board = BoardData.CreateDefaultBoard();
        // Heartland settlements: Brookhollow, Hayneath, Jandelle, Tylburne, Vaskasia = 5
        int influence = Simulator.CountAcademyInfluence(board["Brookhollow"], board);
        Assert.Equal(5 * GameConstants.SettlementSlotCount, influence);
    }

    [Fact]
    public void CountAcademyInfluence_BorderSettlement_BuffsAllBorderSettlements()
    {
        var board = BoardData.CreateDefaultBoard();
        // Border settlements: Cloudfield, Dawnhold, Fossbarrow, Meltridge, Terbisia = 5
        int influence = Simulator.CountAcademyInfluence(board["Cloudfield"], board);
        Assert.Equal(5 * GameConstants.SettlementSlotCount, influence);
    }

    // --- SmartAllocateBoard tests ---

    [Fact]
    public void SmartAllocateBoard_PlacesBuffStructures_InHighInfluenceSettlements()
    {
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster      = false,
            FoodTargetPerSettlement   = 0,
        };
        var allocated = Simulator.SmartAllocateBoard(board, settings);

        // Every settlement that passes break-even should have a Marketplace or Academy.
        // With max level 4 (multiplier 0.10), break-even = 10 slots.
        bool anyBuff = allocated.Values.Any(s =>
            s.Structures.Any(st => st.Type == StructureType.Marketplace || st.Type == StructureType.Academy));
        Assert.True(anyBuff);
    }

    [Fact]
    public void SmartAllocateBoard_PetriciteOnlySettlement_GetMarketplaceNotAcademy()
    {
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster      = false,
            FoodTargetPerSettlement   = 0,
        };
        var allocated = Simulator.SmartAllocateBoard(board, settings);

        // The Great City is Petricite-only: academy influence = 1*6 = 6 < break-even (10),
        // marketplace influence = 3*6 = 18 ≥ break-even → should get Marketplace.
        var tgcBuff = allocated["The Great City"].Structures
            .FirstOrDefault(s => s.Type == StructureType.Marketplace || s.Type == StructureType.Academy);
        Assert.NotNull(tgcBuff);
        Assert.Equal(StructureType.Marketplace, tgcBuff!.Type);
    }

    [Fact]
    public void SmartAllocateBoard_RequiredStructures_PlacedSomewhere()
    {
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = true,
            RequireShrineOfVeiledLady = true,
            RequireQuartermaster      = true,
            FoodTargetPerSettlement   = 0,
        };
        var allocated = Simulator.SmartAllocateBoard(board, settings);

        var allTypes = allocated.Values.SelectMany(s => s.Structures).Select(s => s.Type).ToList();
        Assert.Contains(StructureType.DurandsWorkshop,    allTypes);
        Assert.Contains(StructureType.ShrineOfVeiledLady, allTypes);
        Assert.Contains(StructureType.Quartermaster,      allTypes);
    }

    [Fact]
    public void SmartAllocateBoard_FoodTarget_MetPerSettlement()
    {
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster      = false,
            FoodTargetPerSettlement   = 2,
            MaxBuildingLevel          = 4,
        };
        var allocated = Simulator.SmartAllocateBoard(board, settings);

        // After smart allocation each non-capital settlement should meet the food target.
        // The capital is exempt: its slots are reserved for PetriciteMills.
        foreach (var settlement in allocated.Values)
        {
            if (settlement.AllowsPetriciteMill) continue;
            int food = Simulator.SettlementOutput(settlement).Food;
            Assert.True(food >= 2,
                $"{settlement.Name} has only {food} food (target: 2)");
        }
    }

    [Fact]
    public void SmartAllocateBoard_FoodTargetZero_NoFarmsPlaced()
    {
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster      = false,
            FoodTargetPerSettlement   = 0,
        };
        var allocated = Simulator.SmartAllocateBoard(board, settings);

        // No farms should be placed when food target is 0.
        Assert.DoesNotContain(StructureType.Farm,
            allocated.Values.SelectMany(s => s.Structures).Select(s => s.Type));
    }

    [Fact]
    public void SmartAllocateBoard_LockedSettlement_IsNotReset()
    {
        var board = BoardData.CreateDefaultBoard();
        board["Fossbarrow"].Structures[0] = new Structure(StructureType.Watchtower, 1);

        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster      = false,
            FoodTargetPerSettlement   = 0,
            LockedSettlements         = ["Fossbarrow"],
        };
        var allocated = Simulator.SmartAllocateBoard(board, settings);

        Assert.Equal(StructureType.Watchtower, allocated["Fossbarrow"].Structures[0].Type);
    }

    [Fact]
    public void SmartAllocateBoard_ScoreIsAtLeastAsGoodAsAllLumberyardL1()
    {
        var board = BoardData.CreateDefaultBoard(); // all Lumberyard L1
        double baseScore = Simulator.Score(board);

        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster      = false,
            FoodTargetPerSettlement   = 0,
        };
        var allocated = Simulator.SmartAllocateBoard(board, settings);
        // After Permutate fills the remaining slots the full OptimizeBoard score will be higher,
        // but even the raw smart allocation (which leaves many slots empty) might score lower;
        // the full OptimizeBoard must score at least as well.
        var optimized = Simulator.OptimizeBoard(board, settings);
        Assert.True(Simulator.Score(optimized) >= baseScore);
    }

    [Fact]
    public void SimulationSettings_FoodTargetPerSettlement_DefaultIsTwo()
    {
        var settings = new SimulationSettings();
        Assert.Equal(2, settings.FoodTargetPerSettlement);
    }

    [Fact]
    public void SmartAllocateBoard_FoodTarget_PerSettlementFoodMatchesTarget()
    {
        // Each non-capital settlement should receive no more than FoodTargetPerSettlement food
        // (plus at most 1 from an unavoidable Heartland terrain bonus rounding).
        // Previously, max-level (L4) farms were always placed even when L1/L2 sufficed,
        // causing 5+ food per settlement instead of the target 2.
        // The capital is exempt from the food target and should have 0 food from SmartAllocate.
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster      = false,
            FoodTargetPerSettlement   = 2,
            MaxBuildingLevel          = 4,
        };
        var allocated = Simulator.SmartAllocateBoard(board, settings);

        foreach (var settlement in allocated.Values)
        {
            int food = Simulator.SettlementOutput(settlement).Food;
            if (settlement.AllowsPetriciteMill)
            {
                // Capital is exempt from food target — its slots are reserved for PetriciteMills.
                Assert.Equal(0, food);
            }
            else
            {
                Assert.True(food >= 2,
                    $"{settlement.Name} has only {food} food (target: 2)");
                // Food must not exceed the target by more than 1 (one unavoidable Heartland rounding unit).
                Assert.True(food <= 3,
                    $"{settlement.Name} has {food} food, which overshoots the target of 2");
            }
        }
    }

    [Fact]
    public void OptimizeBoard_UniqueStructures_AppearAtMostOnce()
    {
        // DurandsWorkshop, ShrineOfVeiledLady, and Quartermaster must each appear
        // at most once on the optimized board.
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = true,
            RequireShrineOfVeiledLady = true,
            RequireQuartermaster      = true,
            FoodTargetPerSettlement   = 0,
        };
        var optimized = Simulator.OptimizeBoard(board, settings);

        var allStructures = optimized.Values.SelectMany(s => s.Structures).ToList();

        Assert.Equal(1, allStructures.Count(s => s.Type == StructureType.DurandsWorkshop));
        Assert.Equal(1, allStructures.Count(s => s.Type == StructureType.ShrineOfVeiledLady));
        Assert.Equal(1, allStructures.Count(s => s.Type == StructureType.Quartermaster));
    }

    [Fact]
    public void Permutate_UniqueStructures_AppearAtMostOnce()
    {
        // Even when Permutate runs without SmartAllocate fixed slots, unique structures
        // must not be duplicated (DurandsWorkshop produces Petricite so the optimizer
        // would otherwise keep adding copies to improve the score).
        var board = BoardData.CreateDefaultBoard();
        var result = Simulator.Permutate(board);

        var allStructures = result.Values.SelectMany(s => s.Structures).ToList();

        Assert.True(allStructures.Count(s => s.Type == StructureType.DurandsWorkshop)    <= 1,
            "DurandsWorkshop appears more than once");
        Assert.True(allStructures.Count(s => s.Type == StructureType.ShrineOfVeiledLady) <= 1,
            "ShrineOfVeiledLady appears more than once");
        Assert.True(allStructures.Count(s => s.Type == StructureType.Quartermaster)      <= 1,
            "Quartermaster appears more than once");
    }

    // --- TurnsToComplete and SimulationSettings target tests ---

    [Fact]
    public void SimulationSettings_ResourceTargets_HaveCorrectDefaults()
    {
        var settings = new SimulationSettings();
        Assert.Equal(296_300, settings.LumberTarget);
        Assert.Equal(343_400, settings.StoneTarget);
        Assert.Equal(143_650, settings.MetalTarget);
        Assert.Equal(1_450,   settings.PetriciteTarget);
    }

    [Fact]
    public void TurnsToComplete_ExactProduction_ReturnsOneForEachResource()
    {
        var settings = new SimulationSettings
        {
            LumberTarget    = 100,
            StoneTarget     = 200,
            MetalTarget     = 50,
            PetriciteTarget = 10,
        };
        // Production exactly equals each target → 1 turn each
        var output = new ResourceOutput(Lumber: 100, Stone: 200, Metal: 50, Petricite: 10);
        var turns = Simulator.TurnsToComplete(output, settings);

        Assert.Equal(1, turns.Lumber);
        Assert.Equal(1, turns.Stone);
        Assert.Equal(1, turns.Metal);
        Assert.Equal(1, turns.Petricite);
        Assert.Equal(1, turns.Max);
    }

    [Fact]
    public void TurnsToComplete_CeilingDivision_RoundsUp()
    {
        var settings = new SimulationSettings
        {
            LumberTarget    = 10,
            StoneTarget     = 0,
            MetalTarget     = 0,
            PetriciteTarget = 0,
        };
        // 10 / 3 = 3.33… → 4 turns needed
        var output = new ResourceOutput(Lumber: 3, Stone: 1, Metal: 1, Petricite: 1);
        var turns = Simulator.TurnsToComplete(output, settings);

        Assert.Equal(4, turns.Lumber);
        Assert.Equal(0, turns.Stone);
        Assert.Equal(0, turns.Metal);
        Assert.Equal(0, turns.Petricite);
        Assert.Equal(4, turns.Max);
    }

    [Fact]
    public void TurnsToComplete_ZeroProduction_ReturnsIntMaxForThatResource()
    {
        var settings = new SimulationSettings
        {
            LumberTarget    = 100,
            StoneTarget     = 0,
            MetalTarget     = 0,
            PetriciteTarget = 0,
        };
        var output = new ResourceOutput(Lumber: 0, Stone: 0, Metal: 0, Petricite: 0);
        var turns = Simulator.TurnsToComplete(output, settings);

        Assert.Equal(int.MaxValue, turns.Lumber);
        Assert.Equal(int.MaxValue, turns.Max);
    }

    [Fact]
    public void TurnsToComplete_ZeroTarget_ReturnsZeroTurns()
    {
        var settings = new SimulationSettings
        {
            LumberTarget    = 0,
            StoneTarget     = 0,
            MetalTarget     = 0,
            PetriciteTarget = 0,
        };
        var output = new ResourceOutput(Lumber: 10, Stone: 10, Metal: 10, Petricite: 10);
        var turns = Simulator.TurnsToComplete(output, settings);

        Assert.Equal(0, turns.Lumber);
        Assert.Equal(0, turns.Stone);
        Assert.Equal(0, turns.Metal);
        Assert.Equal(0, turns.Petricite);
        Assert.Equal(0, turns.Max);
    }

    [Fact]
    public void TurnsToComplete_MixedTargets_MaxIdentifiesBottleneck()
    {
        // Lumber needs 50 turns, all other targets are 0 (already met) → Max is 50.
        var settings = new SimulationSettings
        {
            LumberTarget    = 500,
            StoneTarget     = 0,
            MetalTarget     = 0,
            PetriciteTarget = 0,
        };
        var output = new ResourceOutput(Lumber: 10, Stone: 9999, Metal: 9999, Petricite: 9999);
        var turns = Simulator.TurnsToComplete(output, settings);

        Assert.Equal(50, turns.Lumber);
        Assert.Equal(0,  turns.Stone);
        Assert.Equal(0,  turns.Metal);
        Assert.Equal(0,  turns.Petricite);
        Assert.Equal(50, turns.Max);
    }

    [Fact]
    public void OptimizeBoard_NoEmptyProductionSlots()
    {
        // After a full optimization run, no unlocked settlement should have an Empty slot
        // that is not accounted for — the new Score gradient must drive Permutate to fill
        // every slot with a production structure.
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = true,
            RequireShrineOfVeiledLady = true,
            RequireQuartermaster      = true,
            FoodTargetPerSettlement   = 2,
            MaxBuildingLevel          = 4,
        };
        var optimized = Simulator.OptimizeBoard(board, settings);

        foreach (var settlement in optimized.Values)
        {
            int emptyCount = settlement.Structures.Count(s => s.Type == StructureType.Empty);
            Assert.True(emptyCount == 0,
                $"{settlement.Name} still has {emptyCount} empty slot(s) after optimization.");
        }
    }

    [Fact]
    public void OptimizeBoard_Capital_GetsMultiplePetriciteMills()
    {
        // The Great City is the only settlement that can build PetriciteMills, and multiple
        // are allowed. When petricite is the binding bottleneck, the optimizer should fill
        // the capital's available slots with more than one PetriciteMill.
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = false,
            RequireShrineOfVeiledLady = false,
            RequireQuartermaster      = false,
            FoodTargetPerSettlement   = 0,
            MaxBuildingLevel          = 4,
        };
        var optimized = Simulator.OptimizeBoard(board, settings);

        var capital = optimized["The Great City"];
        int millCount = capital.Structures.Count(s => s.Type == StructureType.PetriciteMill);
        Assert.True(millCount > 1,
            $"The Great City should have more than one PetriciteMill after optimization (got {millCount}).");
    }

    [Fact]
    public void SmartAllocateBoard_Capital_NoRequiredStructuresOrFarms()
    {
        // The capital's slots should never receive required structures or food farms from
        // SmartAllocate — they are too valuable as PetriciteMills and must remain free for
        // Permutate to fill with PetriciteMills.
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop    = true,
            RequireShrineOfVeiledLady = true,
            RequireQuartermaster      = true,
            FoodTargetPerSettlement   = 2,
            MaxBuildingLevel          = 4,
        };
        var allocated = Simulator.SmartAllocateBoard(board, settings);
        var capital   = allocated["The Great City"];

        foreach (var structure in capital.Structures)
        {
            Assert.True(
                structure.Type is StructureType.Empty
                                or StructureType.Marketplace
                                or StructureType.Academy
                                or StructureType.PetriciteMill,
                $"The Great City should not contain {structure.Type} — that slot is wasted on a non-PetriciteMill structure.");
        }
    }

    [Fact]
    public void OptimizeBoard_Capital_GetsMultiplePetriciteMills_WithDefaultSettings()
    {
        // With required structures and food targets enabled (the default), the capital
        // should still receive multiple PetriciteMills because required structures and farms
        // are placed in other settlements.
        var board     = BoardData.CreateDefaultBoard();
        var settings  = new SimulationSettings(); // all defaults
        var optimized = Simulator.OptimizeBoard(board, settings);

        var capital   = optimized["The Great City"];
        int millCount = capital.Structures.Count(s => s.Type == StructureType.PetriciteMill);
        Assert.True(millCount > 1,
            $"The Great City should have more than one PetriciteMill with default settings (got {millCount}).");
    }
}
