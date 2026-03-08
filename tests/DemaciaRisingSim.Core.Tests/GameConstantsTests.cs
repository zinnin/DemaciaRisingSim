using DemaciaRisingSim.Core;

namespace DemaciaRisingSim.Core.Tests;

public class GameConstantsTests
{
    [Fact]
    public void StructureData_MaxLevelOutputs_MatchExpectedValues()
    {
        // These values are the normalization basis used internally by ComputeSlotValue.
        // Verifying them here ensures StructureData hasn't been accidentally changed.
        Assert.Equal(150, StructureData.Get(StructureType.Lumberyard,    4).LumberOutput);    // L4
        Assert.Equal(100, StructureData.Get(StructureType.Quarry,        4).StoneOutput);     // L4
        Assert.Equal(50,  StructureData.Get(StructureType.Forge,         4).MetalOutput);     // L4
        Assert.Equal(3,   StructureData.Get(StructureType.PetriciteMill, 3).PetriciteOutput); // L3 (max)
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
