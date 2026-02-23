using Neo.Domain.Equation;
using Neo.Domain.Equation.Variables;
using Neo.Domain.Result;
using Neo.Infrastructure.Integration;
using Neo.Infrastructure.Parsing.Tokens;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Parsing;

public sealed class EquationParser : IEquationParser
{
    private const int StackallocTokenThreshold = 128;

    private readonly PerformanceMonitor? _performanceMonitor;
    private readonly ArrayPool<LinearEquation> _equationPool;
    private readonly ArrayPool<TokenInfo> _tokenPool;

    public EquationParser(PerformanceMonitor? performanceMonitor = null)
    {
        _performanceMonitor = performanceMonitor;
        _equationPool = ArrayPool<LinearEquation>.Shared;
        _tokenPool = ArrayPool<TokenInfo>.Shared;
    }

    public Result<EquationSystem> Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result<EquationSystem>.Failure(Error.EmptyInput);

        var stopwatch = Stopwatch.StartNew();
        var span = input.AsSpan();

        try
        {
            // Tokenize – use stackalloc for small, fallback to ArrayPool for large
            var tokenResult = TokenizeWithAdaptiveBuffer(span);
            using (tokenResult)
            {
                var result = ParseTokens(tokenResult.Tokens, input.AsMemory());
                stopwatch.Stop();
                _performanceMonitor?.RecordOperation("Parse", stopwatch.Elapsed, result.IsSuccess);
                return result;
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _performanceMonitor?.RecordOperation("Parse", stopwatch.Elapsed, false);
            return Result<EquationSystem>.Failure(
                new Error($"Parsing failed: {ex.Message}", "PARSE_ERROR", ex));
        }
    }

    // ---------- Adaptive Tokenization ----------
    private TokenizedResult TokenizeWithAdaptiveBuffer(ReadOnlySpan<char> span)
    {
        // Try stackalloc first
        Span<TokenInfo> stackBuffer = stackalloc TokenInfo[StackallocTokenThreshold];
        var tokenizer = new EquationTokenizer(span, stackBuffer);
        var tokens = tokenizer.Tokenize();

        if (tokens.Length <= StackallocTokenThreshold)
        {
            // Copy tokens to array for return
            var arr = new TokenInfo[tokens.Length];
            tokens.CopyTo(arr);
            return new TokenizedResult(arr, ownsBuffer: false);
        }

        // Fallback to pooled array
        var poolBuffer = _tokenPool.Rent(tokens.Length * 2); // over-allocate to avoid re-rent
        tokenizer = new EquationTokenizer(span, poolBuffer);
        tokens = tokenizer.Tokenize();
        return new TokenizedResult(poolBuffer, tokens.Length, _tokenPool);
    }

    // ---------- Token Parsing ----------
    private Result<EquationSystem> ParseTokens(ReadOnlySpan<TokenInfo> tokens, ReadOnlyMemory<char> source)
    {
        var equations = _equationPool.Rent(10);
        var equationCount = 0;

        try
        {
            var position = 0;
            while (position < tokens.Length && tokens[position].Type != TokenType.End)
            {
                var eqResult = ParseEquation(tokens, source, ref position);
                if (eqResult.IsFailure)
                    return Result<EquationSystem>.Failure(eqResult.Error!);

                if (equationCount >= equations.Length)
                {
                    var newArray = _equationPool.Rent(equations.Length * 2);
                    Array.Copy(equations, newArray, equationCount);
                    ClearAndReturnArray(equations, equationCount);
                    equations = newArray;
                }

                equations[equationCount++] = eqResult.Value!;

                if (position < tokens.Length && tokens[position].Type == TokenType.Separator)
                    position++;
            }

            var final = new LinearEquation[equationCount];
            Array.Copy(equations, final, equationCount);
            return Result<EquationSystem>.Success(new EquationSystem(final));
        }
        finally
        {
            ClearAndReturnArray(equations, equationCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ClearAndReturnArray(LinearEquation[] array, int length)
    {
        Array.Clear(array, 0, length);
        ArrayPool<LinearEquation>.Shared.Return(array);
    }

    // ---------- Single Equation Parsing ----------
    private Result<LinearEquation> ParseEquation(
        ReadOnlySpan<TokenInfo> tokens,
        ReadOnlyMemory<char> source,
        ref int position)
    {
        var coefficients = new Dictionary<Variable, double>();
        double constant = 0;
        bool hasEquals = false;
        double pendingCoefficient = 0;
        bool expectingCoefficient = false;
        bool nextIsNegative = false;  // sign for the next term
        bool onRightSide = false;  // track if we're after "="

        var sourceSpan = source.Span;

        while (position < tokens.Length)
        {
            var token = tokens[position];

            switch (token.Type)
            {
                case TokenType.Number:
                    var numberSpan = sourceSpan.Slice(token.Start, token.Length);
                    if (!TryParseNumber(numberSpan, out double number, out var numberError))
                        return Result<LinearEquation>.Failure(numberError!);

                    if (nextIsNegative)
                        number = -number;
                    nextIsNegative = false;

                    if (onRightSide)
                    {
                        // On right side: this is part of the constant
                        constant += number;
                    }
                    else
                    {
                        pendingCoefficient = number;
                        expectingCoefficient = true;
                    }
                    position++;
                    break;

                case TokenType.Variable:
                    var varSpan = sourceSpan.Slice(token.Start, token.Length);

                    // Validate variable name before creation
                    if (!IsValidVariableName(varSpan))
                        return Result<LinearEquation>.Failure(
                            new Error("INVALID_VARIABLE", $"Invalid variable name: '{varSpan.ToString()}'"));

                    var variable = Variable.Create(varSpan.ToString());  // uses span overload

                    double coeffValue;
                    if (expectingCoefficient)
                    {
                        coeffValue = pendingCoefficient;
                        expectingCoefficient = false;
                    }
                    else
                    {
                        coeffValue = nextIsNegative ? -1 : 1;
                        nextIsNegative = false;
                    }

                    // Combine coefficients if variable already exists
                    if (coefficients.TryGetValue(variable, out var existing))
                        coefficients[variable] = existing + coeffValue;
                    else
                        coefficients[variable] = coeffValue;

                    position++;
                    break;

                case TokenType.Operator:
                    // Determine sign from token value (not just presence)
                    var opSpan = sourceSpan.Slice(token.Start, token.Length);
                    if (opSpan.Length > 0 && opSpan[0] == '-')
                        nextIsNegative = true;
                    // '+' resets sign (already false)
                    position++;
                    break;

                case TokenType.Equals:
                    hasEquals = true;
                    onRightSide = true;
                    if (expectingCoefficient)
                    {
                        // Left-side constant: move to RHS (negate)
                        constant = -pendingCoefficient;
                        expectingCoefficient = false;
                        pendingCoefficient = 0;
                    }
                    position++;
                    break;

                case TokenType.Separator:
                case TokenType.End:
                    // Finalize equation
                    return BuildEquation(coefficients, constant, hasEquals, pendingCoefficient, expectingCoefficient);

                default:
                    position++;
                    break;
            }
        }

        return BuildEquation(coefficients, constant, hasEquals, pendingCoefficient, expectingCoefficient);
    }

    // ---------- Number Parsing (Zero‑Allocation) ----------
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseNumber(ReadOnlySpan<char> span, out double value, out Error? error)
    {
        error = null;
        value = 0;

        if (span.IsEmpty)
        {
            error = new Error("EMPTY_NUMBER", "Empty numeric token.");
            return false;
        }

        // Fast path: integer (no '.' or ',')
        if (!span.ContainsAny('.', ','))
        {
            if (!IsAllDigits(span, out var negative, out var start))
            {
                error = new Error("INVALID_INTEGER", $"Invalid integer format: '{span.ToString()}'");
                return false;
            }

            long result = 0;
            for (int i = start; i < span.Length; i++)
                result = result * 10 + (span[i] - '0');

            value = negative ? -result : result;
            return true;
        }

        // Slow path: floating point – use span‑based double.TryParse (no allocation)
        if (double.TryParse(span, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            return true;

        error = new Error("INVALID_FLOAT", $"Invalid floating‑point number: '{span.ToString()}'");
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAllDigits(ReadOnlySpan<char> span, out bool negative, out int start)
    {
        negative = false;
        start = 0;

        if (span[0] == '-')
        {
            negative = true;
            start = 1;
            if (span.Length == 1)
                return false;
        }

        for (int i = start; i < span.Length; i++)
            if (!char.IsDigit(span[i]))
                return false;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidVariableName(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
            return false;
        // Single letter, optional digits/underscores after first char (for future)
        if (!char.IsLetter(span[0]))
            return false;
        for (int i = 1; i < span.Length; i++)
            if (!char.IsLetterOrDigit(span[i]) && span[i] != '_')
                return false;
        return true;
    }

    // ---------- Equation Finalization ----------
    private Result<LinearEquation> BuildEquation(
        Dictionary<Variable, double> coefficients,
        double constant,
        bool hasEquals,
        double pendingCoefficient,
        bool expectingCoefficient)
    {
        // Note: constant is already accumulated during parsing
        // No need to modify it here

        if (coefficients.Count == 0)
            return Result<LinearEquation>.Failure(
                new Error("Equation must contain at least one variable.", "NO_VARIABLES"));

        var coeffList = coefficients.Select(kvp => new Coefficient(kvp.Value, kvp.Key));
        return Result<LinearEquation>.Success(new LinearEquation(coeffList, constant));
    }

    // ---------- Async ----------
    public Task<Result<EquationSystem>> ParseAsync(string input, CancellationToken cancellationToken = default)
        => Task.Run(() => Parse(input), cancellationToken);
}

// ---------- Helper: Disposable TokenizedResult ----------
internal readonly ref struct TokenizedResult
{
    private readonly TokenInfo[]? _rentedBuffer;
    private readonly ArrayPool<TokenInfo>? _pool;
    public ReadOnlySpan<TokenInfo> Tokens { get; }

    // For stackalloc path (no disposal)
    public TokenizedResult(Span<TokenInfo> tokens, bool ownsBuffer)
    {
        var arr = new TokenInfo[tokens.Length];
        tokens.CopyTo(arr);
        Tokens = arr;
        _rentedBuffer = null;
        _pool = null;
    }

    // For pooled array path
    public TokenizedResult(TokenInfo[] buffer, int count, ArrayPool<TokenInfo> pool)
    {
        _rentedBuffer = buffer;
        _pool = pool;
        Tokens = new ReadOnlySpan<TokenInfo>(buffer, 0, count);
    }

    public void Dispose()
    {
        if (_rentedBuffer != null && _pool != null)
        {
            _pool.Return(_rentedBuffer, clearArray: true);
        }
    }
}