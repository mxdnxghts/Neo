using Neo.Domain.Equation.Variables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Neo.Domain.Equation;

/// <summary>
/// Represents a single linear equation with multiple variables.
/// Example: 2x + 3y - z = 5
/// </summary>
public sealed partial class LinearEquation : IEquatable<LinearEquation>
{
    private readonly Dictionary<Variable, double> _coefficients;

    /// <summary>
    /// Gets the constant term on the right-hand side of the equation.
    /// </summary>
    public double Constant { get; }

    /// <summary>
    /// Gets the variables present in this equation.
    /// </summary>
    public IReadOnlyCollection<Variable> Variables => _coefficients.Keys;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearEquation"/> class.
    /// </summary>
    /// <param name="coefficients">The coefficient-variable pairs.</param>
    /// <param name="constant">The constant term.</param>
    /// <exception cref="ArgumentNullException">Thrown when coefficients is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when equation has no variables or all coefficients are zero.
    /// </exception>
    public LinearEquation(IEnumerable<Coefficient> coefficients, double constant)
    {
        _coefficients = coefficients?.ToDictionary(c => c.Variable, c => c.Value)
                        ?? throw new ArgumentNullException(nameof(coefficients));
        Constant = constant;
        Validate();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearEquation"/> class using a dictionary.
    /// </summary>
    /// <param name="coefficients">Dictionary mapping variables to coefficients.</param>
    /// <param name="constant">The constant term.</param>
    public LinearEquation(IReadOnlyDictionary<Variable, double> coefficients, double constant)
        : this(coefficients.Select(kvp => new Coefficient(kvp.Value, kvp.Key)), constant)
    {
    }

    /// <summary>
    /// Gets the coefficient for the specified variable.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <returns>The coefficient value, or 0 if the variable is not present.</returns>
    public double GetCoefficient(Variable variable) =>
        _coefficients.TryGetValue(variable, out var val) ? val : 0;

    /// <summary>
    /// Determines whether this equation contains the specified variable.
    /// </summary>
    /// <param name="variable">The variable to check.</param>
    /// <returns><c>true</c> if the variable is present; otherwise, <c>false</c>.</returns>
    public bool HasVariable(Variable variable) => _coefficients.ContainsKey(variable);

    /// <summary>
    /// Returns a new equation with the specified variable added (if not already present) with coefficient 0.
    /// Used during system normalization.
    /// </summary>
    /// <param name="variable">The variable to add.</param>
    /// <returns>A new <see cref="LinearEquation"/> instance, or this if variable already exists.</returns>
    public LinearEquation WithZeroCoefficient(Variable variable)
    {
        if (HasVariable(variable))
            return this;
        var newCoeffs = new Dictionary<Variable, double>(_coefficients) { [variable] = 0 };
        return new LinearEquation(newCoeffs, Constant);
    }

    /// <inheritdoc/>
    public bool Equals(LinearEquation? other)
    {
        if (other is null)
            return false;
        if (Math.Abs(Constant - other.Constant) > double.Epsilon)
            return false;
        if (_coefficients.Count != other._coefficients.Count)
            return false;
        foreach (var item in _coefficients)
        {
            if (!other._coefficients.TryGetValue(item.Key, out var otherCoefficient) ||
                Math.Abs(item.Value - otherCoefficient) > double.Epsilon)
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var terms = _coefficients
            .Where(kv => Math.Abs(kv.Value) > double.Epsilon)
            .Select(kv => $"{FormatCoefficient(kv.Value)}{kv.Key}")
            .ToList();

        if (terms.Count == 0)
            terms.Add("0");

        return $"{string.Join(" + ", terms)} = {Constant}";
    }

    /// <summary>
    /// Validates the equation structure.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when equation has no variables or all coefficients are zero.
    /// </exception>
    private void Validate()
    {
        if (_coefficients.Count == 0)
            throw new InvalidOperationException("Equation must have at least one variable.");
        if (_coefficients.Values.All(v => Math.Abs(v) < double.Epsilon))
            throw new InvalidOperationException("At least one coefficient must be non-zero.");
    }

    /// <summary>
    /// Formats a coefficient value for string representation.
    /// </summary>
    /// <param name="coefficient">The coefficient value.</param>
    /// <returns>Empty string for 1, "-" for -1, or the numeric value.</returns>
    private static string FormatCoefficient(double coefficient)
    {
        if (Math.Abs(coefficient - 1) < double.Epsilon)
            return "";
        if (Math.Abs(coefficient + 1) < double.Epsilon)
            return "-";
        return coefficient.ToString(CultureInfo.InvariantCulture);
    }
}