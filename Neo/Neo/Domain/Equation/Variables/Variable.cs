using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Domain.Equation.Variables;

// From old code analysis: variables are single characters (x, y, z)
// We'll create a proper domain model
public sealed class Variable : IEquatable<Variable>, IComparable<Variable>
{
    public string Name { get; }
    public int Index { get; private set; }

    // Based on old code: GetUnknownVariables() extracts letters
    private Variable(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Variable name cannot be empty", nameof(name));
        if (name.Length > 1)
            throw new ArgumentException("Variables are single characters in this system", nameof(name));
        if (!char.IsLetter(name[0]))
            throw new ArgumentException("Variable name must be a letter", nameof(name));

        Name = name.ToLowerInvariant(); // Old code: .ToLower() in SetInputConfiguration
    }

    public static Variable Create(string name)
    {
        return new Variable(name);
    }

    // From old code: GetIndices() method creates dictionary mapping
    public void SetIndex(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        Index = index;
    }

    // Equality based on name (case-insensitive)
    public bool Equals(Variable? other) =>
        other is not null && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is Variable other && Equals(other);

    public override int GetHashCode() => Name.ToLowerInvariant().GetHashCode();

    public int CompareTo(Variable? other) =>
        other is null ? 1 : string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Name;

    // Operator for easy coefficient notation: 2 * x
    public static Coefficient operator *(double coefficient, Variable variable) =>
        new(coefficient, variable);
}
