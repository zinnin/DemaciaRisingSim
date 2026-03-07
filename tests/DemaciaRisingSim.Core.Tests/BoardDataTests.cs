using DemaciaRisingSim.Core;

namespace DemaciaRisingSim.Core.Tests;

public class BoardDataTests
{
    [Fact]
    public void CreateDefaultBoard_Has16Cities()
    {
        var board = BoardData.CreateDefaultBoard();
        Assert.Equal(16, board.Count);
    }

    [Fact]
    public void CreateDefaultBoard_AllCitiesHaveCorrectIds()
    {
        var board = BoardData.CreateDefaultBoard();
        var expected = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P" };
        foreach (var id in expected)
            Assert.True(board.ContainsKey(id), $"Board should contain city {id}");
    }

    [Fact]
    public void CreateDefaultBoard_HeartlandCities_HaveSixTiles()
    {
        var board = BoardData.CreateDefaultBoard();
        // Heartland cities: B, H, J, N, P (6 tiles each)
        foreach (var id in new[] { "B", "H", "J", "P" })
            Assert.Equal(6, board[id].Tiles.Length);
    }

    [Fact]
    public void CreateDefaultBoard_N_IsHeartlandAndPetricite()
    {
        var board = BoardData.CreateDefaultBoard();
        Assert.True(board["N"].Terrain.HasFlag(TerrainType.Heartland));
        Assert.True(board["N"].Terrain.HasFlag(TerrainType.Petricite));
        Assert.Equal(6, board["N"].Tiles.Length);
    }

    [Fact]
    public void CreateDefaultBoard_PetriciteCities_AllowPetricite()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var id in new[] { "A", "D", "I", "N" })
            Assert.True(board[id].AllowsPetricite, $"City {id} should allow petricite tiles");
    }

    [Fact]
    public void CreateDefaultBoard_NonPetriciteCities_DoNotAllowPetricite()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var id in new[] { "B", "C", "E", "F", "G", "H", "J", "K", "L", "M", "O", "P" })
            Assert.False(board[id].AllowsPetricite, $"City {id} should not allow petricite tiles");
    }

    [Fact]
    public void CreateDefaultBoard_MountainCities_HaveCorrectTerrain()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var id in new[] { "E", "G", "L", "O" })
            Assert.True(board[id].Terrain.HasFlag(TerrainType.Mountain), $"City {id} should be mountain terrain");
    }

    [Fact]
    public void CreateDefaultBoard_BorderCities_HaveCorrectTerrain()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var id in new[] { "C", "F", "K", "M" })
            Assert.True(board[id].Terrain.HasFlag(TerrainType.Border), $"City {id} should be border terrain");
    }

    [Fact]
    public void CreateDefaultBoard_AllMultipliersStartAtOne()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var city in board.Values)
            Assert.Equal(1.0, city.Multiplier);
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var board = BoardData.CreateDefaultBoard();
        board["A"].Tiles[0] = TileType.Stone;

        var clone = BoardData.Clone(board);
        Assert.Equal(TileType.Stone, clone["A"].Tiles[0]);

        // Mutating original should not affect clone
        board["A"].Tiles[0] = TileType.Metal;
        Assert.Equal(TileType.Stone, clone["A"].Tiles[0]);
    }
}
