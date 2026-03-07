using DemaciaRisingSim.Core;

namespace DemaciaRisingSim.Core.Tests;

public class GameConstantsTests
{
    [Fact]
    public void TileValues_AreCorrect()
    {
        Assert.Equal(150, GameConstants.LumberTileValue);
        Assert.Equal(100, GameConstants.StoneTileValue);
        Assert.Equal(50, GameConstants.MetalTileValue);
        Assert.Equal(3, GameConstants.PetriciteTileValue);
        Assert.Equal(5, GameConstants.FarmTileValue);
    }

    [Fact]
    public void ResourceRatios_AreCorrect()
    {
        Assert.Equal(1.25, GameConstants.StoneRatio);
        Assert.Equal(1.5, GameConstants.MetalRatio);
        Assert.Equal(0.3, GameConstants.PetriciteRatio);
    }

    [Fact]
    public void Multipliers_AreCorrect()
    {
        Assert.Equal(0.1, GameConstants.MarketplaceMultiplier);
        Assert.Equal(0.1, GameConstants.AcademyMultiplier);
    }
}
