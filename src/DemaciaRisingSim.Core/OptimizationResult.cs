namespace DemaciaRisingSim.Core;

/// <summary>
/// Records the outcome of a single convergence phase within <see cref="Simulator.OptimizeBoard"/>.
/// </summary>
/// <param name="Phase">The 1-based phase number.</param>
/// <param name="Description">
/// A human-readable description of the phase's goal and actions.
/// This string is defined as a constant next to the phase code in <see cref="Simulator"/>
/// so that it stays in sync with the implementation.
/// </param>
/// <param name="Passes">The number of <c>Permutate</c> passes required to reach convergence.</param>
public record OptimizationPhaseResult(int Phase, string Description, int Passes);

/// <summary>
/// The result returned by <see cref="Simulator.OptimizeBoard"/>: the optimized board
/// together with per-phase convergence details.
/// </summary>
/// <param name="Board">The optimized board configuration.</param>
/// <param name="Phases">
/// The phases that ran, in order. Each entry records the phase number, its description,
/// and how many <c>Permutate</c> passes it needed to converge. Phases that did not run
/// (e.g. Phase 3 when the food target is already met) are omitted.
/// </param>
public record OptimizationResult(
    Dictionary<string, Settlement> Board,
    IReadOnlyList<OptimizationPhaseResult> Phases);
