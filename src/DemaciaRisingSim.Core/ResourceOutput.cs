namespace DemaciaRisingSim.Core;

/// <summary>
/// The resource output for a single city or the entire board.
/// </summary>
public record ResourceOutput(int Lumber, int Stone, int Metal, int Petricite)
{
    public static ResourceOutput Zero => new(0, 0, 0, 0);

    public static ResourceOutput operator +(ResourceOutput a, ResourceOutput b) =>
        new(a.Lumber + b.Lumber, a.Stone + b.Stone, a.Metal + b.Metal, a.Petricite + b.Petricite);

    public override string ToString() =>
        $"Lumber: {Lumber}, Stone: {Stone}, Metal: {Metal}, Petricite: {Petricite}";
}
