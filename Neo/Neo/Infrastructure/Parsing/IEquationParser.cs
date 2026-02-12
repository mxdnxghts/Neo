using Neo.Domain.Equation;
using Neo.Domain.Result;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Parsing;

// Primary parser interface
public interface IEquationParser
{
    Result<EquationSystem> Parse(string input);
    Task<Result<EquationSystem>> ParseAsync(string input, CancellationToken cancellationToken = default);
}

// Optimized parser using Span<T>
public interface ISpanEquationParser
{
    Result<EquationSystem> Parse(ReadOnlySpan<char> input);
}

