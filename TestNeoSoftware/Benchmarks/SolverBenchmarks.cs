using BenchmarkDotNet.Attributes;
using Neo.Application.Caching;
using Neo.Application.Solver.Equation;
using Neo.Application.Solver.Matrix;
using Neo.Application.Validators;
using Neo.Domain.Equation;
using Neo.Infrastructure.Integration;
using Neo.Infrastructure.Matrix;
using Neo.Infrastructure.Parsing;

namespace TestNeoSoftware.Benchmarks;

[MemoryDiagnoser]
public class EquationParserBenchmark
{
    private string _smallInput = null!;
    private string _mediumInput = null!;
    private string _largeInput = null!;
    private IEquationParser _parser = null!;

    [GlobalSetup]
    public void Setup()
    {
        _parser = new EquationParser();
        _smallInput = "2x + 3y = 5; x - y = 1";
        _mediumInput = GenerateLargeSystem(100);
        _largeInput = GenerateLargeSystem(1000);
    }

    [Benchmark]
    public void ParseSmall() => _parser.Parse(_smallInput);

    [Benchmark]
    public void ParseMedium() => _parser.Parse(_mediumInput);

    [Benchmark]
    public void ParseLarge() => _parser.Parse(_largeInput);

    private string GenerateLargeSystem(int count)
    {
        var equations = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var vars = new List<string>();
            for (int j = 0; j < Math.Min(count, 5); j++)
            {
                var varName = ((char)('a' + (j % 26))).ToString();
                var coeff = (j + 1) * (i + 1);
                vars.Add($"{coeff}{varName}");
            }
            equations.Add(string.Join(" + ", vars) + $" = {i * 10}");
        }
        return string.Join("; ", equations);
    }
}

[MemoryDiagnoser]
public class MatrixConverterBenchmark
{
    private EquationSystem _smallSystem = null!;
    private EquationSystem _mediumSystem = null!;
    private EquationSystem _largeSystem = null!;
    private IMatrixConverter _converter = null!;

    [GlobalSetup]
    public void Setup()
    {
        _converter = new MatrixConverter();
        _smallSystem = CreateSystem(2);
        _mediumSystem = CreateSystem(50);
        _largeSystem = CreateSystem(200);
    }

    [Benchmark]
    public void ConvertSmall() => _converter.ToArrays(_smallSystem);

    [Benchmark]
    public void ConvertMedium() => _converter.ToArrays(_mediumSystem);

    [Benchmark]
    public void ConvertLarge() => _converter.ToArrays(_largeSystem);

    private EquationSystem CreateSystem(int size)
    {
        var parser = new EquationParser();
        var equations = new List<string>();
        for (int i = 0; i < size; i++)
        {
            var vars = new List<string>();
            for (int j = 0; j < size; j++)
            {
                if (i == j)
                {
                    vars.Add($"2x{j}");
                }
                else if (Math.Abs(i - j) == 1)
                {
                    vars.Add($"-1x{j}");
                }
            }
            equations.Add(string.Join(" + ", vars) + $" = {i}");
        }
        return parser.Parse(string.Join("; ", equations)).Value!;
    }
}

[MemoryDiagnoser]
public class MatrixSolverBenchmark
{
    private MathNet.Numerics.LinearAlgebra.Matrix<double> _smallMatrix = null!;
    private MathNet.Numerics.LinearAlgebra.Vector<double> _smallVector = null!;
    private MathNet.Numerics.LinearAlgebra.Matrix<double> _mediumMatrix = null!;
    private MathNet.Numerics.LinearAlgebra.Vector<double> _mediumVector = null!;
    private MathNet.Numerics.LinearAlgebra.Matrix<double> _largeMatrix = null!;
    private MathNet.Numerics.LinearAlgebra.Vector<double> _largeVector = null!;
    private IMatrixSolver _solver = null!;

    [GlobalSetup]
    public void Setup()
    {
        _solver = new MatrixSolver();
        _smallMatrix = CreateMatrix(3);
        _smallVector = CreateVector(3);
        _mediumMatrix = CreateMatrix(50);
        _mediumVector = CreateVector(50);
        _largeMatrix = CreateMatrix(200);
        _largeVector = CreateVector(200);
    }

    [Benchmark]
    public void SolveSmallLU() => _solver.SolveLU(_smallMatrix, _smallVector);

    [Benchmark]
    public void SolveMediumLU() => _solver.SolveLU(_mediumMatrix, _mediumVector);

    [Benchmark]
    public void SolveLargeLU() => _solver.SolveLU(_largeMatrix, _largeVector);

    [Benchmark]
    public void SolveSmallQR() => _solver.SolveQR(_smallMatrix, _smallVector);

    [Benchmark]
    public void SolveMediumQR() => _solver.SolveQR(_mediumMatrix, _mediumVector);

    [Benchmark]
    public void SolveLargeQR() => _solver.SolveQR(_largeMatrix, _largeVector);

    [Benchmark]
    public void SolveSmallSVD() => _solver.SolveSVD(_smallMatrix, _smallVector);

    [Benchmark]
    public void SolveMediumSVD() => _solver.SolveSVD(_mediumMatrix, _mediumVector);

    [Benchmark]
    public void SolveLargeSVD() => _solver.SolveSVD(_largeMatrix, _largeVector);

    private MathNet.Numerics.LinearAlgebra.Matrix<double> CreateMatrix(int size)
    {
        var matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(size, size);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (i == j)
                    matrix[i, j] = size + 1; // Diagonal dominance
                else
                    matrix[i, j] = 1;
            }
        }
        return matrix;
    }

    private MathNet.Numerics.LinearAlgebra.Vector<double> CreateVector(int size)
    {
        var vector = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(size);
        for (int i = 0; i < size; i++)
        {
            vector[i] = i * 1.5;
        }
        return vector;
    }
}

[MemoryDiagnoser]
public class EquationSolverEndToEndBenchmark
{
    private IEquationSolver _solver = null!;
    private string _simpleInput = null!;
    private string _mediumInput = null!;

    [GlobalSetup]
    public void Setup()
    {
        var parser = new EquationParser();
        var converter = new MatrixConverter();
        var matrixSolver = new MatrixSolver();
        var validator = new SolutionValidator();
        var cache = new NullEquationCache();
        var monitor = new PerformanceMonitor();
        _solver = new EquationSolver(parser, converter, matrixSolver, validator, cache);

        _simpleInput = "2x + 3y = 5; x - y = 1";
        _mediumInput = GenerateSystem(20);
    }

    [Benchmark]
    public void SolveSimple() => _solver.Solve(_simpleInput);

    [Benchmark]
    public void SolveMedium() => _solver.Solve(_mediumInput);

    private string GenerateSystem(int size)
    {
        var equations = new List<string>();
        for (int i = 0; i < size; i++)
        {
            var vars = new List<string>();
            for (int j = 0; j < size; j++)
            {
                if (i == j)
                {
                    vars.Add($"2x{j}");
                }
                else if (Math.Abs(i - j) == 1)
                {
                    vars.Add($"-x{j}");
                }
            }
            equations.Add(string.Join(" + ", vars) + $" = {i * 10}");
        }
        return string.Join("; ", equations);
    }
}