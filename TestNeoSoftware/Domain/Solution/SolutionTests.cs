using Neo.Domain.Equation;
using Neo.Domain.Equation.Variables;
using NeoDomainSolution = Neo.Domain.Solution;
using FluentAssertions;

namespace TestNeoSoftware.Domain.Solution;

[TestFixture]
public class SolutionTests
{
    private EquationSystem _system = null!;
    private Variable _x = null!;
    private Variable _y = null!;

    [SetUp]
    public void SetUp()
    {
        _x = Variable.Create("x");
        _y = Variable.Create("y");
        var equations = new[]
        {
            new LinearEquation(new[] { new Coefficient(1, _x), new Coefficient(1, _y) }, 5),
            new LinearEquation(new[] { new Coefficient(1, _x), new Coefficient(-1, _y) }, 1)
        };
        _system = new EquationSystem(equations);
    }

    [Test]
    public void Success_CreatesSuccessSolution()
    {
        // Arrange
        var values = new Dictionary<Variable, double> { { _x, 2 }, { _y, 1 } };

        // Act
        var solution = NeoDomainSolution.Solution.Success(_system, values);

        // Assert
        solution.Status.Should().Be(NeoDomainSolution.SolutionStatus.Success);
        solution.Values.Should().ContainKey(_x).WhoseValue.Should().Be(2);
        solution.Values.Should().ContainKey(_y).WhoseValue.Should().Be(1);
    }

    [Test]
    public void NoSolution_CreatesNoSolution()
    {
        // Act
        var solution = NeoDomainSolution.Solution.NoSolution(_system, "Inconsistent system");

        // Assert
        solution.Status.Should().Be(NeoDomainSolution.SolutionStatus.NoSolution);
        solution.Values.Should().BeEmpty();
        solution.Message.Should().Be("Inconsistent system");
    }

    [Test]
    public void InfiniteSolutions_CreatesInfiniteSolutions()
    {
        // Act
        var solution = NeoDomainSolution.Solution.InfiniteSolutions(_system, "Dependent equations");

        // Assert
        solution.Status.Should().Be(NeoDomainSolution.SolutionStatus.InfiniteSolutions);
        solution.Values.Should().BeEmpty();
        solution.Message.Should().Be("Dependent equations");
    }

    [Test]
    public void Error_CreatesError()
    {
        // Act
        var solution = NeoDomainSolution.Solution.Error(_system, "Solving failed");

        // Assert
        solution.Status.Should().Be(NeoDomainSolution.SolutionStatus.Error);
        solution.Values.Should().BeEmpty();
        solution.Message.Should().Be("Solving failed");
    }

    [Test]
    public void GetValue_ExistingVariable_ReturnsValue()
    {
        // Arrange
        var values = new Dictionary<Variable, double> { { _x, 2 }, { _y, 1 } };
        var solution = NeoDomainSolution.Solution.Success(_system, values);

        // Act
        var value = solution.GetValue(_x);

        // Assert
        value.Should().Be(2);
    }

    [Test]
    public void GetValue_MissingVariable_ThrowsKeyNotFoundException()
    {
        // Arrange
        var z = Variable.Create("z");
        var values = new Dictionary<Variable, double> { { _x, 2 } };
        var solution = NeoDomainSolution.Solution.Success(_system, values);

        // Act
        Action act = () => solution.GetValue(z);

        // Assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*z*");
    }

    [Test]
    public void ToString_Success_ReturnsVariableValues()
    {
        // Arrange
        var values = new Dictionary<Variable, double> { { _x, 2 }, { _y, 1 } };
        var solution = NeoDomainSolution.Solution.Success(_system, values);

        // Act & Assert
        var result = solution.ToString();
        result.Should().Contain("x = 2");
        result.Should().Contain("y = 1");
    }

    [Test]
    public void ToString_NoSolution_ReturnsStatusAndMessage()
    {
        // Arrange
        var solution = NeoDomainSolution.Solution.NoSolution(_system, "Test message");

        // Act & Assert
        solution.ToString().Should().Contain("NoSolution");
        solution.ToString().Should().Contain("Test message");
    }

    [Test]
    public void SolvedAt_IsSetToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var solution = NeoDomainSolution.Solution.Success(_system, new Dictionary<Variable, double>());

        // Assert
        var after = DateTime.UtcNow;
        solution.SolvedAt.Should().BeOnOrAfter(before);
        solution.SolvedAt.Should().BeOnOrBefore(after);
    }

    [Test]
    public void OriginalSystem_IsStored()
    {
        // Act
        var solution = NeoDomainSolution.Solution.Success(_system, new Dictionary<Variable, double>());

        // Assert
        solution.OriginalSystem.Should().Be(_system);
    }
}
