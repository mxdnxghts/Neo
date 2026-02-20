using Neo.Domain.Equation;
using Neo.Domain.Result;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Parsing;

/// <summary>
/// Interface for parsing human-readable linear equation strings into <see cref="EquationSystem"/> objects.
/// Example input: "2x + 3y = 5; x - y = 1"
/// </summary>
public interface IEquationParser
{
    /// <summary>
    /// Parses an equation string into an <see cref="EquationSystem"/>.
    /// </summary>
    /// <param name="input">The equation string.</param>
    /// <returns>A <see cref="Result{T}"/> containing the parsed system or error.</returns>
    Result<EquationSystem> Parse(string input);

    /// <summary>
    /// Asynchronously parses an equation string into an <see cref="EquationSystem"/>.
    /// </summary>
    /// <param name="input">The equation string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the parse result.</returns>
    Task<Result<EquationSystem>> ParseAsync(string input, CancellationToken cancellationToken = default);
}

/// <summary>
/// High-performance parser interface using <see cref="Span{T}"/> for zero-allocation parsing.
/// </summary>
public interface ISpanEquationParser
{
    /// <summary>
    /// Parses an equation string using span-based processing.
    /// </summary>
    /// <param name="input">The equation string as a span.</param>
    /// <returns>A <see cref="Result{T}"/> containing the parsed system or error.</returns>
    Result<EquationSystem> Parse(ReadOnlySpan<char> input);
}

