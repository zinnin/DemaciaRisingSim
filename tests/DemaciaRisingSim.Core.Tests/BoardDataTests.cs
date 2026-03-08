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
    public void CreateDefaultBoard_AllSettlementsHaveSixSlots()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var settlement in board.Values)
            Assert.Equal(GameConstants.SettlementSlotCount, settlement.Structures.Length);
    }

    [Fact]
    public void CreateDefaultBoard_AllSlotsInitializedToLumberyardL1()
    {
        var board = BoardData.CreateDefaultBoard();
        var expected = new Structure(StructureType.Lumberyard, 1);
        foreach (var settlement in board.Values)
            foreach (var structure in settlement.Structures)
                Assert.Equal(expected, structure);
    }

    [Fact]
    public void CreateDefaultBoard_TheGreatCity_IsCapital()
    {
        var board = BoardData.CreateDefaultBoard();
        Assert.True(board["The Great City"].IsCapital);
        Assert.True(board["The Great City"].AllowsPetriciteMill);
    }

    [Fact]
    public void CreateDefaultBoard_PetriciteTerrain_AllowsPetriciteMill()
    {
        // PetriciteMill is available in all Petricite-terrain settlements, not just the capital.
        var board = BoardData.CreateDefaultBoard();
        var petriciteSettlements = new[] { "The Great City", "Dawnhold", "High Silvermere", "Tylburne" };
        var nonPetriciteNames   = board.Keys.Except(petriciteSettlements).ToArray();

        foreach (var name in petriciteSettlements)
            Assert.True(board[name].AllowsPetriciteMill,
                $"Settlement {name} (Petricite terrain) should allow PetriciteMill");

        foreach (var name in nonPetriciteNames)
            Assert.False(board[name].AllowsPetriciteMill,
                $"Settlement {name} (no Petricite terrain) should not allow PetriciteMill");
    }

    [Fact]
    public void CreateDefaultBoard_Tylburne_IsHeartlandAndPetricite()
    {
        var board = BoardData.CreateDefaultBoard();
        Assert.True(board["Tylburne"].Environment.HasFlag(EnvironmentType.Heartland));
        Assert.True(board["Tylburne"].Environment.HasFlag(EnvironmentType.Petricite));
    }

    [Fact]
    public void CreateDefaultBoard_MountainSettlements_HaveCorrectEnvironment()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var name in new[] { "Evenmoor", "Hawkstone", "Pinara", "Uwendale" })
            Assert.True(board[name].Environment.HasFlag(EnvironmentType.Mountain),
                $"Settlement {name} should have Mountain environment");
    }

    [Fact]
    public void CreateDefaultBoard_BorderSettlements_HaveCorrectEnvironment()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var name in new[] { "Cloudfield", "Fossbarrow", "Meltridge", "Terbisia" })
            Assert.True(board[name].Environment.HasFlag(EnvironmentType.Border),
                $"Settlement {name} should have Border environment");
    }

    [Fact]
    public void CreateDefaultBoard_HeartlandSettlements_HaveCorrectEnvironment()
    {
        var board = BoardData.CreateDefaultBoard();
        foreach (var name in new[] { "Brookhollow", "Hayneath", "Jandelle", "Vaskasia" })
            Assert.True(board[name].Environment.HasFlag(EnvironmentType.Heartland),
                $"Settlement {name} should have Heartland environment");
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
        board["The Great City"].Structures[0] = new Structure(StructureType.Quarry, 2);

        var clone = BoardData.Clone(board);
        Assert.Equal(new Structure(StructureType.Quarry, 2), clone["The Great City"].Structures[0]);

        // Mutating original should not affect clone
        board["The Great City"].Structures[0] = new Structure(StructureType.Forge, 1);
        Assert.Equal(new Structure(StructureType.Quarry, 2), clone["The Great City"].Structures[0]);
    }
}
