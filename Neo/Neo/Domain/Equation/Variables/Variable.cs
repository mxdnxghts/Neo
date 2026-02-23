using System;

namespace Neo.Domain.Equation.Variables;

/// <summary>
/// Represents a variable in a linear equation system (e.g., x, y, z).
/// Variables are immutable, case-insensitive, and identified by a single letter name.
/// </summary>
public sealed class Variable : IEquatable<Variable>, IComparable<Variable>
{
    /// <summary>
    /// Gets the name of the variable (single letter, lowercase).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the index position of this variable in the system.
    /// Used for matrix ordering.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Variable"/> class.
    /// </summary>
    /// <param name="name">The variable name (must be a single letter).</param>
    /// <exception cref="ArgumentException">
    /// Thrown when name is empty, null, whitespace, longer than one character, or not a letter.
    /// </exception>
    private Variable(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Variable name cannot be empty", nameof(name));
        if (name.Length > 1)
            throw new ArgumentException("Variables are single characters in this system", nameof(name));
        if (!char.IsLetter(name[0]))
            throw new ArgumentException("Variable name must be a letter", nameof(name));

        Name = name.ToLowerInvariant();
    }

    /// <summary>
    /// Creates a new <see cref="Variable"/> with the specified name.
    /// </summary>
    /// <param name="name">The variable name (single letter).</param>
    /// <returns>A new <see cref="Variable"/> instance.</returns>
    public static Variable Create(string name) => new(name);

    /// <summary>
    /// Sets the index position of this variable in the equation system.
    /// </summary>
    /// <param name="index">The zero-based index position.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is negative.</exception>
    public void SetIndex(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        Index = index;
    }

    /// <summary>
    /// Determines whether the specified <see cref="Variable"/> is equal to this instance.
    /// Comparison is case-insensitive.
    /// </summary>
    /// <param name="other">The variable to compare.</param>
    /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
    public bool Equals(Variable? other) =>
        other is not null && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Variable other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Name.ToLowerInvariant().GetHashCode();

    /// <summary>
    /// Compares this variable to another for ordering (alphabetical, case-insensitive).
    /// </summary>
    /// <param name="other">The variable to compare to.</param>
    /// <returns>
    /// Less than 0 if this is less than other; 0 if equal; greater than 0 if greater.
    /// </returns>
    public int CompareTo(Variable? other) =>
        other is null ? 1 : string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>
    /// Multiplies a coefficient by a variable to create a <see cref="Coefficient"/>.
    /// Enables syntax like: <c>2 * x</c>.
    /// </summary>
    /// <param name="coefficient">The numeric coefficient value.</param>
    /// <param name="variable">The variable.</param>
    /// <returns>A new <see cref="Coefficient"/> instance.</returns>
    public static Coefficient operator *(double coefficient, Variable variable) =>
        new(coefficient, variable);
}