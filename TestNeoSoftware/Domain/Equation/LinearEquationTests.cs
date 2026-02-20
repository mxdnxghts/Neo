using Neo.Domain.Equation;
using Neo.Domain.Equation.Variables;
using FluentAssertions;

namespace TestNeoSoftware.Domain.Equation;

[TestFixture]
public class LinearEquationTests
{
    [Test]
    public void Constructor_ValidCoefficients_Creates()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var coefficientDict = new Dictionary<Variable, double>
        {
            { x, 2 },
            { y, 3 }
        };

        // Act - use factory method
        var equation = LinearEquation.FromDictionary(coefficientDict, 5);

        // Assert
        equation.Constant.Should().Be(5);
        equation.Variables.Should().Contain(new[] { x, y });
        equation.GetCoefficient(x).Should().Be(2);
        equation.GetCoefficient(y).Should().Be(3);
    }

    [Test]
    public void Constructor_NullCoefficients_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new LinearEquation(null!, 5);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("coefficients");
    }

    [Test]
    public void Constructor_EmptyCoefficients_ThrowsInvalidOperationException()
    {
        // Act
        Action act = () => new LinearEquation(Array.Empty<Coefficient>(), 5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*at least one variable*");
    }

    [Test]
    public void Constructor_AllZeroCoefficients_ThrowsInvalidOperationException()
    {
        // Arrange
        var x = Variable.Create("x");
        var coefficients = new[] { new Coefficient(0, x) };

        // Act
        Action act = () => new LinearEquation(coefficients, 5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*non-zero*");
    }

    [Test]
    public void Constructor_Dictionary_Creates()
    {
        // Arrange
        var x = Variable.Create("x");
        var coefficients = new Dictionary<Variable, double> { { x, 2.5 } };

        // Act - use factory method
        var equation = LinearEquation.FromDictionary(coefficients, 5);

        // Assert
        equation.GetCoefficient(x).Should().Be(2.5);
    }

    [Test]
    public void GetCoefficient_ExistingVariable_ReturnsValue()
    {
        // Arrange
        var x = Variable.Create("x");
        var coefficients = new[] { new Coefficient(2, x) };
        var equation = new LinearEquation(coefficients, 5);

        // Act
        var coefficient = equation.GetCoefficient(x);

        // Assert
        coefficient.Should().Be(2);
    }

    [Test]
    public void GetCoefficient_MissingVariable_ReturnsZero()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var coefficients = new[] { new Coefficient(2, x) };
        var equation = new LinearEquation(coefficients, 5);

        // Act
        var coefficient = equation.GetCoefficient(y);

        // Assert
        coefficient.Should().Be(0);
    }

    [Test]
    public void HasVariable_ExistingVariable_ReturnsTrue()
    {
        // Arrange
        var x = Variable.Create("x");
        var coefficients = new[] { new Coefficient(2, x) };
        var equation = new LinearEquation(coefficients, 5);

        // Act & Assert
        equation.HasVariable(x).Should().BeTrue();
    }

    [Test]
    public void HasVariable_MissingVariable_ReturnsFalse()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var coefficients = new[] { new Coefficient(2, x) };
        var equation = new LinearEquation(coefficients, 5);

        // Act & Assert
        equation.HasVariable(y).Should().BeFalse();
    }

    [Test]
    public void WithZeroCoefficient_AddsMissingVariable()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var coefficients = new[] { new Coefficient(2, x) };
        var equation = new LinearEquation(coefficients, 5);

        // Act
        var newEquation = equation.WithZeroCoefficient(y);

        // Assert
        newEquation.HasVariable(y).Should().BeTrue();
        newEquation.GetCoefficient(y).Should().Be(0);
        newEquation.GetCoefficient(x).Should().Be(2); // Original preserved
    }

    [Test]
    public void WithZeroCoefficient_ExistingVariable_ReturnsSame()
    {
        // Arrange
        var x = Variable.Create("x");
        var coefficients = new[] { new Coefficient(2, x) };
        var equation = new LinearEquation(coefficients, 5);

        // Act
        var newEquation = equation.WithZeroCoefficient(x);

        // Assert
        newEquation.Should().BeSameAs(equation);
    }

    [Test]
    public void Equals_SameContent_ReturnsTrue()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var coeffs1 = new[] { new Coefficient(2, x), new Coefficient(3, y) };
        var coeffs2 = new[] { new Coefficient(2, x), new Coefficient(3, y) };
        var eq1 = new LinearEquation(coeffs1, 5);
        var eq2 = new LinearEquation(coeffs2, 5);

        // Act & Assert
        eq1.Should().BeEquivalentTo(eq2, options => options.ExcludingMissingMembers());
    }

    [Test]
    public void Equals_DifferentConstant_ReturnsFalse()
    {
        // Arrange
        var x = Variable.Create("x");
        var coeffs = new[] { new Coefficient(2, x) };
        var eq1 = new LinearEquation(coeffs, 5);
        var eq2 = new LinearEquation(coeffs, 10);

        // Act & Assert
        eq1.Should().NotBeEquivalentTo(eq2, options => options.ExcludingMissingMembers());
    }

    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");
        var coefficients = new[]
        {
            new Coefficient(2, x),
            new Coefficient(-3, y)
        };
        var equation = new LinearEquation(coefficients, 5);

        // Act & Assert
        equation.ToString().Should().Be("2x + -3y = 5");
    }

    [Test]
    public void ToString_CoefficientIsOne_OmitsOne()
    {
        // Arrange
        var x = Variable.Create("x");
        var coefficients = new[] { new Coefficient(1, x) };
        var equation = new LinearEquation(coefficients, 5);

        // Act & Assert
        equation.ToString().Should().Be("x = 5");
    }

    [Test]
    public void ToString_CoefficientIsNegativeOne_OmitsOne()
    {
        // Arrange
        var x = Variable.Create("x");
        var coefficients = new[] { new Coefficient(-1, x) };
        var equation = new LinearEquation(coefficients, 5);

        // Act & Assert
        equation.ToString().Should().Be("-x = 5");
    }
}
