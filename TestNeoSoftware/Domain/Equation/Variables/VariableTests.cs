using Neo.Domain.Equation.Variables;

namespace TestNeoSoftware.Domain.Equation.Variables;

[TestFixture]
public class VariableTests
{
    [Test]
    public void Create_ValidName_ReturnsVariable()
    {
        // Act
        var variable = Variable.Create("x");

        // Assert
        variable.Should().NotBeNull();
        variable.Name.Should().Be("x");
    }

    [Test]
    public void Create_ValidName_IsLowerCase()
    {
        // Act
        var variable = Variable.Create("X");

        // Assert
        variable.Name.Should().Be("x");
    }

    [Test]
    public void Create_EmptyName_ThrowsArgumentException()
    {
        // Act
        Action act = () => Variable.Create("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*empty*");
    }

    [Test]
    public void Create_WhitespaceName_ThrowsArgumentException()
    {
        // Act
        Action act = () => Variable.Create(" ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_MultiLetterName_ThrowsArgumentException()
    {
        // Act
        Action act = () => Variable.Create("xy");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*single character*");
    }

    [Test]
    public void Create_NonLetterName_ThrowsArgumentException()
    {
        // Act
        Action act = () => Variable.Create("1");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be a letter*");
    }

    [Test]
    public void Equals_SameNameDifferentCase_ReturnsTrue()
    {
        // Arrange
        var x1 = Variable.Create("X");
        var x2 = Variable.Create("x");

        // Act & Assert
        x1.Should().BeEquivalentTo(x2, options => options.ExcludingMissingMembers());
        x1.GetHashCode().Should().Be(x2.GetHashCode());
    }

    [Test]
    public void Equals_DifferentName_ReturnsFalse()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");

        // Act & Assert
        x.Equals(y).Should().BeFalse();
    }

    [Test]
    public void CompareTo_AlphabeticalOrder_ReturnsCorrect()
    {
        // Arrange
        var x = Variable.Create("x");
        var y = Variable.Create("y");

        // Act & Assert
        x.CompareTo(y).Should().BeNegative();
        y.CompareTo(x).Should().BePositive();
        x.CompareTo(x).Should().Be(0);
    }

    [Test]
    public void ToString_ReturnsName()
    {
        // Arrange
        var variable = Variable.Create("x");

        // Act & Assert
        variable.ToString().Should().Be("x");
    }

    [Test]
    public void MultiplyOperator_CreatesCoefficient()
    {
        // Arrange
        var x = Variable.Create("x");

        // Act
        var coefficient = 2.5 * x;

        // Assert
        coefficient.Value.Should().Be(2.5);
        coefficient.Variable.Should().Be(x);
    }

    [Test]
    public void SetIndex_ValidIndex_SetsIndex()
    {
        // Arrange
        var variable = Variable.Create("x");

        // Act
        variable.SetIndex(5);

        // Assert
        variable.Index.Should().Be(5);
    }

    [Test]
    public void SetIndex_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var variable = Variable.Create("x");

        // Act
        Action act = () => variable.SetIndex(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}