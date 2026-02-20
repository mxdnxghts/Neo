using System;

namespace Neo.Domain.Equation.Variables;

/// <summary>
/// Represents a coefficient-variable pair in a linear equation (e.g., 3x, -2y).
/// </summary>
public sealed record Coefficient
{
    /// <summary>
    /// Gets the numeric value of the coefficient.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the variable associated with this coefficient.
    /// </summary>
    public Variable Variable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Coefficient"/> class.
    /// </summary>
    /// <param name="value">The coefficient value.</param>
    /// <param name="variable">The variable.</param>
    /// <exception cref="ArgumentNullException">Thrown when variable is null.</exception>
    public Coefficient(double value, Variable variable)
    {
        Value = value;
        Variable = variable ?? throw new ArgumentNullException(nameof(variable));
    }

    /// <summary>
    /// Creates a new coefficient with the negated value.
    /// </summary>
    /// <returns>A new <see cref="Coefficient"/> with value multiplied by -1.</returns>
    public Coefficient Negate() => new(-Value, Variable);

    /// <summary>
    /// Creates a coefficient with value of 1 for the specified variable.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <returns>A new <see cref="Coefficient"/> with value 1.</returns>
    public static Coefficient UnitCoefficient(Variable variable) => new(1, variable);

    /// <inheritdoc/>
    public override string ToString() => Value switch
    {
        0 => "0",
        1 => $"{Variable}",
        -1 => $"-{Variable}",
        _ => $"{Value}{Variable}"
    };
}