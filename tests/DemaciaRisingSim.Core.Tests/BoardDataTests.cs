using DemaciaRisingSim.Core;

namespace DemaciaRisingSim.Core.Tests;

public class BoardDataTests
{
    [Fact]
    public void CreateDefaultBoard_Has16Settlements()
    {
        var board = BoardData.CreateDefaultBoard();
        Assert.Equal(16, board.Count);
    }

    [Fact]
    public void CreateDefaultBoard_AllSettlementsHaveCorrectNames()
    {
        var board = BoardData.CreateDefaultBoard();
        var expected = new[]
        {
            "The Great City", "Brookhollow", "Cloudfield", "Dawnhold", "Evenmoor",
            "Fossbarrow", "Hawkstone", "Hayneath", "High Silvermere", "Jandelle",
            "Meltridge", "Pinara", "Terbisia", "Tylburne", "Uwendale", "Vaskasia",
        };
        foreach (var name in expected)
            Assert.True(board.ContainsKey(name), $"Board should contain settlement {name}");
    }

    [Fact]
    public void CreateDefaultBoard_HeartlandSettlements_HaveSixTiles()
    {
        var board = BoardData.CreateDefaultBoard();
        // Heartland settlements: Brookhollow, Hayneath, Jandelle, Vaskasia (6 tiles each)
        foreach (var name in new[] { "Brookhollow", "Hayneath", "Jandelle", "Vaskasia" })
            Assert.Equal(6, board[name].Tiles.Length);
    }

    [Fact]
    public void CreateDefaultBoard_Tylburne_IsHeartlandAndPetricite()
    {
        var board = BoardData.CreateDefaultBoard();
        Assert.True(board["Tylburne"].Terrain.HasFlag(TerrainType.Heartland));
        Assert.True(board["Tylburne"].Terrain.HasFlag(TerrainType.Petricite));
        Assert.Equal(6, board["Tylburne"].Tiles.Length);
    }

    [Fact]
    public void CreateDefaultBoard_PetriciteSettlements_AllowPetricite()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var name in new[] { "The Great City", "Dawnhold", "High Silvermere", "Tylburne" })
            Assert.True(board[name].AllowsPetricite, $"Settlement {name} should allow petricite tiles");
    }

    [Fact]
    public void CreateDefaultBoard_NonPetriciteSettlements_DoNotAllowPetricite()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var name in new[]
        {
            "Brookhollow", "Cloudfield", "Evenmoor", "Fossbarrow", "Hawkstone",
            "Hayneath", "Jandelle", "Meltridge", "Pinara", "Terbisia", "Uwendale", "Vaskasia",
        })
            Assert.False(board[name].AllowsPetricite, $"Settlement {name} should not allow petricite tiles");
    }

    [Fact]
    public void CreateDefaultBoard_MountainSettlements_HaveCorrectTerrain()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var name in new[] { "Evenmoor", "Hawkstone", "Pinara", "Uwendale" })
            Assert.True(board[name].Terrain.HasFlag(TerrainType.Mountain), $"Settlement {name} should be mountain terrain");
    }

    [Fact]
    public void CreateDefaultBoard_BorderSettlements_HaveCorrectTerrain()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var name in new[] { "Cloudfield", "Fossbarrow", "Meltridge", "Terbisia" })
            Assert.True(board[name].Terrain.HasFlag(TerrainType.Border), $"Settlement {name} should be border terrain");
    }

    [Fact]
    public void CreateDefaultBoard_AllMultipliersStartAtOne()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var settlement in board.Values)
            Assert.Equal(1.0, settlement.Multiplier);
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var board = BoardData.CreateDefaultBoard();
        board["The Great City"].Tiles[0] = TileType.Stone;

        var clone = BoardData.Clone(board);
        Assert.Equal(TileType.Stone, clone["The Great City"].Tiles[0]);

        // Mutating original should not affect clone
        board["The Great City"].Tiles[0] = TileType.Metal;
        Assert.Equal(TileType.Stone, clone["The Great City"].Tiles[0]);
    }
}
