using Neo.Domain.Equation.Variables;

namespace TestNeoSoftware.Domain.Equation.Variables;

[TestFixture]
public class CoefficientTests
{
    [Test]
    public void Constructor_ValidArguments_Creates()
    {
        // Arrange
        var variable = Variable.Create("x");

        // Act
        var coefficient = new Coefficient(2.5, variable);

        // Assert
        coefficient.Value.Should().Be(2.5);
        coefficient.Variable.Should().Be(variable);
    }

    [Test]
    public void Constructor_NullVariable_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new Coefficient(2.5, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("variable");
    }

    [Test]
    public void Negate_ReturnsNegatedValue()
    {
        // Arrange
        var variable = Variable.Create("x");
        var coefficient = new Coefficient(3.0, variable);

        // Act
        var negated = coefficient.Negate();

        // Assert
        negated.Value.Should().Be(-3.0);
        negated.Variable.Should().Be(variable);
    }

    [Test]
    public void Negate_NegativeValue_ReturnsPositive()
    {
        // Arrange
        var variable = Variable.Create("x");
        var coefficient = new Coefficient(-5.0, variable);

        // Act
        var negated = coefficient.Negate();

        // Assert
        negated.Value.Should().Be(5.0);
    }

    [Test]
    public void UnitCoefficient_CreatesWithOne()
    {
        // Arrange
        var variable = Variable.Create("x");

        // Act
        var coefficient = Coefficient.UnitCoefficient(variable);

        // Assert
        coefficient.Value.Should().Be(1);
        coefficient.Variable.Should().Be(variable);
    }

    [Test]
    public void ToString_ValueIsOne_ReturnsVariableName()
    {
        // Arrange
        var variable = Variable.Create("x");
        var coefficient = new Coefficient(1, variable);

        // Act & Assert
        coefficient.ToString().Should().Be("x");
    }

    [Test]
    public void ToString_ValueIsNegativeOne_ReturnsMinusVariableName()
    {
        // Arrange
        var variable = Variable.Create("x");
        var coefficient = new Coefficient(-1, variable);

        // Act & Assert
        coefficient.ToString().Should().Be("-x");
    }

    [Test]
    public void ToString_ValueIsZero_ReturnsZero()
    {
        // Arrange
        var variable = Variable.Create("x");
        var coefficient = new Coefficient(0, variable);

        // Act & Assert
        coefficient.ToString().Should().Be("0");
    }

    [Test]
    public void ToString_ValueIsOther_ReturnsValueVariable()
    {
        // Arrange
        var variable = Variable.Create("x");
        var coefficient = new Coefficient(2.5, variable);

        // Act & Assert
        coefficient.ToString().Should().Be("2.5x");
    }

    [Test]
    public void Equals_SameValueAndVariable_ReturnsTrue()
    {
        // Arrange
        var variable = Variable.Create("x");
        var c1 = new Coefficient(2.5, variable);
        var c2 = new Coefficient(2.5, variable);

        // Act & Assert
        c1.Should().BeEquivalentTo(c2, options => options.ExcludingMissingMembers());
    }

    [Test]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var variable = Variable.Create("x");
        var c1 = new Coefficient(2.5, variable);
        var c2 = new Coefficient(3.0, variable);

        // Act & Assert
        c1.Should().NotBeEquivalentTo(c2);
    }
}