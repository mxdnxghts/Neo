using Neo.Domain.Equation.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Domain.Equation;

/// <summary>
/// Represents a system of linear equations.
/// Example: 
///   2x + 3y = 5
///   x - y = 1
/// </summary>
public sealed class EquationSystem
{
    private readonly List<LinearEquation> _equations;
    private readonly List<Variable> _variables;

    /// <summary>
    /// Gets the equations in this system.
    /// </summary>
    public IReadOnlyList<LinearEquation> Equations => _equations;

    /// <summary>
    /// Gets all distinct variables in the system, ordered alphabetically.
    /// </summary>
    public IReadOnlyList<Variable> Variables => _variables;

    /// <summary>
    /// Gets the number of equations in the system.
    /// </summary>
    public int EquationCount => _equations.Count;

    /// <summary>
    /// Gets the number of distinct variables in the system.
    /// </summary>
    public int VariableCount => _variables.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquationSystem"/> class.
    /// </summary>
    /// <param name="equations">The collection of equations.</param>
    /// <exception cref="ArgumentNullException">Thrown when equations is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no equations are provided.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when system exceeds limits (too many equations or variables).
    /// </exception>
    public EquationSystem(IEnumerable<LinearEquation> equations)
    {
        if (equations is null)
            throw new ArgumentNullException(nameof(equations));

        _equations = [.. equations];
        if (_equations.Count == 0)
            throw new ArgumentException("At least one equation is required", nameof(equations));

        _variables = ExtractAllVariables(_equations);

        Validate();
    }

    /// <summary>
    /// Extracts all distinct variables from the equations.
    /// </summary>
    /// <param name="equations">The equations to extract variables from.</param>
    /// <returns>List of variables ordered alphabetically.</returns>
    private List<Variable> ExtractAllVariables(List<LinearEquation> equations)
    {
        var allVariables = new HashSet<Variable>();

        foreach (var equation in equations)
        {
            foreach (var variable in equation.Variables)
            {
                allVariables.Add(variable);
            }
        }

        return allVariables.OrderBy(v => v.Name).ToList();
    }

    /// <summary>
    /// Validates the equation system for solvability and constraints.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when system exceeds limits or is underdetermined.
    /// </exception>
    private void Validate()
    {
        if (_equations.Count > 100)
            throw new InvalidOperationException("Too many equations");

        if (_variables.Count > 26)
            throw new InvalidOperationException("Too many variables (max 26)");

        if (_equations.Count < _variables.Count)
            throw new InvalidOperationException("Underdetermined system: more variables than equations");
    }

    /// <summary>
    /// Normalizes the system by ensuring all equations contain all variables.
    /// Adds zero coefficients for missing variables.
    /// </summary>
    /// <returns>A new normalized <see cref="EquationSystem"/>.</returns>
    public EquationSystem Normalize()
    {
        var normalizedEquations = new List<LinearEquation>();

        foreach (var equation in _equations)
        {
            var normalizedEquation = equation;
            foreach (var variable in _variables)
            {
                normalizedEquation = normalizedEquation.WithZeroCoefficient(variable);
            }
            normalizedEquations.Add(normalizedEquation);
        }

        return new EquationSystem(normalizedEquations);
    }

    /// <summary>
    /// Converts the equation system to matrix form for numerical solving.
    /// </summary>
    /// <returns>
    /// A tuple containing the coefficient matrix (2D array) and constants vector.
    /// </returns>
    public (double[,] Coefficients, double[] Constants) ToMatrix()
    {
        var normalized = Normalize();
        var coefficients = new double[normalized.EquationCount, normalized.VariableCount];
        var constants = new double[normalized.EquationCount];

        for (int i = 0; i < normalized.EquationCount; i++)
        {
            var equation = normalized.Equations[i];
            for (int j = 0; j < normalized.VariableCount; j++)
            {
                coefficients[i, j] = equation.GetCoefficient(normalized.Variables[j]);
            }
            constants[i] = equation.Constant;
        }

        return (coefficients, constants);
    }

    /// <summary>
    /// Gets a value indicating whether the system is square (equal equations and variables).
    /// </summary>
    public bool IsSquare => EquationCount == VariableCount;

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Join(Environment.NewLine, _equations.Select(e => e.ToString()));
    }
}
