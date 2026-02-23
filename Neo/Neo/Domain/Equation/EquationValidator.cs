using Neo.Domain.Result;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Domain.Equation;

// Validator based on old IsTrash() logic
public static class EquationValidator
{
    public static Result<EquationSystem> ValidateAndCreate(IEnumerable<string> equationStrings)
    {
        if (equationStrings is null)
            return Result<EquationSystem>.Failure(Error.NullInput);

        var equations = new List<LinearEquation>();
        var errors = new List<string>();

        foreach (var eqString in equationStrings)
        {
            try
            {
                // Simplified parsing - will be replaced with proper parser
                var equation = ParseEquation(eqString);
                equations.Add(equation);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to parse '{eqString}': {ex.Message}");
            }
        }

        if (errors.Any())
            return Result<EquationSystem>.Failure(
                new Error($"Validation failed: {string.Join("; ", errors)}", "VALIDATION_ERROR"));

        if (equations.Count == 0)
            return Result<EquationSystem>.Failure(Error.EmptyInput);

        try
        {
            var system = new EquationSystem(equations);

            // From old IsTrash() logic:
            // 1. Check variable count vs equation count
            if (system.EquationCount > 5) // Old code: len is <= 0 or > 5
                return Result<EquationSystem>.Failure(
                    new Error("Too many equations", "TOO_MANY_EQUATIONS"));

            // 2. Check if solvable
            if (system.EquationCount < system.VariableCount)
                return Result<EquationSystem>.Failure(Error.Underdetermined);

            return Result<EquationSystem>.Success(system);
        }
        catch (Exception ex)
        {
            return Result<EquationSystem>.Failure(
                new Error($"Failed to create equation system: {ex.Message}", "SYSTEM_ERROR", ex));
        }
    }

    private static LinearEquation ParseEquation(string equationString)
    {
        // Temporary implementation - will be replaced with proper parser
        // This mimics old GetDigits() and AppendZeroCoefficients logic

        // Remove whitespace
        equationString = equationString.Replace(" ", "");

        // Split by =
        var parts = equationString.Split('=');
        if (parts.Length != 2)
            throw new FormatException("Equation must contain exactly one '='");

        // Extract coefficients (simplified)
        var left = parts[0];
        var right = parts[1];

        // This is a placeholder - actual parsing will be in Parser layer
        throw new NotImplementedException("Full parsing will be implemented in Parser layer");
    }
}

// Builder pattern for complex equation systems