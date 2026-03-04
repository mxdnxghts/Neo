using BenchmarkDotNet.Attributes;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Infrastructure.Parsing;

namespace NeoBenchmark.Parsing;

/// <summary>
/// Benchmarks for equation parsing performance.
/// </summary>
[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class ParsingBenchmarks
{
    private EquationParser _parser = null!;
    
    // Simple cases
    private readonly string _simple2x2 = "2x + 3y = 5; x - y = 1";
    private readonly string _simple3x3 = "x + y + z = 6; 2x - y + z = 3; x + 2y - z = 2";
    
    // Complex cases
    private readonly string _decimals = "1.5x + 2.75y = 3.125; 0.5x - 1.25y = 0.875";
    private readonly string _negatives = "-2x - 3y = -5; -x + y = -1";
    private readonly string _sparse = "2x + 0y + 3z = 5; 0x + 4y - z = 2; x + 0y + 0z = 1";
    private readonly string _zeroCoefficients = "0x + 0y = 5; 3x - y = 2";
    
    // Edge cases
    private readonly string _extraSpaces = "  2x   +   3y   =   5  ;  x  -  y  =  1  ";
    private readonly string _mixedFormat = "2x+3y=5; x-y=1";
    
    // Large cases
    private string _large5x5 = null!;
    
    // Invalid cases
    private readonly string _invalidSyntax = "invalid equation";
    private readonly string _missingVariable = "2 + 3y = 5; x - y = 1";
    private readonly string _unbalanced = "2x + 3y = 5";

    [GlobalSetup]
    public void Setup()
    {
        _parser = new EquationParser();
        _large5x5 = GenerateLargeEquationString(5);
    }

    // Simple benchmarks
    [Benchmark(Baseline = true)]
    public Result<EquationSystem> Parse_Simple2x2() 
        => _parser.Parse(_simple2x2);

    [Benchmark]
    public Result<EquationSystem> Parse_Simple3x3() 
        => _parser.Parse(_simple3x3);

    // Complex coefficient benchmarks
    [Benchmark]
    public Result<EquationSystem> Parse_Decimals() 
        => _parser.Parse(_decimals);

    [Benchmark]
    public Result<EquationSystem> Parse_Negatives() 
        => _parser.Parse(_negatives);

    [Benchmark]
    public Result<EquationSystem> Parse_Sparse() 
        => _parser.Parse(_sparse);

    [Benchmark]
    public Result<EquationSystem> Parse_ZeroCoefficients() 
        => _parser.Parse(_zeroCoefficients);

    // Format variation benchmarks
    [Benchmark]
    public Result<EquationSystem> Parse_ExtraSpaces() 
        => _parser.Parse(_extraSpaces);

    [Benchmark]
    public Result<EquationSystem> Parse_MixedFormat() 
        => _parser.Parse(_mixedFormat);

    // Large system benchmark
    [Benchmark]
    public Result<EquationSystem> Parse_Large5x5() 
        => _parser.Parse(_large5x5);

    // Invalid input benchmarks
    [Benchmark]
    public Result<EquationSystem> Parse_InvalidSyntax() 
        => _parser.Parse(_invalidSyntax);

    [Benchmark]
    public Result<EquationSystem> Parse_MissingVariable() 
        => _parser.Parse(_missingVariable);

    [Benchmark]
    public Result<EquationSystem> Parse_Unbalanced() 
        => _parser.Parse(_unbalanced);

    private static string GenerateLargeEquationString(int n)
    {
        var equations = new List<string>();
        for (int i = 0; i < n; i++)
        {
            var terms = new List<string>();
            for (int j = 0; j < n; j++)
            {
                var coeff = i * n + j + 1;
                terms.Add($"{coeff}x{j + 1}");
            }
            var constant = n * n + i + 1;
            equations.Add($"{string.Join(" + ", terms)} = {constant}");
        }
        return string.Join("; ", equations);
    }
}
