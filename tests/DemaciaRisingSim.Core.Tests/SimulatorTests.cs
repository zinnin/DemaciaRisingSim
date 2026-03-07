using DemaciaRisingSim.Core;

namespace DemaciaRisingSim.Core.Tests;

public class SimulatorTests
{
    // --- CityOutput tests ---

    [Fact]
    public void CityOutput_AllLumber_ReturnsCorrectLumber()
    {
        var city = new City("X", TerrainType.None, [], [], 3)
        {
            Tiles = [TileType.Lumber, TileType.Lumber, TileType.Lumber],
            Multiplier = 1.0,
        };
        var output = Simulator.CityOutput(city);
        Assert.Equal(3 * GameConstants.LumberTileValue, output.Lumber);
        Assert.Equal(0, output.Stone);
        Assert.Equal(0, output.Metal);
        Assert.Equal(0, output.Petricite);
    }

    [Fact]
    public void CityOutput_Mountain_GrantsStoneBonusTiles()
    {
        var city = new City("X", TerrainType.Mountain, [], [], 1)
        {
            Tiles = [TileType.Stone],
            Multiplier = 1.0,
        };
        var output = Simulator.CityOutput(city);
        // 1 stone tile + 2 bonus tiles from mountain = 3 * StoneTileValue
        Assert.Equal(3 * GameConstants.StoneTileValue, output.Stone);
    }

    [Fact]
    public void CityOutput_Border_GrantsMetalBonusTiles()
    {
        var city = new City("X", TerrainType.Border, [], [], 1)
        {
            Tiles = [TileType.Metal],
            Multiplier = 1.0,
        };
        var output = Simulator.CityOutput(city);
        // 1 metal tile + 2 bonus tiles from border = 3 * MetalTileValue
        Assert.Equal(3 * GameConstants.MetalTileValue, output.Metal);
    }

    [Fact]
    public void CityOutput_Heartland_AppliesLumberBonus()
    {
        var city = new City("X", TerrainType.Heartland, [], [], 2)
        {
            Tiles = [TileType.Lumber, TileType.Lumber],
            Multiplier = 1.0,
        };
        var output = Simulator.CityOutput(city);
        // Heartland gives +0.25 lumber multiplier: 2 * 150 * 1.25 = 375
        int expected = (int)Math.Floor(2 * GameConstants.LumberTileValue * (1.0 + GameConstants.HeartlandLumberBonus));
        Assert.Equal(expected, output.Lumber);
    }

    [Fact]
    public void CityOutput_Petricite_IsOnlyAllowedTileTypeForPetriciteOutput()
    {
        var city = new City("X", TerrainType.Petricite, [], [], 2)
        {
            Tiles = [TileType.Petricite, TileType.Petricite],
            Multiplier = 1.0,
        };
        var output = Simulator.CityOutput(city);
        Assert.Equal(2 * GameConstants.PetriciteTileValue, output.Petricite);
    }

    [Fact]
    public void CityOutput_Multiplier_ScalesProduction()
    {
        var city = new City("X", TerrainType.None, [], [], 2)
        {
            Tiles = [TileType.Lumber, TileType.Stone],
            Multiplier = 1.2,
        };
        var output = Simulator.CityOutput(city);
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
        // Mountain cities get stone bonus tiles even without stone tiles placed
        Assert.True(output.Stone > 0);
        // Border cities get metal bonus tiles even without metal tiles placed
        Assert.True(output.Metal > 0);
        Assert.Equal(0, output.Petricite);
    }

    [Fact]
    public void BoardOutput_DoesNotMutateInputBoard()
    {
        var board = BoardData.CreateDefaultBoard();
        double originalMultiplier = board["A"].Multiplier;

        Simulator.BoardOutput(board);

        // Input board should be unchanged
        Assert.Equal(originalMultiplier, board["A"].Multiplier);
    }

    [Fact]
    public void BoardOutput_MarketplaceTile_BoostsNeighborMultipliers()
    {
        var baseBoard = BoardData.CreateDefaultBoard();
        var marketBoard = BoardData.Clone(baseBoard);

        // Add a marketplace to city A (neighbors: D, I, N)
        marketBoard["A"].Tiles[0] = TileType.Marketplace;

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
        board["A"].Tiles[0] = TileType.Stone;
        board["B"].Tiles[0] = TileType.Metal;
        board["C"].Tiles[0] = TileType.Lumber;

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
