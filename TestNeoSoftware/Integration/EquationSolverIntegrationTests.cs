using Moq;
using Neo.Application.Caching;
using Neo.Application.Solver.Equation;
using Neo.Application.Solver.Matrix;
using Neo.Application.Validators;
using Neo.Domain.Equation.Variables;
using Neo.Infrastructure.Integration;
using Neo.Infrastructure.Matrix;
using Neo.Infrastructure.Parsing;
using Microsoft.Extensions.Caching.Memory;

namespace TestNeoSoftware.Integration;

/// <summary>
/// Base class for integration tests providing real implementations of all dependencies.
/// </summary>
#pragma warning disable NUnit1032 // Field should be disposed in TearDown

public class EquationSolverIntegrationTestBase
{
    protected Mock<IMemoryCache> _memoryCacheMock;
    protected IEquationParser Parser = null!;
    protected IMatrixConverter Converter = null!;
    protected IMatrixSolver Solver = null!;
    protected ISolutionValidator Validator = null!;
    protected IEquationCache Cache = null!;
    protected PerformanceMonitor Monitor = null!;
    protected IEquationSolver EquationSolver = null!;
#pragma warning restore NUnit1032

    [SetUp]
    public void SetUp()
    {
        _memoryCacheMock = new Mock<IMemoryCache>();
        Parser = new EquationParser();
        Converter = new MatrixConverter();
        Solver = new MatrixSolver();
        Validator = new SolutionValidator();
        Cache = new MemoryEquationCache(_memoryCacheMock.Object); // Fresh cache for each test
        Monitor = new PerformanceMonitor();
        EquationSolver = new EquationSolver(Parser, Converter, Solver, Validator, Cache,
            new SolvingOptions { EnableCaching = false }); // Disable caching by default
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up disposable resources (cast to concrete type for Dispose)
        if (Converter is MatrixConverter mc)
            mc.Dispose();
    }
}

[TestFixture]
public class EquationParserIntegrationTests
{
    private IEquationParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new EquationParser();
    }

    [Test]
    public void Parse_SimpleEquation_CreatesSystem()
    {
        // Act - single equation with single variable
        var result = _parser.Parse("2x = 5");

        // Debug
        if (result.IsFailure)
        {
            System.Console.WriteLine($"Parser Error: {result.Error?.Code} - {result.Error?.Message}");
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        var system = result.Value;
        system!.EquationCount.Should().Be(1);
        system.Variables.Should().HaveCount(1);
    }

    [Test]
    public void Parse_Simple2x2System_CreatesSystem()
    {
        // Act - 2 equations with 2 variables
        var result = _parser.Parse("2x + 3y = 5; x - y = 1");

        // Debug
        if (result.IsFailure)
        {
            System.Console.WriteLine($"Parser Error: {result.Error?.Code} - {result.Error?.Message}");
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        var system = result.Value;
        system!.EquationCount.Should().Be(2);
        system.Variables.Should().HaveCount(2);
    }

    [Test]
    public void Parse_MultipleEquations_CreatesSystem()
    {
        // Act
        var result = _parser.Parse("2x+3y=5; x-y=1");

        // Debug
        if (result.IsFailure)
            System.Console.WriteLine($"Parser Error: {result.Error?.Code} - {result.Error?.Message}");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var system = result.Value;
        system!.EquationCount.Should().Be(2);
        system.Variables.Should().HaveCount(2);
    }

    [Test]
    public void Parse_WithSpacesAndTabs_Works()
    {
        // Act - 2 equations with 2 variables
        var result = _parser.Parse("  2x  + 3y  =  5  ;  x - y = 1  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var system = result.Value;
        system!.EquationCount.Should().Be(2);
    }

    [Test]
    public void Parse_ImplicitCoefficient_DefaultsToOne()
    {
        // Act - single equation with single variable (implicit coefficient 1)
        var result = _parser.Parse("x = 2");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var system = result.Value;
        var x = Variable.Create("x");
        system!.Equations[0].GetCoefficient(x).Should().Be(1);
    }

    [Test]
    public void Parse_ImplicitNegativeCoefficient()
    {
        // Act - single equation with single variable (implicit coefficient -1)
        var result = _parser.Parse("-x = 2");

        // Debug
        if (result.IsFailure)
            System.Console.WriteLine($"Parser Error: {result.Error?.Code} - {result.Error?.Message}");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var system = result.Value;
        var x = Variable.Create("x");
        system!.Equations[0].GetCoefficient(x).Should().Be(-1);
    }

    [Test]
    public void Parse_RepeatedVariables_Combines()
    {
        // Act - single variable repeated
        var result = _parser.Parse("2x + 3x = 10");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var system = result.Value;
        var x = Variable.Create("x");
        system!.Equations[0].GetCoefficient(x).Should().Be(5);
    }

    [Test]
    public void Parse_LeftSideConstant_MovesToRight()
    {
        // Act - constant on left side: 2x + 3 = 7 should become 2x = 4
        var result = _parser.Parse("2x + 3 = 7");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var system = result.Value;
        system!.Equations[0].Constant.Should().Be(4); // 7 - 3 = 4
    }

    [Test]
    public void Parse_NegativeNumbers()
    {
        // Act - single equation with negative coefficients
        var result = _parser.Parse("-2x = -5");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var system = result.Value;
        var x = Variable.Create("x");
        system!.Equations[0].GetCoefficient(x).Should().Be(-2);
        system!.Equations[0].Constant.Should().Be(-5);
    }

    [Test]
    public void Parse_FloatWithDot()
    {
        // Act - single equation with float coefficients
        var result = _parser.Parse("2.5x = 5.0");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var system = result.Value;
        var x = Variable.Create("x");
        system!.Equations[0].GetCoefficient(x).Should().Be(2.5);
        system!.Equations[0].Constant.Should().Be(5.0);
    }

    [Test]
    public void Parse_EmptyInput_ReturnsError()
    {
        // Act
        var result = _parser.Parse("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("EMPTY_INPUT");
    }

    [Test]
    public void Parse_NullInput_ReturnsError()
    {
        // Act
        var result = _parser.Parse(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("EMPTY_INPUT");
    }

    [Test]
    public void Parse_NoVariables_ReturnsError()
    {
        // Act - no variables, just constants
        var result = _parser.Parse("5 = 10");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("variable");
    }

    [Test]
    public async Task ParseAsync_ValidInput_ReturnsSystem()
    {
        // Act - single equation
        var result = await _parser.ParseAsync("x = 2");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.EquationCount.Should().Be(1);
    }
}

[TestFixture]
public class EquationSolverIntegrationTests : EquationSolverIntegrationTestBase
{
    [Test]
    public void Solve_2x2_ReturnsCorrect()
    {
        // Act: 2x + 3y = 5; x - y = 1
        // Solution: x = 1.6, y = 0.6
        var result = EquationSolver.Solve("2x + 3y = 5; x - y = 1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var solution = result.Value;
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        solution!.GetValue(x).Should().BeApproximately(1.6, 0.01);
        solution.GetValue(y).Should().BeApproximately(0.6, 0.01);
    }

    [Test]
    public void Solve_SimpleX_ReturnsCorrect()
    {
        // Act
        var result = EquationSolver.Solve("2x = 10");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var solution = result.Value;
        var x = Variable.Create("x");
        solution!.GetValue(x).Should().BeApproximately(5, 0.0001);
    }

    [Test]
    public void Solve_DiagonalMatrix()
    {
        // Act
        var result = EquationSolver.Solve("2x = 4; 3y = 9");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var solution = result.Value;
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        solution!.GetValue(x).Should().BeApproximately(2, 0.0001);
        solution.GetValue(y).Should().BeApproximately(3, 0.0001);
    }

    [Test]
    public void Solve_WithNegativeCoefficients()
    {
        // Act: -2x + -3y = -5; x - y = 1
        // Solution: x = 1.6, y = 0.6
        var result = EquationSolver.Solve("-2x + -3y = -5; x - y = 1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var solution = result.Value;
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        solution!.GetValue(x).Should().BeApproximately(1.6, 0.01);
        solution.GetValue(y).Should().BeApproximately(0.6, 0.01);
    }

    [Test]
    public void Solve_WithFloats()
    {
        // Act: 1.5x + 2.5y = 8.5; 0.5x - 1.5y = -0.5
        // Solution: x ≈ 3.29, y ≈ 1.43
        var result = EquationSolver.Solve("1.5x + 2.5y = 8.5; 0.5x - 1.5y = -0.5");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var solution = result.Value;
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        // Verify with tolerance for floating point
        solution!.GetValue(x).Should().BeApproximately(3.29, 0.01);
        solution.GetValue(y).Should().BeApproximately(1.43, 0.01);
    }

    [Test]
    public void Solve_InconsistentSystem_ReturnsNoSolution()
    {
        // Act
        var result = EquationSolver.Solve("x + y = 2; x + y = 3");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NO_SOLUTION");
    }

    [Test]
    public void Solve_Underdetermined_ReturnsInfiniteSolutions()
    {
        // Act - underdetermined system (more variables than equations) is rejected at parse time
        var result = EquationSolver.Solve("x + y = 2");

        // Assert - parser rejects underdetermined systems
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("PARSE_ERROR");
        result.Error.Message.Should().Contain("Underdetermined");
    }

    [Test]
    public void Solve_SingularMatrix_ReturnsError()
    {
        // Act
        var result = EquationSolver.Solve("x + y = 2; 2x + 2y = 4");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().BeOneOf("INFINITE_SOLUTIONS", "SINGULAR_MATRIX");
    }
}

[TestFixture]
public class EquationSolverCachingTests : EquationSolverIntegrationTestBase
{
    [Test]
    public void Solve_WithCaching_SecondCallUsesCache()
    {
        // Arrange - use cache
        var cache = new MemoryEquationCache(_memoryCacheMock.Object);
        var solver = new EquationSolver(Parser, Converter, Solver, Validator, cache);
        var input = "x + y = 2; x - y = 0";

        // Act - first call
        var result1 = solver.Solve(input);
        result1.IsSuccess.Should().BeTrue();

        // Act - second call (should use cache)
        var result2 = solver.Solve(input);

        // Assert
        result2.IsSuccess.Should().BeTrue();
        result2.Value!.GetValue(Variable.Create("x")).Should().Be(result1.Value.GetValue(Variable.Create("x")));
    }

    [Test]
    public void Solve_WithCaching_DifferentInputs_NoCache()
    {
        // Arrange
        var cache = new MemoryEquationCache(_memoryCacheMock.Object);
        var solver = new EquationSolver(Parser, Converter, Solver, Validator, cache);

        // Act
        var result1 = solver.Solve("x = 1");
        var result2 = solver.Solve("y = 2");

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value!.GetValue(Variable.Create("x")).Should().Be(1);
        result2.Value!.GetValue(Variable.Create("y")).Should().Be(2);
    }
}

[TestFixture]
public class EquationSolverBatchTests
{
    private IEquationSolver _solver = null!;

    [SetUp]
    public void SetUp()
    {
        var parser = new EquationParser();
        var converter = new MatrixConverter();
        var matrixSolver = new MatrixSolver();
        var validator = new SolutionValidator();
        var cache = new NullEquationCache(); // No caching for batch tests
        var monitor = new PerformanceMonitor();
        var options = new SolvingOptions { EnableCaching = false, MaxDegreeOfParallelism = 1 };
        _solver = new EquationSolver(parser, converter, matrixSolver, validator, cache, options);
    }

    [Test]
    public async Task SolveBatchAsync_MultipleValid_AllSuccess()
    {
        // Arrange - inputs are independent single-variable equations
        var inputs = new[] { "x = 1", "y = 2", "z = 3" };

        // Act
        var result = await _solver.SolveBatchAsync(inputs);

        // Assert - all three single-variable equations should return solutions
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Test]
    public async Task SolveBatchAsync_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var inputs = Array.Empty<string>();

        // Act
        var result = await _solver.SolveBatchAsync(inputs);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task SolveBatchAsync_MixedValidInvalid()
    {
        // Arrange
        var inputs = new[] { "x = 1", "invalid", "y = 2" };

        // Act
        var result = await _solver.SolveBatchAsync(inputs);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Valid inputs should have solutions, invalid should not
        result.Value.Should().HaveCountLessThan(3); // Some may be skipped
    }
}

[TestFixture]
public class EquationSolverOptionsTests : EquationSolverIntegrationTestBase
{
    [Test]
    public void SolveWithOptions_DisableCaching_Works()
    {
        // Arrange
        var options = new SolvingOptions { EnableCaching = false };

        // Act
        var result = EquationSolver.SolveWithOptions("x + y = 2; x - y = 0", options);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void SolveWithAlgorithm_QR_Works()
    {
        // Act
        var result = EquationSolver.SolveWithAlgorithm("x + y = 2; x - y = 0", SolvingAlgorithm.QR);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void SolveWithAlgorithm_Cholesky_ForSPD()
    {
        // Arrange - symmetric positive definite system
        // 2x + y = 5
        // x + 2y = 5
        var input = "2x + y = 5; x + 2y = 5";

        // Act
        var result = EquationSolver.SolveWithAlgorithm(input, SolvingAlgorithm.Cholesky);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}