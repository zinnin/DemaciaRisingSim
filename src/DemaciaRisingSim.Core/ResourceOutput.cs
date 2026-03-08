namespace DemaciaRisingSim.Core;

/// <summary>
/// The resource output for a single settlement or the entire board.
/// Food is tracked but is not included in the production score.
/// </summary>
public record ResourceOutput(int Lumber, int Stone, int Metal, int Petricite, int Food = 0)
{
    public static ResourceOutput Zero => new(0, 0, 0, 0, 0);

    public static ResourceOutput operator +(ResourceOutput a, ResourceOutput b) =>
        new(a.Lumber + b.Lumber, a.Stone + b.Stone, a.Metal + b.Metal, a.Petricite + b.Petricite, a.Food + b.Food);

    public override string ToString() =>
        $"Lumber: {Lumber}, Stone: {Stone}, Metal: {Metal}, Petricite: {Petricite}, Food: {Food}";
}
