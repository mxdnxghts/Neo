using Neo.Domain.Equation;
using Neo.Domain.Equation.Variables;

namespace TestNeoSoftware.Domain.Equation;

[TestFixture]
public class EquationSystemTests
{
    [Test]
    public void Constructor_ValidEquations_Creates()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var equations = new[]
        {
            new LinearEquation(new[] { new Coefficient(2, x), new Coefficient(3, y) }, 5),
            new LinearEquation(new[] { new Coefficient(1, x), new Coefficient(-1, y) }, 1)
        };

        // Act
        var system = new EquationSystem(equations);

        // Assert
        system.EquationCount.Should().Be(2);
        system.Equations.Should().HaveCount(2);
        system.Variables.Should().HaveCount(2);
    }

    [Test]
    public void Constructor_NullEquations_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new EquationSystem(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("equations");
    }

    [Test]
    public void Constructor_EmptyEquations_ThrowsArgumentException()
    {
        // Act
        Action act = () => new EquationSystem(Array.Empty<LinearEquation>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one equation*");
    }

    [Test]
    public void Constructor_TooManyEquations_ThrowsInvalidOperationException()
    {
        // Arrange
        var x = Variable.Create("x");
        var equations = Enumerable.Range(0, 101)
            .Select(i => new LinearEquation(new[] { new Coefficient(1, x) }, i))
            .ToList();

        // Act
        Action act = () => new EquationSystem(equations);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Too many equations*");
    }

    [Test, Ignore("Cannot create more than 26 unique single-letter variables")]
    public void Constructor_TooManyVariables_ThrowsInvalidOperationException()
    {
        // Arrange - create 27 variables (more than 26 limit)
        // Note: This test is ignored because Variable only allows single-letter names (a-z)
        // making it impossible to create more than 26 unique variables.
        var variables = new List<Variable>();
        for (int i = 0; i < 27; i++)
        {
            char varName = (char)('a' + (i % 26));
            variables.Add(Variable.Create(varName.ToString()));
        }
        var equations = variables
            .Select(v => new LinearEquation(new[] { new Coefficient(1, v) }, 1))
            .ToList();

        // Act
        Action act = () => new EquationSystem(equations);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Too many variables*");
    }

    [Test]
    public void Constructor_MoreVariablesThanEquations_ThrowsInvalidOperationException()
    {
        // Arrange - 1 equation, 2 variables
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var equations = new[]
        {
            new LinearEquation(new[] { new Coefficient(1, x), new Coefficient(1, y) }, 5)
        };

        // Act
        Action act = () => new EquationSystem(equations);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Underdetermined*");
    }

    [Test]
    public void Normalize_AddsZeroCoefficients()
    {
        // Arrange - 2 equations with different variables
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var equations = new[]
        {
            new LinearEquation(new[] { new Coefficient(2, x) }, 5),
            new LinearEquation(new[] { new Coefficient(3, y) }, 7)
        };
        var system = new EquationSystem(equations);

        // Act
        var normalized = system.Normalize();

        // Assert
        foreach (var equation in normalized.Equations)
        {
            equation.HasVariable(x).Should().BeTrue();
            equation.HasVariable(y).Should().BeTrue();
        }
    }

    [Test]
    public void ToMatrix_ReturnsCorrectArrays()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var equations = new[]
        {
            new LinearEquation(new[] { new Coefficient(2, x), new Coefficient(3, y) }, 5),
            new LinearEquation(new[] { new Coefficient(1, x), new Coefficient(-1, y) }, 1)
        };
        var system = new EquationSystem(equations);

        // Act
        var (coefficients, constants) = system.ToMatrix();

        // Assert
        coefficients.GetLength(0).Should().Be(2); // 2 equations
        coefficients.GetLength(1).Should().Be(2); // 2 variables
        coefficients[0, 0].Should().Be(2); // 2x
        coefficients[0, 1].Should().Be(3); // 3y
        coefficients[1, 0].Should().Be(1); // 1x
        coefficients[1, 1].Should().Be(-1); // -1y
        constants[0].Should().Be(5);
        constants[1].Should().Be(1);
    }

    [Test]
    public void IsSquare_EqualEquationsAndVariables_ReturnsTrue()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var equations = new[]
        {
            new LinearEquation(new[] { new Coefficient(1, x), new Coefficient(1, y) }, 5),
            new LinearEquation(new[] { new Coefficient(1, x), new Coefficient(-1, y) }, 1)
        };
        var system = new EquationSystem(equations);

        // Act & Assert
        system.IsSquare.Should().BeTrue();
    }

    [Test]
    public void IsSquare_MoreEquationsThanVariables_ReturnsFalse()
    {
        // Arrange - 3 equations, 2 variables (overdetermined but still valid)
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var equations = new[]
        {
            new LinearEquation(new[] { new Coefficient(1, x), new Coefficient(1, y) }, 5),
            new LinearEquation(new[] { new Coefficient(1, x), new Coefficient(-1, y) }, 1),
            new LinearEquation(new[] { new Coefficient(2, x), new Coefficient(1, y) }, 8)
        };
        var system = new EquationSystem(equations);

        // Act & Assert
        system.IsSquare.Should().BeFalse();
    }

    [Test]
    public void ToString_ReturnsEquationsJoinedByNewline()
    {
        // Arrange
        var x = Variable.Create("x");
        var equations = new[]
        {
            new LinearEquation(new[] { new Coefficient(1, x) }, 5)
        };
        var system = new EquationSystem(equations);

        // Act & Assert
        system.ToString().Should().Contain("x = 5");
    }
}