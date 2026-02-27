using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Neo.Infrastructure.Telemetry;
using System.Diagnostics;

namespace TestNeoSoftware.Telemetry;

/// <summary>
/// Tests for the <see cref="NeoTelemetryService"/> class.
/// </summary>
[TestFixture]
public class NeoTelemetryServiceTests
{
    private Mock<ILogger<NeoTelemetryService>> _loggerMock = null!;
    private NeoTelemetryService _telemetryService = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<NeoTelemetryService>>();
        _telemetryService = new NeoTelemetryService(_loggerMock.Object);
    }

    [Test]
    public void Constructor_ShouldCreateService_WithValidLogger()
    {
        // Act
        var service = new NeoTelemetryService(_loggerMock.Object);
        
        // Assert
        service.Should().NotBeNull();
        service.ActivitySource.Should().NotBeNull();
        service.Meter.Should().NotBeNull();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NeoTelemetryService(null!));
    }

    [Test]
    public void StartSolveActivity_ShouldCreateActivity_WithOperationName()
    {
        // Act
        using var activity = _telemetryService.StartSolveActivity("TestOperation");
        
        // Assert
        activity.Should().NotBeNull();
        activity!.DisplayName.Should().Be("TestOperation");
        activity.Kind.Should().Be(ActivityKind.Internal);
    }

    [Test]
    public void StartSolveActivity_ShouldSetEquationHashTag_WhenHashProvided()
    {
        // Arrange
        const string equationHash = "test-hash-123";
        
        // Act
        using var activity = _telemetryService.StartSolveActivity("TestOperation", equationHash);
        
        // Assert
        activity.Should().NotBeNull();
        activity!.GetTagItem("neo.equation.hash").Should().Be(equationHash);
    }

    [Test]
    public void StartSolveActivity_ShouldNotSetHashTag_WhenHashNotProvided()
    {
        // Act
        using var activity = _telemetryService.StartSolveActivity("TestOperation");
        
        // Assert
        activity.Should().NotBeNull();
        activity!.GetTagItem("neo.equation.hash").Should().BeNull();
    }

    [Test]
    public void RecordEquationSolved_ShouldNotThrow_WithValidParameters()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => 
            _telemetryService.RecordEquationSolved(3, TimeSpan.FromMilliseconds(50), "LU"));
    }

    [Test]
    [TestCase(1, "LU")]
    [TestCase(5, "QR")]
    [TestCase(10, "Cholesky")]
    [TestCase(100, "SVD")]
    public void RecordEquationSolved_ShouldRecord_WithVariousAlgorithms(int variableCount, string algorithm)
    {
        // Act & Assert
        Assert.DoesNotThrow(() => 
            _telemetryService.RecordEquationSolved(variableCount, TimeSpan.FromMilliseconds(50), algorithm));
    }

    [Test]
    public void RecordError_ShouldNotThrow_WithValidParameters()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => 
            _telemetryService.RecordError("TestOperation", "TEST_ERROR", "Test details"));
    }

    [Test]
    public void RecordError_ShouldNotThrow_WithoutDetails()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => 
            _telemetryService.RecordError("TestOperation", "TEST_ERROR"));
    }

    [Test]
    public void RecordCacheHit_ShouldNotThrow_WithoutDuration()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.RecordCacheHit());
    }

    [Test]
    public void RecordCacheHit_ShouldNotThrow_WithDuration()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.RecordCacheHit(TimeSpan.FromMilliseconds(5)));
    }

    [Test]
    public void RecordCacheMiss_ShouldNotThrow_WithoutDuration()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.RecordCacheMiss());
    }

    [Test]
    public void RecordCacheMiss_ShouldNotThrow_WithDuration()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.RecordCacheMiss(TimeSpan.FromMilliseconds(10)));
    }

    [Test]
    public void ActivitySource_ShouldReturnValidSource()
    {
        // Act
        var source = _telemetryService.ActivitySource;
        
        // Assert
        source.Should().NotBeNull();
        source.Name.Should().Be("Neo.EquationSolver");
    }

    [Test]
    public void Meter_ShouldReturnValidMeter()
    {
        // Act
        var meter = _telemetryService.Meter;
        
        // Assert
        meter.Should().NotBeNull();
        meter.Name.Should().Be("Neo.EquationSolver");
    }

    [Test]
    public void Dispose_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            _telemetryService.Dispose();
            _telemetryService.Dispose(); // Second call should not throw
        });
    }

    [TearDown]
    public void TearDown()
    {
        _telemetryService?.Dispose();
    }
}
