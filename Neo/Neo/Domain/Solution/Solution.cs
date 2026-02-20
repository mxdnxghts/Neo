using Neo.Domain.Equation;
using Neo.Domain.Equation.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Domain.Solution;

/// <summary>
/// Represents the solution to a system of linear equations.
/// Contains variable-value mappings and solution status.
/// </summary>
public sealed class Solution
{
    /// <summary>
    /// Gets the original equation system that was solved.
    /// </summary>
    public EquationSystem OriginalSystem { get; }

    /// <summary>
    /// Gets the computed values for each variable.
    /// Empty for non-success statuses.
    /// </summary>
    public IReadOnlyDictionary<Variable, double> Values { get; }

    /// <summary>
    /// Gets the status of the solution (success, no solution, infinite solutions, error).
    /// </summary>
    public SolutionStatus Status { get; }

    /// <summary>
    /// Gets an optional message describing the result or error.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Gets the UTC timestamp when the solution was computed.
    /// </summary>
    public DateTime SolvedAt { get; }

    private Solution(
        EquationSystem originalSystem,
        Dictionary<Variable, double> values,
        SolutionStatus status,
        string? message = null)
    {
        OriginalSystem = originalSystem ?? throw new ArgumentNullException(nameof(originalSystem));
        Values = values;
        Status = status;
        Message = message;
        SolvedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a successful solution with variable values.
    /// </summary>
    /// <param name="system">The original equation system.</param>
    /// <param name="values">The computed variable values.</param>
    /// <returns>A new <see cref="Solution"/> with Success status.</returns>
    public static Solution Success(EquationSystem system, Dictionary<Variable, double> values) =>
        new(system, values, SolutionStatus.Success);

    /// <summary>
    /// Creates a solution indicating no solution exists (inconsistent system).
    /// </summary>
    /// <param name="system">The original equation system.</param>
    /// <param name="message">Explanation of why no solution exists.</param>
    /// <returns>A new <see cref="Solution"/> with NoSolution status.</returns>
    public static Solution NoSolution(EquationSystem system, string message) =>
        new(system, [], SolutionStatus.NoSolution, message);

    /// <summary>
    /// Creates a solution indicating infinitely many solutions exist.
    /// </summary>
    /// <param name="system">The original equation system.</param>
    /// <param name="message">Explanation of the infinite solution case.</param>
    /// <returns>A new <see cref="Solution"/> with InfiniteSolutions status.</returns>
    public static Solution InfiniteSolutions(EquationSystem system, string message) =>
        new(system, [], SolutionStatus.InfiniteSolutions, message);

    /// <summary>
    /// Creates a solution indicating an error occurred during solving.
    /// </summary>
    /// <param name="system">The original equation system.</param>
    /// <param name="message">Error description.</param>
    /// <returns>A new <see cref="Solution"/> with Error status.</returns>
    public static Solution Error(EquationSystem system, string message) =>
        new(system, [], SolutionStatus.Error, message);

    /// <summary>
    /// Gets the computed value for the specified variable.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <returns>The computed value.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the variable is not found in the solution.
    /// </exception>
    public double GetValue(Variable variable)
    {
        if (!Values.TryGetValue(variable, out var value))
            throw new KeyNotFoundException($"Variable {variable} not found in solution");
        return value;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (Status != SolutionStatus.Success)
            return $"Solution Status: {Status} - {Message}";

        var lines = Values.Select(kv => $"{kv.Key} = {kv.Value}");
        return string.Join(Environment.NewLine, lines);
    }
}
