using Neo.Domain.Equation;
using Neo.Domain.Equation.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Domain.Solution;

// Represents the solution to an equation system
public sealed class Solution
{
    public EquationSystem OriginalSystem { get; }
    public IReadOnlyDictionary<Variable, double> Values { get; }
    public SolutionStatus Status { get; }
    public string? Message { get; }
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

    public static Solution Success(EquationSystem system, Dictionary<Variable, double> values) =>
        new(system, values, SolutionStatus.Success);

    public static Solution NoSolution(EquationSystem system, string message) =>
        new(system, [], SolutionStatus.NoSolution, message);

    public static Solution InfiniteSolutions(EquationSystem system, string message) =>
        new(system, [], SolutionStatus.InfiniteSolutions, message);

    public static Solution Error(EquationSystem system, string message) =>
        new(system, [], SolutionStatus.Error, message);

    public double GetValue(Variable variable)
    {
        if (!Values.TryGetValue(variable, out var value))
            throw new KeyNotFoundException($"Variable {variable} not found in solution");
        return value;
    }

    public override string ToString()
    {
        if (Status != SolutionStatus.Success)
            return $"Solution Status: {Status} - {Message}";

        var lines = Values.Select(kv => $"{kv.Key} = {kv.Value}");
        return string.Join(Environment.NewLine, lines);
    }
}
