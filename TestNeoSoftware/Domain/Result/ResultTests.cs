using Neo.Domain.Result;
using FluentAssertions;
using ResultNS = Neo.Domain.Result;

namespace TestNeoSoftware.Domain.Result;

[TestFixture]
public class ResultTests
{
    [Test]
    public void Success_Result_HasValue()
    {
        // Act
        var result = ResultNS.Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Test]
    public void Failure_Result_HasError()
    {
        // Arrange
        var error = new Error("Test error", "TEST_ERROR");

        // Act
        var result = ResultNS.Result<int>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(error);
        result.Value.Should().Be(0); // Default value for int
    }

    [Test]
    public void Failure_WithMessageAndCode_CreatesError()
    {
        // Act
        var result = Result<int>.Failure("Something went wrong", "CUSTOM_ERROR");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Something went wrong");
        result.Error.Code.Should().Be("CUSTOM_ERROR");
    }

    [Test]
    public void GetValueOrThrow_Success_ReturnsValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var value = result.GetValueOrThrow();

        // Assert
        value.Should().Be(42);
    }

    [Test]
    public void GetValueOrThrow_Failure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Failure(new Error("Error", "ERROR"));

        // Act
        Action act = () => result.GetValueOrThrow();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Error");
    }

    [Test]
    public void Match_Success_CallsOnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var onSuccessCalled = false;
        var onFailureCalled = false;

        // Act
        var matchResult = result.Match(
            v => { onSuccessCalled = true; return v * 2; },
            _ => { onFailureCalled = true; return 0; }
        );

        // Assert
        matchResult.Should().Be(84);
        onSuccessCalled.Should().BeTrue();
        onFailureCalled.Should().BeFalse();
    }

    [Test]
    public void Match_Failure_CallsOnFailure()
    {
        // Arrange
        var result = Result<int>.Failure(new Error("Error", "ERROR"));
        var onSuccessCalled = false;
        var onFailureCalled = false;

        // Act
        var matchResult = result.Match(
            v => { onSuccessCalled = true; return v * 2; },
            _ => { onFailureCalled = true; return -1; }
        );

        // Assert
        matchResult.Should().Be(-1);
        onSuccessCalled.Should().BeFalse();
        onFailureCalled.Should().BeTrue();
    }

    [Test]
    public void Map_Success_TransformsValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var mapped = result.Map(v => v.ToString());

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("42");
    }

    [Test]
    public void Map_Failure_PreservesFailure()
    {
        // Arrange
        var error = new Error("Error", "ERROR");
        var result = Result<int>.Failure(error);

        // Act
        var mapped = result.Map(v => v.ToString());

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
    }

    [Test]
    public void Bind_Success_ReturnsBoundResult()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var bound = result.Bind(v => Result<string>.Success(v.ToString()));

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("42");
    }

    [Test]
    public void Bind_Failure_PreservesFailure()
    {
        // Arrange
        var error = new Error("Error", "ERROR");
        var result = Result<int>.Failure(error);

        // Act
        var bound = result.Bind(v => Result<string>.Success(v.ToString()));

        // Assert
        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be(error);
    }

    [Test]
    public void NonGeneric_Success_HasNoError()
    {
        // Act
        var result = ResultNS.Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Test]
    public void NonGeneric_Failure_HasError()
    {
        // Arrange
        var error = new Error("Error", "ERROR");

        // Act
        var result = ResultNS.Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeSameAs(error);
    }

    [Test]
    public void NonGeneric_Failure_WithMessage_CreatesError()
    {
        // Act
        var result = ResultNS.Result.Failure("Something went wrong", "CUSTOM_ERROR");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Something went wrong");
        result.Error.Code.Should().Be("CUSTOM_ERROR");
    }
}

[TestFixture]
public class ErrorTests
{
    [Test]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var exception = new InvalidOperationException("Test");

        // Act
        var error = new Error("Message", "CODE", exception);

        // Assert
        error.Message.Should().Be("Message");
        error.Code.Should().Be("CODE");
        error.Exception.Should().Be(exception);
        error.Timestamp.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Test]
    public void Constructor_WithoutException_SetsNull()
    {
        // Act
        var error = new Error("Message", "CODE");

        // Assert
        error.Exception.Should().BeNull();
    }

    [Test]
    public void ToString_ReturnsCodeAndMessage()
    {
        // Arrange
        var error = new Error("Message", "CODE");

        // Act & Assert
        error.ToString().Should().Be("[CODE] Message");
    }

    [Test]
    public void StaticErrors_HaveCorrectCodes()
    {
        // Assert
        Error.NullInput.Code.Should().Be("NULL_INPUT");
        Error.EmptyInput.Code.Should().Be("EMPTY_INPUT");
        Error.InvalidFormat.Code.Should().Be("INVALID_FORMAT");
        Error.Underdetermined.Code.Should().Be("UNDERDETERMINED");
        Error.Overdetermined.Code.Should().Be("OVERDETERMINED");
        Error.SingularMatrix.Code.Should().Be("SINGULAR_MATRIX");
    }

    [Test]
    public void Context_IsEmptyByDefault()
    {
        // Arrange
        var error = new Error("Message", "CODE");

        // Assert
        error.Context.Should().BeEmpty();
    }
}
