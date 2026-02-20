using Neo.Infrastructure.Integration;
using FluentAssertions;

namespace TestNeoSoftware.Infrastructure.Integration;

[TestFixture]
public class PerformanceMonitorTests
{
    private PerformanceMonitor _monitor = null!;

    [SetUp]
    public void SetUp()
    {
        _monitor = new PerformanceMonitor();
    }

    [Test]
    public void RecordOperation_RecordsDuration()
    {
        // Arrange
        var operationName = "TestOperation";
        var duration = TimeSpan.FromMilliseconds(100);

        // Act
        _monitor.RecordOperation(operationName, duration, true);

        // Assert
        var stats = _monitor.GetStats(operationName);
        stats.TotalOperations.Should().Be(1);
        stats.SuccessCount.Should().Be(1);
        stats.SuccessRate.Should().Be(1.0);
    }

    [Test]
    public void RecordOperation_RecordsFailure()
    {
        // Arrange
        var operationName = "FailedOperation";

        // Act
        _monitor.RecordOperation(operationName, TimeSpan.FromMilliseconds(50), false);

        // Assert
        var stats = _monitor.GetStats(operationName);
        stats.TotalOperations.Should().Be(1);
        stats.SuccessCount.Should().Be(0);
        stats.SuccessRate.Should().Be(0.0);
    }

    [Test]
    public void MultipleOperations_AggregatesStats()
    {
        // Arrange
        var operationName = "MultiOperation";

        // Act
        _monitor.RecordOperation(operationName, TimeSpan.FromMilliseconds(100), true);
        _monitor.RecordOperation(operationName, TimeSpan.FromMilliseconds(200), true);
        _monitor.RecordOperation(operationName, TimeSpan.FromMilliseconds(50), false);

        // Assert
        var stats = _monitor.GetStats(operationName);
        stats.TotalOperations.Should().Be(3);
        stats.SuccessCount.Should().Be(2);
        stats.SuccessRate.Should().BeApproximately(2.0 / 3.0, 0.01);
        stats.AverageDuration.Should().BeCloseTo(TimeSpan.FromMilliseconds(116.67), TimeSpan.FromMilliseconds(1));
        stats.MinDuration.Should().Be(TimeSpan.FromMilliseconds(50));
        stats.MaxDuration.Should().Be(TimeSpan.FromMilliseconds(200));
    }

    [Test]
    public void GetStats_UnknownOperation_ReturnsEmptyStats()
    {
        // Act
        var stats = _monitor.GetStats("UnknownOperation");

        // Assert
        stats.TotalOperations.Should().Be(0);
        stats.SuccessCount.Should().Be(0);
        stats.SuccessRate.Should().Be(0);
        stats.AverageDuration.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void StartActivity_RecordsDuration()
    {
        // Arrange
        var operationName = "TimedActivity";

        // Act
        using (var activity = _monitor.StartActivity(operationName))
        {
            Thread.Sleep(50); // Simulate work
        }

        // Assert
        var stats = _monitor.GetStats(operationName);
        stats.TotalOperations.Should().Be(1);
        stats.SuccessCount.Should().Be(1);
        stats.AverageDuration.Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(40));
    }

    [Test]
    public void StartActivity_SetSuccessFalse_RecordsFailure()
    {
        // Arrange
        var operationName = "FailedActivity";

        // Act
        using (var activity = _monitor.StartActivity(operationName))
        {
            activity.SetSuccess(false);
        }

        // Assert
        var stats = _monitor.GetStats(operationName);
        stats.TotalOperations.Should().Be(1);
        stats.SuccessCount.Should().Be(0);
        stats.SuccessRate.Should().Be(0.0);
    }

    [Test]
    public void StartActivity_SetSuccessTrue_RecordsSuccess()
    {
        // Arrange
        var operationName = "SuccessfulActivity";

        // Act
        using (var activity = _monitor.StartActivity(operationName))
        {
            activity.SetSuccess(true);
        }

        // Assert
        var stats = _monitor.GetStats(operationName);
        stats.TotalOperations.Should().Be(1);
        stats.SuccessCount.Should().Be(1);
        stats.SuccessRate.Should().Be(1.0);
    }

    [Test]
    public void MultipleActivities_AggregatesCorrectly()
    {
        // Arrange
        var operationName = "MultipleActivities";

        // Act
        using (var activity1 = _monitor.StartActivity(operationName))
        {
            Thread.Sleep(10);
        }

        using (var activity2 = _monitor.StartActivity(operationName))
        {
            Thread.Sleep(20);
            activity2.SetSuccess(false);
        }

        // Assert
        var stats = _monitor.GetStats(operationName);
        stats.TotalOperations.Should().Be(2);
        stats.SuccessCount.Should().Be(1);
        stats.MinDuration.Should().BeLessThan(stats.MaxDuration);
    }

    [Test]
    public void Activity_DisposeTwice_DoesNotDoubleCount()
    {
        // Arrange
        var operationName = "DoubleDispose";

        // Act
        var activity = _monitor.StartActivity(operationName);
        activity.Dispose();
        activity.Dispose(); // Second dispose should be no-op

        // Assert
        var stats = _monitor.GetStats(operationName);
        stats.TotalOperations.Should().Be(1);
    }
}

[TestFixture]
public class PerformanceStatsTests
{
    [Test]
    public void DefaultConstructor_SetsDefaults()
    {
        // Act
        var stats = new PerformanceStats();

        // Assert
        stats.TotalOperations.Should().Be(0);
        stats.SuccessCount.Should().Be(0);
        stats.SuccessRate.Should().Be(0);
        stats.AverageDuration.Should().Be(TimeSpan.Zero);
        stats.MinDuration.Should().Be(TimeSpan.Zero);
        stats.MaxDuration.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void Record_WithValues_SetsProperties()
    {
        // Arrange & Act
        var stats = new PerformanceStats
        {
            TotalOperations = 10,
            SuccessCount = 8,
            SuccessRate = 0.8,
            AverageDuration = TimeSpan.FromMilliseconds(100),
            MinDuration = TimeSpan.FromMilliseconds(50),
            MaxDuration = TimeSpan.FromMilliseconds(200)
        };

        // Assert
        stats.TotalOperations.Should().Be(10);
        stats.SuccessCount.Should().Be(8);
        stats.SuccessRate.Should().Be(0.8);
        stats.AverageDuration.Should().Be(TimeSpan.FromMilliseconds(100));
        stats.MinDuration.Should().Be(TimeSpan.FromMilliseconds(50));
        stats.MaxDuration.Should().Be(TimeSpan.FromMilliseconds(200));
    }

    [Test]
    public void Equality_SameValues_ReturnsTrue()
    {
        // Arrange
        var stats1 = new PerformanceStats { TotalOperations = 5, SuccessCount = 4 };
        var stats2 = new PerformanceStats { TotalOperations = 5, SuccessCount = 4 };

        // Act & Assert
        stats1.Should().BeEquivalentTo(stats2, options => options.ExcludingMissingMembers());
    }

    [Test]
    public void Equality_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var stats1 = new PerformanceStats { TotalOperations = 5 };
        var stats2 = new PerformanceStats { TotalOperations = 10 };

        // Act & Assert
        stats1.Should().NotBeEquivalentTo(stats2);
    }
}
