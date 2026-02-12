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
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Parsing;

public class EquationParser : IEquationParser
{
    private readonly PerformanceMonitor? _performanceMonitor;
    private readonly ArrayPool<LinearEquation> _equationPool;

    public EquationParser(PerformanceMonitor? performanceMonitor = null)
    {
        _performanceMonitor = performanceMonitor;
        _equationPool = ArrayPool<LinearEquation>.Shared;
    }

    public Result<EquationSystem> Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result<EquationSystem>.Failure(Error.EmptyInput);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var span = input.AsSpan();

            // Use stack allocation for token buffer
            Span<TokenInfo> tokenBuffer = stackalloc TokenInfo[128];
            var tokenizer = new EquationTokenizer(span, tokenBuffer);
            var tokens = tokenizer.Tokenize();

            // Parse tokens
            var result = ParseTokens(tokens, input.AsMemory());

            stopwatch.Stop();
            _performanceMonitor?.RecordOperation("Parse", stopwatch.Elapsed, result.IsSuccess);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _performanceMonitor?.RecordOperation("Parse", stopwatch.Elapsed, false);

            return Result<EquationSystem>.Failure(
                new Error($"Parsing failed: {ex.Message}", "PARSE_ERROR", ex));
        }
    }

    private Result<EquationSystem> ParseTokens(ReadOnlySpan<TokenInfo> tokens, ReadOnlyMemory<char> source)
    {
        var equations = _equationPool.Rent(10);
        var equationCount = 0;

        try
        {
            var position = 0;

            while (position < tokens.Length && tokens[position].Type != TokenType.End)
            {
                var equationResult = ParseEquation(tokens, source, ref position);
                if (equationResult.IsFailure)
                    return Result<EquationSystem>.Failure(equationResult.Error);

                // Ensure capacity
                if (equationCount >= equations.Length)
                {
                    var newArray = _equationPool.Rent(equations.Length * 2);
                    Array.Copy(equations, newArray, equationCount);
                    _equationPool.Return(equations);
                    equations = newArray;
                }

                equations[equationCount++] = equationResult.Value;

                // Skip separator if present
                if (position < tokens.Length && tokens[position].Type == TokenType.Separator)
                    position++;
            }

            // Create final array with exact size
            var finalEquations = new LinearEquation[equationCount];
            Array.Copy(equations, finalEquations, equationCount);

            return Result<EquationSystem>.Success(new EquationSystem(finalEquations));
        }
        finally
        {
            _equationPool.Return(equations);
        }
    }

    private Result<LinearEquation> ParseEquation(
        ReadOnlySpan<TokenInfo> tokens,
        ReadOnlyMemory<char> source,
        ref int position)
    {
        var coefficients = new Dictionary<Variable, double>();
        double constant = 0;
        bool hasEquals = false;
        bool isNegative = false;
        bool expectingCoefficient = false;
        double pendingCoefficient = 0;

        while (position < tokens.Length)
        {
            var token = tokens[position];

            switch (token.Type)
            {
                case TokenType.Number:
                    var numberSpan = source.Span.Slice(token.Start, token.Length);
                    var numberValue = FastParseNumber(numberSpan);

                    if (isNegative)
                    {
                        numberValue = -numberValue;
                        isNegative = false;
                    }

                    pendingCoefficient = numberValue;
                    expectingCoefficient = true;
                    position++;
                    break;

                case TokenType.Variable:
                    var variableSpan = source.Span.Slice(token.Start, token.Length);
                    var variableName = variableSpan.ToString();
                    var variable = Variable.Create(variableName);

                    // Determine coefficient value
                    double coefficientValue;
                    if (expectingCoefficient)
                    {
                        coefficientValue = pendingCoefficient;
                        expectingCoefficient = false;
                    }
                    else
                    {
                        // Implicit coefficient of 1 (or -1)
                        coefficientValue = isNegative ? -1 : 1;
                        isNegative = false;
                    }

                    // Handle existing coefficient for same variable
                    if (coefficients.TryGetValue(variable, out var existingCoefficient))
                    {
                        coefficients[variable] = existingCoefficient + coefficientValue;
                    }
                    else
                    {
                        coefficients[variable] = coefficientValue;
                    }

                    position++;
                    break;

                case TokenType.Operator:
                    isNegative = true; // Only minus is an operator in this context
                    position++;
                    break;

                case TokenType.Equals:
                    hasEquals = true;

                    // If we have a pending coefficient before equals, it's a constant on left side
                    if (expectingCoefficient)
                    {
                        constant = -pendingCoefficient; // Move to right side with sign change
                        expectingCoefficient = false;
                    }

                    position++;
                    break;

                case TokenType.Separator:
                case TokenType.End:
                    // End of equation
                    return FinalizeEquation(coefficients, constant, hasEquals, pendingCoefficient, expectingCoefficient);

                default:
                    position++;
                    break;
            }
        }

        return FinalizeEquation(coefficients, constant, hasEquals, pendingCoefficient, expectingCoefficient);
    }

    private static double FastParseNumber(ReadOnlySpan<char> span)
    {
        ReadOnlySpan<char> dotChar = ".".AsSpan();
        // Fast path for integers
        if (!span.Contains(".".AsSpan(), StringComparison.CurrentCultureIgnoreCase) &&
            !span.Contains(",".AsSpan(), StringComparison.CurrentCultureIgnoreCase))
        {
            return FastParseInteger(span);
        }

        // Fall back to double.Parse for floats
        return double.Parse(span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    private static double FastParseInteger(ReadOnlySpan<char> span)
    {
        int result = 0;
        bool negative = false;
        int start = 0;

        if (span[0] == '-')
        {
            negative = true;
            start = 1;
        }

        for (int i = start; i < span.Length; i++)
        {
            result = result * 10 + (span[i] - '0');
        }

        return negative ? -result : result;
    }

    private Result<LinearEquation> FinalizeEquation(
        Dictionary<Variable, double> coefficients,
        double constant,
        bool hasEquals,
        double pendingCoefficient,
        bool expectingCoefficient)
    {
        // Handle trailing coefficient (constant on right side)
        if (hasEquals && expectingCoefficient)
        {
            constant = pendingCoefficient;
        }

        if (coefficients.Count == 0)
            return Result<LinearEquation>.Failure(
                new Error("Equation has no variables", "NO_VARIABLES"));

        var coefficientList = coefficients.Select(kvp => new Coefficient(kvp.Value, kvp.Key));
        return Result<LinearEquation>.Success(new LinearEquation(coefficientList, constant));
    }

    public async Task<Result<EquationSystem>> ParseAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Parse(input), cancellationToken);
    }
}