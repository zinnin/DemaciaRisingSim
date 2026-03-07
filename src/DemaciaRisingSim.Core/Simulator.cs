namespace DemaciaRisingSim.Core;

/// <summary>
/// Calculates resource production and scores for board configurations,
/// and provides optimization routines to find high-scoring tile arrangements.
/// </summary>
public static class Simulator
{
    /// <summary>
    /// Calculates the resource output for a single city after applying terrain bonuses.
    /// Note: multipliers must already be set on the city (call ApplyMultipliers first on the full board).
    /// </summary>
    public static ResourceOutput CityOutput(City city)
    {
        int lumber = 0, stone = 0, metal = 0, petricite = 0;

        foreach (var tile in city.Tiles)
        {
            switch (tile)
            {
                case TileType.Lumber: lumber += GameConstants.LumberTileValue; break;
                case TileType.Stone: stone += GameConstants.StoneTileValue; break;
                case TileType.Metal: metal += GameConstants.MetalTileValue; break;
                case TileType.Petricite: petricite += GameConstants.PetriciteTileValue; break;
            }
        }

        // Apply terrain-specific flat bonuses and multiplier to base production
        if (city.Terrain.HasFlag(TerrainType.Mountain))
        {
            stone += GameConstants.StoneTileValue * GameConstants.MountainStoneBonusTiles;
            lumber = (int)Math.Floor(lumber * city.Multiplier);
        }
        else if (city.Terrain.HasFlag(TerrainType.Border))
        {
            metal += GameConstants.MetalTileValue * GameConstants.BorderMetalBonusTiles;
            lumber = (int)Math.Floor(lumber * city.Multiplier);
        }
        else if (city.Terrain.HasFlag(TerrainType.Heartland))
        {
            lumber = (int)Math.Floor(lumber * (city.Multiplier + GameConstants.HeartlandLumberBonus));
        }
        else
        {
            lumber = (int)Math.Floor(lumber * city.Multiplier);
        }

        stone = (int)Math.Floor(stone * city.Multiplier);
        metal = (int)Math.Floor(metal * city.Multiplier);
        petricite = (int)Math.Floor(petricite * city.Multiplier);

        return new ResourceOutput(lumber, stone, metal, petricite);
    }

    /// <summary>
    /// Calculates total resource production for the entire board,
    /// applying all marketplace and academy multiplier effects first.
    /// </summary>
    public static ResourceOutput BoardOutput(Dictionary<string, City> board)
    {
        // Work on a copy so we don't mutate the caller's board
        var workingBoard = BoardData.Clone(board);

        // Reset all multipliers to base
        foreach (var city in workingBoard.Values)
            city.Multiplier = 1.0;

        // Apply marketplace and academy multipliers
        foreach (var city in workingBoard.Values)
        {
            foreach (var tile in city.Tiles)
            {
                if (tile == TileType.Marketplace)
                {
                    foreach (var neighborId in city.Neighbors)
                        if (workingBoard.TryGetValue(neighborId, out var neighbor))
                            neighbor.Multiplier += GameConstants.MarketplaceMultiplier;
                }
                else if (tile == TileType.Academy)
                {
                    foreach (var buffedId in city.AcademyBuff)
                        if (workingBoard.TryGetValue(buffedId, out var buffed))
                            buffed.Multiplier += GameConstants.AcademyMultiplier;
                }
            }
        }

        // Sum up city outputs
        var total = ResourceOutput.Zero;
        foreach (var city in workingBoard.Values)
            total += CityOutput(city);

        return total;
    }

    /// <summary>
    /// Generates a single numeric score for a board's resource output.
    /// Higher is better. Rewards balanced production matching the ideal ratios.
    /// </summary>
    public static double Score(ResourceOutput output)
    {
        if (output.Lumber == 0) return 0;

        double realLumber = (double)output.Lumber / GameConstants.LumberTileValue;
        double realStone = (double)output.Stone / GameConstants.StoneTileValue;
        double realMetal = (double)output.Metal / GameConstants.MetalTileValue;
        double realPetricite = (double)output.Petricite / GameConstants.PetriciteTileValue;

        double stoneRelative = realStone / realLumber;
        double metalRelative = realMetal / realLumber;
        double petriciteRelative = realPetricite / realLumber;

        double ratioPenalty = Math.Exp(-1.0 * (
            Math.Pow((Math.Abs(stoneRelative - GameConstants.StoneRatio) / GameConstants.StoneRatio), 2) +
            Math.Pow((Math.Abs(metalRelative - GameConstants.MetalRatio) / GameConstants.MetalRatio), 2) +
            Math.Pow((Math.Abs(petriciteRelative - GameConstants.PetriciteRatio) / GameConstants.PetriciteRatio), 2)));

        return (realLumber + realStone + realMetal + realPetricite) * ratioPenalty;
    }

    /// <summary>
    /// Scores a board configuration directly.
    /// </summary>
    public static double Score(Dictionary<string, City> board) => Score(BoardOutput(board));

    /// <summary>
    /// Performs one pass over every tile on the board, trying each valid tile type
    /// and keeping any change that improves the score.
    /// </summary>
    public static Dictionary<string, City> Permutate(Dictionary<string, City> board)
    {
        var current = BoardData.Clone(board);
        double highScore = Score(current);

        foreach (var cityId in current.Keys.ToList())
        {
            var city = current[cityId];
            var maxTile = city.AllowsPetricite ? TileType.Petricite : TileType.Academy;

            for (int tileIndex = 0; tileIndex < city.Tiles.Length; tileIndex++)
            {
                var originalTile = city.Tiles[tileIndex];

                for (var candidate = TileType.Lumber; candidate <= maxTile; candidate++)
                {
                    city.Tiles[tileIndex] = candidate;
                    double candidateScore = Score(current);
                    if (candidateScore > highScore)
                    {
                        highScore = candidateScore;
                        board = BoardData.Clone(current);
                    }
                    else
                    {
                        city.Tiles[tileIndex] = originalTile;
                        current = BoardData.Clone(board);
                        city = current[cityId];
                    }
                }
            }
        }

        return board;
    }

    /// <summary>
    /// Iteratively calls Permutate until no further improvement is found,
    /// up to a maximum of <see cref="GameConstants.MaxOptimizationIterations"/> iterations.
    /// </summary>
    public static Dictionary<string, City> OptimizeBoard(Dictionary<string, City> board)
    {
        var solution = Permutate(board);
        int iterations = GameConstants.MaxOptimizationIterations;

        while (iterations-- > 0 && !BoardsEqual(solution, board))
        {
            board = BoardData.Clone(solution);
            solution = Permutate(solution);
        }

        return solution;
    }

    /// <summary>
    /// Generates a formatted summary of a board's tile layout and production output.
    /// </summary>
    public static string FullReport(Dictionary<string, City> board)
    {
        var output = BoardOutput(board);
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("Board Layout:");
        foreach (var kv in board)
        {
            var tiles = string.Join(", ", kv.Value.Tiles.Select(t => t.ToString()));
            sb.AppendLine($"  {kv.Key}: [{tiles}]");
        }

        sb.AppendLine();
        sb.AppendLine($"Total Production: {output}");
        sb.AppendLine();

        double realLumber = (double)output.Lumber / GameConstants.LumberTileValue;
        double realStone = (double)output.Stone / GameConstants.StoneTileValue;
        double realMetal = (double)output.Metal / GameConstants.MetalTileValue;
        double realPetricite = (double)output.Petricite / GameConstants.PetriciteTileValue;

        sb.AppendLine("Adjusted Output (in tile-equivalents):");
        sb.AppendLine($"  Lumber:    {realLumber:F4}");
        sb.AppendLine($"  Stone:     {realStone:F4}");
        sb.AppendLine($"  Metal:     {realMetal:F4}");
        sb.AppendLine($"  Petricite: {realPetricite:F4}");

        if (realLumber > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Ratios (relative to lumber):");
            sb.AppendLine($"  Stone:     {realStone / realLumber:F4} (target: {GameConstants.StoneRatio})");
            sb.AppendLine($"  Metal:     {realMetal / realLumber:F4} (target: {GameConstants.MetalRatio})");
            sb.AppendLine($"  Petricite: {realPetricite / realLumber:F4} (target: {GameConstants.PetriciteRatio})");
        }

        sb.AppendLine();
        sb.AppendLine($"Score: {Score(output):F6}");

        return sb.ToString();
    }

    /// <summary>
    /// Checks whether two boards have identical tile assignments.
    /// </summary>
    private static bool BoardsEqual(Dictionary<string, City> a, Dictionary<string, City> b)
    {
        if (a.Count != b.Count) return false;
        foreach (var kv in a)
        {
            if (!b.TryGetValue(kv.Key, out var bCity)) return false;
            if (!kv.Value.Tiles.SequenceEqual(bCity.Tiles)) return false;
        }
        return true;
    }
}
