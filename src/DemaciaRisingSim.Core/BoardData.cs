namespace DemaciaRisingSim.Core;

/// <summary>
/// Provides the default board configuration for Demacia Rising with all 16 settlements.
/// Each settlement has its environment and 6 structure slots defined.
/// Academy buff zones are derived automatically from shared environment types.
/// Settlement connections (for Marketplace bonuses) are set up via AddLink.
/// </summary>
public static class BoardData
{
    /// <summary>
    /// Creates a fresh default board with all 16 settlements, each initialized to
    /// Lumberyard level 1 in every slot.
    /// </summary>
    public static Dictionary<string, Settlement> CreateDefaultBoard()
    {
        var theGreatCity   = new Settlement("The Great City",  EnvironmentType.Petricite, isCapital: true);
        var brookhollow    = new Settlement("Brookhollow",     EnvironmentType.Heartland);
        var cloudfield     = new Settlement("Cloudfield",      EnvironmentType.Border);
        var dawnhold       = new Settlement("Dawnhold",        EnvironmentType.Petricite | EnvironmentType.Border);
        var evenmoor       = new Settlement("Evenmoor",        EnvironmentType.Mountain);
        var fossbarrow     = new Settlement("Fossbarrow",      EnvironmentType.Border);
        var hawkstone      = new Settlement("Hawkstone",       EnvironmentType.Mountain);
        var hayneath       = new Settlement("Hayneath",        EnvironmentType.Heartland);
        var highSilvermere = new Settlement("High Silvermere", EnvironmentType.Petricite | EnvironmentType.Mountain);
        var jandelle       = new Settlement("Jandelle",        EnvironmentType.Heartland);
        var meltridge      = new Settlement("Meltridge",       EnvironmentType.Border);
        var pinara         = new Settlement("Pinara",          EnvironmentType.Mountain);
        var terbisia       = new Settlement("Terbisia",        EnvironmentType.Border);
        var tylburne       = new Settlement("Tylburne",        EnvironmentType.Petricite | EnvironmentType.Heartland);
        var uwendale       = new Settlement("Uwendale",        EnvironmentType.Mountain);
        var vaskasia       = new Settlement("Vaskasia",        EnvironmentType.Heartland);

        // Fill every slot with Lumberyard L1 as the default starting state
        var all = new[] {
            theGreatCity, brookhollow, cloudfield, dawnhold, evenmoor, fossbarrow,
            hawkstone, hayneath, highSilvermere, jandelle, meltridge, pinara,
            terbisia, tylburne, uwendale, vaskasia };
        foreach (var s in all)
            Array.Fill(s.Structures, new Structure(StructureType.Lumberyard, 1));

        // Set up settlement connections (Marketplace bonus links)
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
