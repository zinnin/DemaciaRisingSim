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
        for (int level = 1; level <= 3; level++)
            Assert.True(StructureData.Get(StructureType.PetriciteMill, level).CapitalOnly);
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
    public void SettlementOutput_Heartland_FirstFarmGrantsBonusFood()
    {
        var settlement = new Settlement("X", EnvironmentType.Heartland, slotCount: 2)
        {
            Structures = [new Structure(StructureType.Farm, 1),
                          new Structure(StructureType.Farm, 1)],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        // Farm L1 = 1 Food each; first farm gets +1 Heartland bonus = 3 total
        Assert.Equal(3, output.Food);
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
    public void Score_ZeroLumber_ReturnsZero()
    {
        var output = new ResourceOutput(0, 100, 50, 3);
        Assert.Equal(0, Simulator.Score(output));
    }

    [Fact]
    public void Score_PerfectRatios_IsHigherThanImbalanced()
    {
        double lumberUnits = 10;
        var balanced = new ResourceOutput(
            Lumber:    (int)(lumberUnits * GameConstants.LumberTileValue),
            Stone:     (int)(lumberUnits * GameConstants.StoneRatio     * GameConstants.StoneTileValue),
            Metal:     (int)(lumberUnits * GameConstants.MetalRatio     * GameConstants.MetalTileValue),
            Petricite: (int)(lumberUnits * GameConstants.PetriciteRatio * GameConstants.PetriciteTileValue));

        var allLumber = new ResourceOutput(
            Lumber:    (int)(lumberUnits * 4 * GameConstants.LumberTileValue),
            Stone: 0, Metal: 0, Petricite: 0);

        Assert.True(Simulator.Score(balanced) > Simulator.Score(allLumber));
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
    public void OptimizeBoard_WithDefaultSettings_PreAllocatesRequiredStructures()
    {
        var board = BoardData.CreateDefaultBoard();
        var settings = new SimulationSettings
        {
            RequireDurandsWorkshop  = true,
            RequireShrineOfVeiledLady = true,
            RequireQuartermaster    = true,
        };
        var optimized = Simulator.OptimizeBoard(board, settings);

        Assert.Equal(StructureType.DurandsWorkshop,    optimized["The Great City"].Structures[4].Type);
        Assert.Equal(StructureType.ShrineOfVeiledLady, optimized["The Great City"].Structures[5].Type);
        Assert.Equal(StructureType.Quartermaster,      optimized["High Silvermere"].Structures[4].Type);
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
    public void FullReport_ContainsScoreAndProductionLines()
    {
        var board = BoardData.CreateDefaultBoard();
        string report = Simulator.FullReport(board);
        Assert.Contains("Score:", report);
        Assert.Contains("Total Production:", report);
        Assert.Contains("Food:", report);
    }
}
