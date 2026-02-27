using FluentAssertions;
using MathNet.Numerics.LinearAlgebra;
using Neo.Infrastructure.Telemetry;
using System.Diagnostics;

namespace TestNeoSoftware.Telemetry;

/// <summary>
/// Tests for the <see cref="TelemetryActivityExtensions"/> class.
/// </summary>
[TestFixture]
public class TelemetryActivityExtensionsTests
{
    private Activity _activity = null!;
    private ActivitySource _activitySource = null!;

    [SetUp]
    public void SetUp()
    {
        _activitySource = new ActivitySource("TestSource", "1.0.0");
        _activity = _activitySource.StartActivity("TestActivity")!;
    }

    [TearDown]
    public void TearDown()
    {
        _activity?.Dispose();
        _activitySource?.Dispose();
    }

    [Test]
    public void SetMatrixTags_ShouldSetTags_WithValidMatrix()
    {
        // Arrange
        var matrix = Matrix<double>.Build.Dense(3, 3, (i, j) => i * 3 + j);
        
        // Act
        _activity.SetMatrixTags(matrix, "test");
        
        // Assert
        _activity.GetTagItem("test.rows").Should().Be(3);
        _activity.GetTagItem("test.columns").Should().Be(3);
        _activity.GetTagItem("test.isSquare").Should().BeTrue();
    }

    [Test]
    public void SetMatrixTags_ShouldSetNonSquareTags_WithRectangularMatrix()
    {
        // Arrange
        var matrix = Matrix<double>.Build.Dense(3, 4, (i, j) => i * 4 + j);
        
        // Act
        _activity.SetMatrixTags(matrix);
        
        // Assert
        _activity.GetTagItem("matrix.rows").Should().Be(3);
        _activity.GetTagItem("matrix.columns").Should().Be(4);
        _activity.GetTagItem("matrix.isSquare").Should().BeFalse();
    }

    [Test]
    public void SetMatrixTags_ShouldNotThrow_WhenActivityIsNull()
    {
        // Arrange
        var matrix = Matrix<double>.Build.Dense(2, 2);
        Activity? nullActivity = null;
        
        // Act & Assert
        Assert.DoesNotThrow(() => nullActivity!.SetMatrixTags(matrix));
    }

    [Test]
    public void SetSolutionTags_ShouldSetTags_WithSuccess()
    {
        // Act
        _activity.SetSolutionTags("LU", true, TimeSpan.FromMilliseconds(50));
        
        // Assert
        _activity.GetTagItem("neo.algorithm").Should().Be("LU");
        _activity.GetTagItem("neo.success").Should().BeTrue();
        _activity.GetTagItem("neo.duration.ms").Should().Be(50.0);
        _activity.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Test]
    public void SetSolutionTags_ShouldSetTags_WithFailure()
    {
        // Act
        _activity.SetSolutionTags("QR", false, TimeSpan.FromMilliseconds(100));
        
        // Assert
        _activity.GetTagItem("neo.algorithm").Should().Be("QR");
        _activity.GetTagItem("neo.success").Should().BeFalse();
        _activity.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Test]
    public void SetSolutionTags_ShouldNotThrow_WhenActivityIsNull()
    {
        // Arrange
        Activity? nullActivity = null;
        
        // Act & Assert
        Assert.DoesNotThrow(() => nullActivity!.SetSolutionTags("LU", true, TimeSpan.Zero));
    }

    [Test]
    public void SetExceptionTags_ShouldSetTags_WithException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        
        // Act
        _activity.SetExceptionTags(exception);
        
        // Assert
        _activity.GetTagItem("neo.exception.type").Should().Be("System.InvalidOperationException");
        _activity.GetTagItem("neo.exception.message").Should().Be("Test error");
        _activity.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Test]
    public void SetExceptionTags_ShouldNotThrow_WhenActivityIsNull()
    {
        // Arrange
        Activity? nullActivity = null;
        var exception = new Exception("Test");
        
        // Act & Assert
        Assert.DoesNotThrow(() => nullActivity!.SetExceptionTags(exception));
    }

    [Test]
    public void SetCacheTags_ShouldSetTags_WithCacheHit()
    {
        // Act
        _activity.SetCacheTags(true, TimeSpan.FromMilliseconds(5));
        
        // Assert
        _activity.GetTagItem("neo.cache.hit").Should().BeTrue();
        _activity.GetTagItem("neo.cache.lookup.ms").Should().Be(5.0);
    }

    [Test]
    public void SetCacheTags_ShouldSetTags_WithCacheMiss()
    {
        // Act
        _activity.SetCacheTags(false);
        
        // Assert
        _activity.GetTagItem("neo.cache.hit").Should().BeFalse();
        _activity.GetTagItem("neo.cache.lookup.ms").Should().BeNull();
    }

    [Test]
    public void SetCacheTags_ShouldNotThrow_WhenActivityIsNull()
    {
        // Arrange
        Activity? nullActivity = null;
        
        // Act & Assert
        Assert.DoesNotThrow(() => nullActivity!.SetCacheTags(true));
    }

    [Test]
    public void SetEquationSystemTags_ShouldSetTags_WithValidCounts()
    {
        // Act
        _activity.SetEquationSystemTags(3, 3);
        
        // Assert
        _activity.GetTagItem("neo.equation.count").Should().Be(3);
        _activity.GetTagItem("neo.variable.count").Should().Be(3);
        _activity.GetTagItem("neo.isSquare").Should().BeTrue();
    }

    [Test]
    public void SetEquationSystemTags_ShouldSetTags_WithNonSquareSystem()
    {
        // Act
        _activity.SetEquationSystemTags(3, 4);
        
        // Assert
        _activity.GetTagItem("neo.equation.count").Should().Be(3);
        _activity.GetTagItem("neo.variable.count").Should().Be(4);
        _activity.GetTagItem("neo.isSquare").Should().BeFalse();
    }

    [Test]
    public void SetEquationSystemTags_ShouldNotThrow_WhenActivityIsNull()
    {
        // Arrange
        Activity? nullActivity = null;
        
        // Act & Assert
        Assert.DoesNotThrow(() => nullActivity!.SetEquationSystemTags(2, 2));
    }
}
