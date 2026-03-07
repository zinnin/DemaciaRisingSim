namespace DemaciaRisingSim.Core;

/// <summary>
/// Provides the default board configuration for Demacia Rising with all 16 settlements.
/// Each settlement has its terrain, tile count, and academy buff zone defined.
/// Settlement connections (for marketplace bonuses) are set up via AddLink.
/// </summary>
public static class BoardData
{
    /// <summary>
    /// Creates a fresh default board with all 16 settlements initialized to Lumber tiles.
    /// </summary>
    public static Dictionary<string, Settlement> CreateDefaultBoard()
    {
        var theGreatCity   = new Settlement("The Great City",  TerrainType.Petricite,
            academyBuff: ["The Great City"],
            tileCount: 5);

        var brookhollow    = new Settlement("Brookhollow",     TerrainType.Heartland,
            academyBuff: ["Brookhollow", "Hayneath", "Jandelle", "Tylburne", "Vaskasia"],
            tileCount: 6);

        var cloudfield     = new Settlement("Cloudfield",      TerrainType.Border,
            academyBuff: ["Cloudfield", "Dawnhold", "Fossbarrow", "Meltridge", "Terbisia"],
            tileCount: 5);

        var dawnhold       = new Settlement("Dawnhold",        TerrainType.Petricite | TerrainType.Border,
            academyBuff: ["Cloudfield", "Dawnhold", "Fossbarrow", "Meltridge", "Terbisia"],
            tileCount: 5);

        var evenmoor       = new Settlement("Evenmoor",        TerrainType.Mountain,
            academyBuff: ["Evenmoor", "Hawkstone", "High Silvermere", "Pinara", "Uwendale"],
            tileCount: 5);

        var fossbarrow     = new Settlement("Fossbarrow",      TerrainType.Border,
            academyBuff: ["Cloudfield", "Dawnhold", "Fossbarrow", "Meltridge", "Terbisia"],
            tileCount: 5);

        var hawkstone      = new Settlement("Hawkstone",       TerrainType.Mountain,
            academyBuff: ["Evenmoor", "Hawkstone", "High Silvermere", "Pinara", "Uwendale"],
            tileCount: 5);

        var hayneath       = new Settlement("Hayneath",        TerrainType.Heartland,
            academyBuff: ["Brookhollow", "Hayneath", "Jandelle", "Tylburne", "Vaskasia"],
            tileCount: 6);

        var highSilvermere = new Settlement("High Silvermere", TerrainType.Petricite | TerrainType.Mountain,
            academyBuff: ["Evenmoor", "Hawkstone", "High Silvermere", "Pinara", "Uwendale"],
            tileCount: 5);

        var jandelle       = new Settlement("Jandelle",        TerrainType.Heartland,
            academyBuff: ["Brookhollow", "Hayneath", "Jandelle", "Tylburne", "Vaskasia"],
            tileCount: 6);

        var meltridge      = new Settlement("Meltridge",       TerrainType.Border,
            academyBuff: ["Cloudfield", "Dawnhold", "Fossbarrow", "Meltridge", "Terbisia"],
            tileCount: 5);

        var pinara         = new Settlement("Pinara",          TerrainType.Mountain,
            academyBuff: ["Evenmoor", "Hawkstone", "High Silvermere", "Pinara", "Uwendale"],
            tileCount: 5);

        var terbisia       = new Settlement("Terbisia",        TerrainType.Border,
            academyBuff: ["Cloudfield", "Dawnhold", "Fossbarrow", "Meltridge", "Terbisia"],
            tileCount: 5);

        var tylburne       = new Settlement("Tylburne",        TerrainType.Petricite | TerrainType.Heartland,
            academyBuff: ["Brookhollow", "Hayneath", "Jandelle", "Tylburne", "Vaskasia"],
            tileCount: 6);

        var uwendale       = new Settlement("Uwendale",        TerrainType.Mountain,
            academyBuff: ["Evenmoor", "Hawkstone", "High Silvermere", "Pinara", "Uwendale"],
            tileCount: 5);

        var vaskasia       = new Settlement("Vaskasia",        TerrainType.Heartland,
            academyBuff: ["Brookhollow", "Hayneath", "Jandelle", "Tylburne", "Vaskasia"],
            tileCount: 6);

        // Set up settlement connections (marketplace bonus links)
        theGreatCity.AddLink(dawnhold);
        theGreatCity.AddLink(highSilvermere);
        theGreatCity.AddLink(tylburne);

        brookhollow.AddLink(evenmoor);
        brookhollow.AddLink(terbisia);
        brookhollow.AddLink(tylburne);
        brookhollow.AddLink(vaskasia);

        cloudfield.AddLink(hayneath);
        cloudfield.AddLink(jandelle);
        cloudfield.AddLink(terbisia);
        cloudfield.AddLink(tylburne);

        dawnhold.AddLink(theGreatCity);
        dawnhold.AddLink(pinara);
        dawnhold.AddLink(vaskasia);

        evenmoor.AddLink(brookhollow);
        evenmoor.AddLink(vaskasia);

        fossbarrow.AddLink(highSilvermere);
        fossbarrow.AddLink(pinara);

        hawkstone.AddLink(highSilvermere);
        hawkstone.AddLink(uwendale);

        hayneath.AddLink(cloudfield);
        hayneath.AddLink(jandelle);

        highSilvermere.AddLink(theGreatCity);
        highSilvermere.AddLink(fossbarrow);
        highSilvermere.AddLink(hawkstone);
        highSilvermere.AddLink(jandelle);
        highSilvermere.AddLink(pinara);
        highSilvermere.AddLink(uwendale);

        jandelle.AddLink(cloudfield);
        jandelle.AddLink(hayneath);
        jandelle.AddLink(highSilvermere);
        jandelle.AddLink(meltridge);
        jandelle.AddLink(tylburne);

        meltridge.AddLink(jandelle);
        meltridge.AddLink(uwendale);

        pinara.AddLink(dawnhold);
        pinara.AddLink(fossbarrow);
        pinara.AddLink(highSilvermere);

        terbisia.AddLink(brookhollow);
        terbisia.AddLink(cloudfield);

        tylburne.AddLink(theGreatCity);
        tylburne.AddLink(brookhollow);
        tylburne.AddLink(cloudfield);
        tylburne.AddLink(jandelle);

        uwendale.AddLink(hawkstone);
        uwendale.AddLink(highSilvermere);
        uwendale.AddLink(meltridge);

        vaskasia.AddLink(brookhollow);
        vaskasia.AddLink(dawnhold);
        vaskasia.AddLink(evenmoor);

        return new Dictionary<string, Settlement>
        {
            [theGreatCity.Name]   = theGreatCity,
            [brookhollow.Name]    = brookhollow,
            [cloudfield.Name]     = cloudfield,
            [dawnhold.Name]       = dawnhold,
            [evenmoor.Name]       = evenmoor,
            [fossbarrow.Name]     = fossbarrow,
            [hawkstone.Name]      = hawkstone,
            [hayneath.Name]       = hayneath,
            [highSilvermere.Name] = highSilvermere,
            [jandelle.Name]       = jandelle,
            [meltridge.Name]      = meltridge,
            [pinara.Name]         = pinara,
            [terbisia.Name]       = terbisia,
            [tylburne.Name]       = tylburne,
            [uwendale.Name]       = uwendale,
            [vaskasia.Name]       = vaskasia,
        };
    }

    /// <summary>Creates a deep copy of a board.</summary>
    public static Dictionary<string, Settlement> Clone(Dictionary<string, Settlement> board) =>
        board.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());
}
