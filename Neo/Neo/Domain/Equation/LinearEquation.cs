using Neo.Domain.Equation.Variables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Neo.Domain.Equation;

public sealed partial class LinearEquation : IEquatable<LinearEquation>
{
    private readonly Dictionary<Variable, double> _coefficients;

    public double Constant { get; }
    public IReadOnlyCollection<Variable> Variables => _coefficients.Keys;

    // Primary constructor – domain object is built from already‑parsed data
    public LinearEquation(IEnumerable<Coefficient> coefficients, double constant)
    {
        _coefficients = coefficients?.ToDictionary(c => c.Variable, c => c.Value)
                        ?? throw new ArgumentNullException(nameof(coefficients));
        Constant = constant;
        Validate();
    }

    // Convenience constructor for dictionary
    public LinearEquation(IReadOnlyDictionary<Variable, double> coefficients, double constant)
        : this(coefficients.Select(kvp => new Coefficient(kvp.Value, kvp.Key)), constant)
    {
    }

    public double GetCoefficient(Variable variable) =>
        _coefficients.TryGetValue(variable, out var val) ? val : 0;

    public bool HasVariable(Variable variable) => _coefficients.ContainsKey(variable);

    public LinearEquation WithZeroCoefficient(Variable variable)
    {
        if (HasVariable(variable))
            return this;
        var newCoeffs = new Dictionary<Variable, double>(_coefficients) { [variable] = 0 };
        return new LinearEquation(newCoeffs, Constant);
    }

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

    private void Validate()
    {
        if (_coefficients.Count == 0)
            throw new InvalidOperationException("Equation must have at least one variable.");
        if (_coefficients.Values.All(v => Math.Abs(v) < double.Epsilon))
            throw new InvalidOperationException("At least one coefficient must be non-zero.");
    }

    private static string FormatCoefficient(double coefficient)
    {
        if (Math.Abs(coefficient - 1) < double.Epsilon)
            return "";
        if (Math.Abs(coefficient + 1) < double.Epsilon)
            return "-";
        return coefficient.ToString(CultureInfo.InvariantCulture);
    }
}