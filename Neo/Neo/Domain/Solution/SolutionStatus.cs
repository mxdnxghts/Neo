namespace Neo.Domain.Solution;

/// <summary>
/// Represents the status of a solution to a linear equation system.
/// </summary>
public enum SolutionStatus
{
    /// <summary>
    /// A unique solution was found.
    /// </summary>
    Success,

    /// <summary>
    /// The system has no solution (inconsistent equations).
    /// </summary>
    NoSolution,

    /// <summary>
    /// The system has infinitely many solutions.
    /// </summary>
    InfiniteSolutions,

    /// <summary>
    /// An error occurred during solving.
    /// </summary>
    Error
}