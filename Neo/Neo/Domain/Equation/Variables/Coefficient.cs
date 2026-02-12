using System;

namespace Neo.Domain.Equation.Variables;

// Represents a coefficient-variable pair
public sealed record Coefficient
{
    public double Value { get; }
    public Variable Variable { get; }

    public Coefficient(double value, Variable variable)
    {
        Value = value;
        Variable = variable ?? throw new ArgumentNullException(nameof(variable));
    }

    // From old code: Handle negative coefficients
    public Coefficient Negate() => new(-Value, Variable);

    // From old code: AppendUnitVariable adds "1" before variable
    public static Coefficient UnitCoefficient(Variable variable) => new(1, variable);

    public override string ToString() => Value switch
    {
        0 => "0",
        1 => $"{Variable}",
        -1 => $"-{Variable}",
        _ => $"{Value}{Variable}"
    };
}