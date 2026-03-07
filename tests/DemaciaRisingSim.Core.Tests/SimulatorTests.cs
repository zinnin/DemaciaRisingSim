using DemaciaRisingSim.Core;

namespace DemaciaRisingSim.Core.Tests;

public class SimulatorTests
{
    // --- SettlementOutput tests ---

    [Fact]
    public void SettlementOutput_AllLumber_ReturnsCorrectLumber()
    {
        var settlement = new Settlement("X", TerrainType.None, [], 3)
        {
            Tiles = [TileType.Lumber, TileType.Lumber, TileType.Lumber],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        Assert.Equal(3 * GameConstants.LumberTileValue, output.Lumber);
        Assert.Equal(0, output.Stone);
        Assert.Equal(0, output.Metal);
        Assert.Equal(0, output.Petricite);
    }

    [Fact]
    public void SettlementOutput_Mountain_GrantsStoneBonusTiles()
    {
        var settlement = new Settlement("X", TerrainType.Mountain, [], 1)
        {
            Tiles = [TileType.Stone],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        // 1 stone tile + 2 bonus tiles from mountain = 3 * StoneTileValue
        Assert.Equal(3 * GameConstants.StoneTileValue, output.Stone);
    }

    [Fact]
    public void SettlementOutput_Border_GrantsMetalBonusTiles()
    {
        var settlement = new Settlement("X", TerrainType.Border, [], 1)
        {
            Tiles = [TileType.Metal],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        // 1 metal tile + 2 bonus tiles from border = 3 * MetalTileValue
        Assert.Equal(3 * GameConstants.MetalTileValue, output.Metal);
    }

    [Fact]
    public void SettlementOutput_Heartland_AppliesLumberBonus()
    {
        var settlement = new Settlement("X", TerrainType.Heartland, [], 2)
        {
            Tiles = [TileType.Lumber, TileType.Lumber],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        // Heartland gives +0.25 lumber multiplier: 2 * 150 * 1.25 = 375
        int expected = (int)Math.Floor(2 * GameConstants.LumberTileValue * (1.0 + GameConstants.HeartlandLumberBonus));
        Assert.Equal(expected, output.Lumber);
    }

    [Fact]
    public void SettlementOutput_Petricite_IsOnlyAllowedTileTypeForPetriciteOutput()
    {
        var settlement = new Settlement("X", TerrainType.Petricite, [], 2)
        {
            Tiles = [TileType.Petricite, TileType.Petricite],
            Multiplier = 1.0,
        };
        var output = Simulator.SettlementOutput(settlement);
        Assert.Equal(2 * GameConstants.PetriciteTileValue, output.Petricite);
    }

    [Fact]
    public void SettlementOutput_Multiplier_ScalesProduction()
    {
        var settlement = new Settlement("X", TerrainType.None, [], 2)
        {
            Tiles = [TileType.Lumber, TileType.Stone],
            Multiplier = 1.2,
        };
        var output = Simulator.SettlementOutput(settlement);
        Assert.Equal((int)Math.Floor(GameConstants.LumberTileValue * 1.2), output.Lumber);
        Assert.Equal((int)Math.Floor(GameConstants.StoneTileValue * 1.2), output.Stone);
    }

    // --- BoardOutput tests ---

    [Fact]
    public void BoardOutput_AllLumber_ReturnsNonZeroLumber()
    {
        var board = BoardData.CreateDefaultBoard();
        // All tiles default to Lumber (0)
        var output = Simulator.BoardOutput(board);
        Assert.True(output.Lumber > 0);
        // Mountain settlements get stone bonus tiles even without stone tiles placed
        Assert.True(output.Stone > 0);
        // Border settlements get metal bonus tiles even without metal tiles placed
        Assert.True(output.Metal > 0);
        Assert.Equal(0, output.Petricite);
    }

    [Fact]
    public void BoardOutput_DoesNotMutateInputBoard()
    {
        var board = BoardData.CreateDefaultBoard();
        double originalMultiplier = board["The Great City"].Multiplier;

        Simulator.BoardOutput(board);

        // Input board should be unchanged
        Assert.Equal(originalMultiplier, board["The Great City"].Multiplier);
    }

    [Fact]
    public void BoardOutput_MarketplaceTile_BoostsNeighborMultipliers()
    {
        var baseBoard = BoardData.CreateDefaultBoard();
        var marketBoard = BoardData.Clone(baseBoard);

        // Add a marketplace to The Great City (neighbors: Dawnhold, High Silvermere, Tylburne)
        marketBoard["The Great City"].Tiles[0] = TileType.Marketplace;

        var baseOutput = Simulator.BoardOutput(baseBoard);
        var marketOutput = Simulator.BoardOutput(marketBoard);

        // Marketplace should increase total output (neighbors get +10% multiplier)
        Assert.True(marketOutput.Lumber + marketOutput.Stone + marketOutput.Metal + marketOutput.Petricite >
                    baseOutput.Lumber + baseOutput.Stone + baseOutput.Metal + baseOutput.Petricite);
    }

    // --- Score tests ---

    [Fact]
    public void Score_ZeroLumber_ReturnsZero()
    {
        var output = new ResourceOutput(0, 100, 50, 3);
        Assert.Equal(0, Simulator.Score(output));
    }

    [Fact]
    public void Score_PerfectRatios_IsHigherThanImbalanced()
    {
        // Perfect-ish ratios: stone=1.25x lumber, metal=1.5x lumber, petricite=0.3x lumber
        double lumberUnits = 10;
        var balanced = new ResourceOutput(
            Lumber: (int)(lumberUnits * GameConstants.LumberTileValue),
            Stone: (int)(lumberUnits * GameConstants.StoneRatio * GameConstants.StoneTileValue),
            Metal: (int)(lumberUnits * GameConstants.MetalRatio * GameConstants.MetalTileValue),
            Petricite: (int)(lumberUnits * GameConstants.PetriciteRatio * GameConstants.PetriciteTileValue));

        var allLumber = new ResourceOutput(
            Lumber: (int)(lumberUnits * 4 * GameConstants.LumberTileValue),
            Stone: 0,
            Metal: 0,
            Petricite: 0);

        Assert.True(Simulator.Score(balanced) > Simulator.Score(allLumber));
    }

    [Fact]
    public void Score_IsBetweenZeroAndPositive()
    {
        var board = BoardData.CreateDefaultBoard();
        board["The Great City"].Tiles[0] = TileType.Stone;
        board["Brookhollow"].Tiles[0] = TileType.Metal;
        board["Cloudfield"].Tiles[0] = TileType.Lumber;

        double score = Simulator.Score(board);
        Assert.True(score >= 0);
    }

    // --- Optimization tests ---

    [Fact]
    public void OptimizeBoard_ScoreIsAtLeastAsGoodAsInitial()
    {
        var board = BoardData.CreateDefaultBoard();
        double initialScore = Simulator.Score(board);
        var optimized = Simulator.OptimizeBoard(board);
        double optimizedScore = Simulator.Score(optimized);
        Assert.True(optimizedScore >= initialScore);
    }

    [Fact]
    public void FullReport_ContainsScoreLine()
    {
        var board = BoardData.CreateDefaultBoard();
        string report = Simulator.FullReport(board);
        Assert.Contains("Score:", report);
        Assert.Contains("Total Production:", report);
    }
}
