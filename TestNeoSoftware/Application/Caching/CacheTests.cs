using Neo.Application.Caching;
using Neo.Domain.Equation;
using Neo.Domain.Equation.Variables;
using Neo.Domain.Solution;

namespace TestNeoSoftware.Application.Caching;

[TestFixture]
public class MemoryEquationCacheTests
{
    private MemoryEquationCache _cache = null!;
    private EquationSystem _system = null!;
    private Solution _solution = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = new MemoryEquationCache();

        var x = Variable.Create("x");
        var equations = new[]
        {
            new LinearEquation(new[] { new Coefficient(1, x) }, 5)
        };
        _system = new EquationSystem(equations);
        _solution = Solution.Success(_system, new Dictionary<Variable, double> { { x, 5 } });
    }

    [TearDown]
    public void TearDown()
    {
    }

    [Test]
    public void Set_GetSystem_ReturnsStored()
    {
        // Arrange
        var key = "test-key";

        // Act
        _cache.SetSystem(key, _system);
        var result = _cache.GetSystem(key);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(_system);
    }

    [Test]
    public void GetSystem_NotFound_ReturnsNull()
    {
        // Act
        var result = _cache.GetSystem("non-existent-key");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Test]
    public async Task GetSystem_Expired_ReturnsNull()
    {
        // Arrange
        var key = "expiring-key";
        var ttl = TimeSpan.FromMilliseconds(10);
        _cache.SetSystem(key, _system, ttl);

        // Act - wait for expiration
        await Task.Delay(50);
        var result = _cache.GetSystem(key);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Test]
    public void SetSystem_WithTtl_StoresWithExpiration()
    {
        // Arrange
        var key = "ttl-key";
        var ttl = TimeSpan.FromMinutes(5);

        // Act
        _cache.SetSystem(key, _system, ttl);
        var result = _cache.GetSystem(key);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(_system);
    }

    [Test]
    public void Set_GetSolution_ReturnsStored()
    {
        // Arrange
        var key = "solution-key";

        // Act
        _cache.SetSolution(key, _solution);
        var result = _cache.GetSolution(key);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(_solution);
    }

    [Test]
    public void GetSolution_NotFound_ReturnsNull()
    {
        // Act
        var result = _cache.GetSolution("non-existent-key");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Test]
    public async Task GetSolution_Expired_ReturnsNull()
    {
        // Arrange
        var key = "expiring-solution";
        var ttl = TimeSpan.FromMilliseconds(10);
        _cache.SetSolution(key, _solution, ttl);

        // Act - wait for expiration
        await Task.Delay(50);
        var result = _cache.GetSolution(key);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Test]
    public void Clear_RemovesAll()
    {
        // Arrange
        _cache.SetSystem("system-key", _system);
        _cache.SetSolution("solution-key", _solution);

        // Act
        _cache.Clear();

        // Assert
        _cache.GetSystem("system-key").Value.Should().BeNull();
        _cache.GetSolution("solution-key").Value.Should().BeNull();
    }

    [Test]
    public void SetSystem_OverwritesExisting()
    {
        // Arrange
        var key = "overwrite-key";
        var x = Variable.Create("x");
        var newSystem = new EquationSystem(new[]
        {
            new LinearEquation(new[] { new Coefficient(2, x) }, 10)
        });
        _cache.SetSystem(key, _system);

        // Act
        _cache.SetSystem(key, newSystem);
        var result = _cache.GetSystem(key);

        // Assert
        result.Value.Should().Be(newSystem);
    }
}

[TestFixture]
public class NullEquationCacheTests
{
    private NullEquationCache _cache = null!;
    private EquationSystem _system = null!;
    private Solution _solution = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = new NullEquationCache();

        var x = Variable.Create("x");
        var equations = new[]
        {
            new LinearEquation(new[] { new Coefficient(1, x) }, 5)
        };
        _system = new EquationSystem(equations);
        _solution = Solution.Success(_system, new Dictionary<Variable, double> { { x, 5 } });
    }

    [Test]
    public void GetSystem_Always_ReturnsNull()
    {
        // Act
        var result = _cache.GetSystem("any-key");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Test]
    public void SetSystem_DoesNothing()
    {
        // Act
        _cache.SetSystem("key", _system);
        var result = _cache.GetSystem("key");

        // Assert
        result.Value.Should().BeNull();
    }

    [Test]
    public void GetSolution_Always_ReturnsNull()
    {
        // Act
        var result = _cache.GetSolution("any-key");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Test]
    public void SetSolution_DoesNothing()
    {
        // Act
        _cache.SetSolution("key", _solution);
        var result = _cache.GetSolution("key");

        // Assert
        result.Value.Should().BeNull();
    }

    [Test]
    public void Clear_DoesNothing()
    {
        // Act - should not throw
        _cache.Clear();

        // Assert - still returns null
        _cache.GetSystem("key").Value.Should().BeNull();
    }
}