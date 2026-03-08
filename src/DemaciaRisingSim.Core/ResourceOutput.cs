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

/// <summary>
/// Turns required to accumulate each resource target given the board's per-turn output.
/// A value of <see cref="int.MaxValue"/> means the resource has zero production but a
/// non-zero target (effectively unreachable). A value of 0 means the target is 0 (already met).
/// </summary>
public record TurnsBreakdown(int Lumber, int Stone, int Metal, int Petricite)
{
    /// <summary>The bottleneck: the largest turns value across all four resources.</summary>
    public int Max => Math.Max(Lumber, Math.Max(Stone, Math.Max(Metal, Petricite)));

    /// <summary>
    /// Formats a turns value for display.
    /// Returns "—" when the value is <see cref="int.MaxValue"/> (zero production, non-zero target).
    /// </summary>
    public static string Format(int turns) =>
        turns == int.MaxValue ? "—" : turns.ToString();
}
