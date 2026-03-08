using DemaciaRisingSim.Core;

namespace DemaciaRisingSim.Core.Tests;

public class GameConstantsTests
{
    [Fact]
    public void NormalizationValues_MatchMaxLevelStructureOutputs()
    {
        // Normalization constants should equal the max-level output of each structure type
        Assert.Equal(150, GameConstants.LumberTileValue);   // Lumberyard L4
        Assert.Equal(100, GameConstants.StoneTileValue);    // Quarry L4
        Assert.Equal(50,  GameConstants.MetalTileValue);    // Forge L4
        Assert.Equal(3,   GameConstants.PetriciteTileValue);// PetriciteMill L3

        // Verify against StructureData definitions
        Assert.Equal(GameConstants.LumberTileValue,    StructureData.Get(StructureType.Lumberyard,    4).LumberOutput);
        Assert.Equal(GameConstants.StoneTileValue,     StructureData.Get(StructureType.Quarry,        4).StoneOutput);
        Assert.Equal(GameConstants.MetalTileValue,     StructureData.Get(StructureType.Forge,         4).MetalOutput);
        Assert.Equal(GameConstants.PetriciteTileValue, StructureData.Get(StructureType.PetriciteMill, 3).PetriciteOutput);
    }

    [Fact]
    public void ResourceRatios_AreCorrect()
    {
        Assert.Equal(1.25, GameConstants.StoneRatio);
        Assert.Equal(1.5,  GameConstants.MetalRatio);
        Assert.Equal(0.3,  GameConstants.PetriciteRatio);
    }

    [Fact]
    public void SettlementSlotCount_IsSix()
    {
        Assert.Equal(6, GameConstants.SettlementSlotCount);
    }

    [Fact]
    public void AcademyAndMarketplaceMultipliers_AreInStructureData()
    {
        // Level-based multipliers live in StructureData, not GameConstants
        Assert.Equal(0.05, StructureData.Get(StructureType.Academy,     1).AcademyMultiplier);
        Assert.Equal(0.10, StructureData.Get(StructureType.Academy,     3).AcademyMultiplier);
        Assert.Equal(0.05, StructureData.Get(StructureType.Marketplace, 1).MarketplaceMultiplier);
        Assert.Equal(0.10, StructureData.Get(StructureType.Marketplace, 3).MarketplaceMultiplier);
    }
}
