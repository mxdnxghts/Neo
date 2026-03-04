# Neo Benchmark Plan

## Overview

This document outlines a comprehensive benchmarking strategy for the Neo equation solver system. The benchmarks are designed to measure performance across all critical operations, identify bottlenecks, and track improvements over time.

---

## Benchmark Categories

### 1. Parsing Benchmarks

Measure the performance of equation string parsing.

| Benchmark | Description | Input Size | Metrics |
|-----------|-------------|------------|---------|
| `Parse_Simple2x2` | Parse 2 equations with 2 variables | `"2x + 3y = 5; x - y = 1"` | Time, Allocations |
| `Parse_Medium3x3` | Parse 3 equations with 3 variables | `"x + y + z = 6; 2x - y + z = 3; x + 2y - z = 2"` | Time, Allocations |
| `Parse_Large5x5` | Parse 5 equations with 5 variables | 5 equations, 5 variables each | Time, Allocations |
| `Parse_ComplexCoefficients` | Parse with decimal coefficients | `"1.5x + 2.75y = 3.125; ..."` | Time, Allocations |
| `Parse_NegativeCoefficients` | Parse with negative values | `"-2x - 3y = -5; ..."` | Time, Allocations |
| `Parse_SparseSystem` | Parse with zero coefficients | `"2x + 0y + 3z = 5; ..."` | Time, Allocations |
| `Parse_InvalidInput` | Parse malformed input | `"invalid equation"` | Time, Error Rate |

**Goal:** Identify parsing bottlenecks and string manipulation overhead.

---

### 2. Solving Benchmarks

Measure the performance of the complete solving pipeline.

| Benchmark | Description | Matrix Size | Algorithm | Metrics |
|-----------|-------------|-------------|-----------|---------|
| `Solve_2x2_LU` | Solve 2x2 system | 2×2 | LU | Time, Allocations |
| `Solve_3x3_LU` | Solve 3x3 system | 3×3 | LU | Time, Allocations |
| `Solve_5x5_LU` | Solve 5x5 system | 5×5 | LU | Time, Allocations |
| `Solve_10x10_LU` | Solve 10x10 system | 10×10 | LU | Time, Allocations |
| `Solve_2x2_QR` | Solve 2x2 system | 2×2 | QR | Time, Allocations |
| `Solve_3x3_Cholesky` | Solve SPD 3x3 system | 3×3 | Cholesky | Time, Allocations |
| `Solve_3x3_SVD` | Solve 3x3 system | 3×3 | SVD | Time, Allocations |

**Goal:** Compare algorithm performance and identify optimal use cases.

---

### 3. Algorithm Comparison Benchmarks

Compare different solving algorithms on the same input.

| Benchmark | Description | Variables | Metrics |
|-----------|-------------|-----------|---------|
| `Algorithms_2x2` | Compare LU, QR, Cholesky, SVD | 2 | Time per Algorithm |
| `Algorithms_5x5` | Compare all algorithms | 5 | Time per Algorithm |
| `Algorithms_10x10` | Compare all algorithms | 10 | Time per Algorithm |
| `Algorithms_NonSquare` | Compare QR, SVD on non-square | 5×3 | Time per Algorithm |
| `Algorithms_SPD` | Compare on symmetric positive-definite | 5 | Time per Algorithm |

**Goal:** Validate algorithm selection logic and performance characteristics.

---

### 4. Caching Benchmarks

Measure caching performance and effectiveness.

| Benchmark | Description | Cache State | Metrics |
|-----------|-------------|-------------|---------|
| `Solve_WithCache_Hit` | Solve with cached result | Warm cache | Time, Hit Rate |
| `Solve_WithCache_Miss` | Solve with cache miss | Cold cache | Time, Miss Rate |
| `Solve_NoCache` | Solve with caching disabled | N/A | Time, Baseline |
| `Cache_SetAndGet` | Measure cache operations | N/A | Time per Operation |
| `Cache_HashComputation` | Measure SHA-256 hash generation | Various input sizes | Time, Allocations |
| `Cache_TtlExpiration` | Measure TTL expiration handling | N/A | Time, Accuracy |

**Goal:** Quantify caching overhead and benefits.

---

### 5. Batch Processing Benchmarks

Measure parallel batch solving performance.

| Benchmark | Description | Batch Size | Parallelism | Metrics |
|-----------|-------------|------------|-------------|---------|
| `Batch_Small` | Batch of 10 equations | 10 | Default | Time, Throughput |
| `Batch_Medium` | Batch of 100 equations | 100 | Default | Time, Throughput |
| `Batch_Large` | Batch of 1000 equations | 1000 | Default | Time, Throughput |
| `Batch_Parallel_1` | Batch with 1 thread | 100 | 1 | Time, CPU Usage |
| `Batch_Parallel_4` | Batch with 4 threads | 100 | 4 | Time, CPU Usage |
| `Batch_Parallel_8` | Batch with 8 threads | 100 | 8 | Time, CPU Usage |
| `Batch_Parallel_Max` | Batch with max parallelism | 100 | -1 | Time, CPU Usage |

**Goal:** Optimize parallel processing configuration.

---

### 6. Streaming Benchmarks

Measure `IAsyncEnumerable` streaming performance.

| Benchmark | Description | Stream Size | Metrics |
|-----------|-------------|-------------|---------|
| `Stream_Small` | Stream 10 equations | 10 | Time, Memory |
| `Stream_Medium` | Stream 100 equations | 100 | Time, Memory |
| `Stream_Large` | Stream 1000 equations | 1000 | Time, Memory |
| `Stream_VsBatch` | Compare streaming vs batch | 100 | Time, Memory |

**Goal:** Evaluate streaming efficiency for large datasets.

---

### 7. Matrix Conversion Benchmarks

Measure equation-to-matrix conversion performance.

| Benchmark | Description | Size | Metrics |
|-----------|-------------|------|---------|
| `ToMatrix_2x2` | Convert 2x2 system | 2×2 | Time, Allocations |
| `ToMatrix_5x5` | Convert 5x5 system | 5×5 | Time, Allocations |
| `ToMatrix_10x10` | Convert 10x10 system | 10×10 | Time, Allocations |
| `Normalize_Equations` | Normalize equation system | Various | Time, Allocations |

**Goal:** Optimize matrix conversion overhead.

---

### 8. Validation Benchmarks

Measure solution validation performance.

| Benchmark | Description | Metrics |
|-----------|-------------|---------|
| `Validate_Success` | Validate correct solution | Time |
| `Validate_NoSolution` | Validate no-solution case | Time |
| `Validate_Infinite` | Validate infinite solutions | Time |
| `Validate_Tolerance` | Validate with different tolerances | Time |

**Goal:** Ensure validation doesn't bottleneck solving.

---

### 9. Memory Allocation Benchmarks

Measure memory usage patterns.

| Benchmark | Description | Metrics |
|-----------|-------------|---------|
| `Memory_SingleSolve` | Memory per single solve | Gen 0/1/2, LOH |
| `Memory_BatchSolve` | Memory per batch solve | Gen 0/1/2, LOH |
| `Memory_Streaming` | Memory during streaming | Gen 0/1/2, LOH |
| `Memory_Caching` | Memory with caching enabled | Gen 0/1/2, LOH |

**Goal:** Identify memory pressure and GC impact.

---

### 10. End-to-End Benchmarks

Measure complete pipeline performance.

| Benchmark | Description | Scenario | Metrics |
|-----------|-------------|----------|---------|
| `E2E_Simple` | Full pipeline | 2x2 simple system | Total Time |
| `E2E_Complex` | Full pipeline | 5x5 complex system | Total Time |
| `E2E_Batch` | Full pipeline | 100 equations | Total Time |
| `E2E_WithCache` | Full pipeline with cache | Repeated solves | Total Time |
| `E2E_NoCache` | Full pipeline no cache | Fresh solves | Total Time |

**Goal:** Measure real-world performance.

---

## Benchmark Configuration

### Environment Settings

```csharp
[Config(typeof(Config))]
public class NeoBenchmarks
{
    private class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.Default
                .WithRuntime(CoreRuntime.Core10)
                .WithWarmupCount(3)
                .WithIterationCount(10)
                .WithInvocationCount(100)
                .WithUnrollFactor(10));
            
            AddDiagnoser(MemoryDiagner.Default);
            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(BaselineRatioColumn.RatioMean);
        }
    }
}
```

### Baseline Configuration

```csharp
[Benchmark(Baseline = true)]
public Result<Solution> Solve_WithCache() { }

[Benchmark]
public Result<Solution> Solve_NoCache() { }
```

---

## Implementation Structure

### File Organization

```
NeoBenchmark/
├── Program.cs                    # Entry point
├── BenchmarkConfig.cs            # Shared configuration
├── Parsing/
│   ├── ParsingBenchmarks.cs      # Parsing benchmarks
│   └── ParsingData.cs            # Test data generators
├── Solving/
│   ├── SolvingBenchmarks.cs      # Core solving benchmarks
│   ├── AlgorithmComparison.cs    # Algorithm benchmarks
│   └── SolvingData.cs            # Test matrices
├── Caching/
│   ├── CachingBenchmarks.cs      # Cache benchmarks
│   └── CacheScenarios.cs         # Cache test scenarios
├── Batch/
│   ├── BatchBenchmarks.cs        # Batch processing benchmarks
│   └── BatchData.cs              # Batch test data
├── Memory/
│   ├── MemoryBenchmarks.cs       # Memory allocation benchmarks
│   └── GcPressure.cs             # GC pressure tests
└── EndToEnd/
    ├── E2EBenchmarks.cs          # End-to-end benchmarks
    └── Scenarios.cs              # E2E scenarios
```

---

## Sample Benchmark Implementation

### Parsing Benchmarks

```csharp
using BenchmarkDotNet.Attributes;
using Neo.Application.Solver.Equation;
using Neo.Infrastructure.Parsing;

namespace NeoBenchmark.Parsing;

[MemoryDiagnoser]
public class ParsingBenchmarks
{
    private EquationParser _parser;
    
    // Simple cases
    private string _simple2x2 = "2x + 3y = 5; x - y = 1";
    private string _simple3x3 = "x + y + z = 6; 2x - y + z = 3; x + 2y - z = 2";
    
    // Complex cases
    private string _decimals = "1.5x + 2.75y = 3.125; 0.5x - 1.25y = 0.875";
    private string _negatives = "-2x - 3y = -5; -x + y = -1";
    private string _sparse = "2x + 0y + 3z = 5; 0x + 4y - z = 2; x + 0y + 0z = 1";
    
    // Large cases
    private string _large5x5;
    
    [GlobalSetup]
    public void Setup()
    {
        _parser = new EquationParser();
        _large5x5 = GenerateLargeEquationString(5);
    }
    
    [Benchmark]
    public Result<EquationSystem> Parse_Simple2x2() 
        => _parser.Parse(_simple2x2);
    
    [Benchmark]
    public Result<EquationSystem> Parse_Simple3x3() 
        => _parser.Parse(_simple3x3);
    
    [Benchmark]
    public Result<EquationSystem> Parse_Decimals() 
        => _parser.Parse(_decimals);
    
    [Benchmark]
    public Result<EquationSystem> Parse_Negatives() 
        => _parser.Parse(_negatives);
    
    [Benchmark]
    public Result<EquationSystem> Parse_Sparse() 
        => _parser.Parse(_sparse);
    
    [Benchmark]
    public Result<EquationSystem> Parse_Large5x5() 
        => _parser.Parse(_large5x5);
    
    private string GenerateLargeEquationString(int n)
    {
        var equations = new List<string>();
        for (int i = 0; i < n; i++)
        {
            var terms = new List<string>();
            for (int j = 0; j < n; j++)
            {
                var coeff = (i * n + j + 1);
                terms.Add($"{coeff}x{j + 1}");
            }
            var constant = n * n + i + 1;
            equations.Add($"{string.Join(" + ", terms)} = {constant}");
        }
        return string.Join("; ", equations);
    }
}
```

### Solving Benchmarks

```csharp
using BenchmarkDotNet.Attributes;
using MathNet.Numerics.LinearAlgebra;
using Neo.Application.Solver.Equation;

namespace NeoBenchmark.Solving;

[MemoryDiagnoser]
public class SolvingBenchmarks
{
    private EquationSolver _solver;
    private EquationSystem _system2x2;
    private EquationSystem _system3x3;
    private EquationSystem _system5x5;
    
    [GlobalSetup]
    public void Setup()
    {
        _solver = CreateEquationSolver();
        _system2x2 = CreateSystem(2);
        _system3x3 = CreateSystem(3);
        _system5x5 = CreateSystem(5);
    }
    
    [Benchmark]
    public Result<Solution> Solve_2x2_LU() 
        => _solver.Solve(_system2x2);
    
    [Benchmark]
    public Result<Solution> Solve_3x3_LU() 
        => _solver.Solve(_system3x3);
    
    [Benchmark]
    public Result<Solution> Solve_5x5_LU() 
        => _solver.Solve(_system5x5);
    
    [Benchmark]
    public Result<Solution> Solve_3x3_QR() 
        => _solver.SolveWithAlgorithm(_system3x3, SolvingAlgorithm.QR);
    
    [Benchmark]
    public Result<Solution> Solve_3x3_Cholesky() 
        => _solver.SolveWithAlgorithm(_system3x3, SolvingAlgorithm.Cholesky);
    
    [Benchmark]
    public Result<Solution> Solve_3x3_SVD() 
        => _solver.SolveWithAlgorithm(_system3x3, SolvingAlgorithm.SVD);
    
    private EquationSystem CreateSystem(int size) { }
    private EquationSolver CreateEquationSolver() { }
}
```

### Caching Benchmarks

```csharp
using BenchmarkDotNet.Attributes;
using Neo.Application.Caching;
using Neo.Application.Solver.Equation;

namespace NeoBenchmark.Caching;

[MemoryDiagnoser]
public class CachingBenchmarks
{
    private EquationSolver _solverWithCache;
    private EquationSolver _solverNoCache;
    private string _equation = "2x + 3y = 8; x - y = 1";
    
    [GlobalSetup]
    public void Setup()
    {
        _solverWithCache = CreateSolver(enableCache: true);
        _solverNoCache = CreateSolver(enableCache: false);
        
        // Warm up cache
        _solverWithCache.Solve(_equation);
    }
    
    [Benchmark(Baseline = true)]
    public Result<Solution> Solve_WithCache_Hit() 
        => _solverWithCache.Solve(_equation);
    
    [Benchmark]
    public Result<Solution> Solve_NoCache() 
        => _solverNoCache.Solve(_equation);
    
    [Benchmark]
    public Result<Solution> Solve_WithCache_Miss() 
    {
        var result = _solverWithCache.Solve(_equation);
        // Simulate cache miss by using different input each time
        return result;
    }
}
```

### Batch Processing Benchmarks

```csharp
using BenchmarkDotNet.Attributes;
using Neo.Application.Solver.Equation;

namespace NeoBenchmark.Batch;

[MemoryDiagnoser]
public class BatchBenchmarks
{
    private EquationSolver _solver;
    private List<string> _batch10;
    private List<string> _batch100;
    private List<string> _batch1000;
    
    [GlobalSetup]
    public void Setup()
    {
        _solver = CreateEquationSolver();
        _batch10 = GenerateEquations(10);
        _batch100 = GenerateEquations(100);
        _batch1000 = GenerateEquations(1000);
    }
    
    [Benchmark]
    public async Task<Result<IReadOnlyList<Solution>>> Batch_10() 
        => await _solver.SolveBatchAsync(_batch10, CancellationToken.None);
    
    [Benchmark]
    public async Task<Result<IReadOnlyList<Solution>>> Batch_100() 
        => await _solver.SolveBatchAsync(_batch100, CancellationToken.None);
    
    [Benchmark]
    public async Task<Result<IReadOnlyList<Solution>>> Batch_1000() 
        => await _solver.SolveBatchAsync(_batch1000, CancellationToken.None);
}
```

---

## Running Benchmarks

### Basic Execution

```bash
# Run all benchmarks
dotnet run --project NeoBenchmark/NeoBenchmark.csproj --configuration Release

# Run specific benchmark class
dotnet run --project NeoBenchmark/NeoBenchmark.csproj --configuration Release -- --filter NeoBenchmark.Parsing.ParsingBenchmarks

# Run specific benchmark method
dotnet run --project NeoBenchmark/NeoBenchmark.csproj --configuration Release -- --filter "*Parse_Simple2x2*"
```

### Export Results

```bash
# Export to CSV
dotnet run --project NeoBenchmark/NeoBenchmark.csproj --configuration Release -- --exporters csv

# Export to JSON
dotnet run --project NeoBenchmark/NeoBenchmark.csproj --configuration Release -- --exporters json

# Export to HTML
dotnet run --project NeoBenchmark/NeoBenchmark.csproj --configuration Release -- --exporters html
```

### Compare Versions

```bash
# Run with baseline comparison
dotnet run --project NeoBenchmark/NeoBenchmark.csproj --configuration Release -- --join
```

---

## Performance Targets

| Category | Target | Current | Status |
|----------|--------|---------|--------|
| Parse 2x2 | < 10 μs | TBD | ⏳ |
| Parse 5x5 | < 50 μs | TBD | ⏳ |
| Solve 2x2 LU | < 100 μs | TBD | ⏳ |
| Solve 5x5 LU | < 500 μs | TBD | ⏳ |
| Cache Hit | < 5 μs | TBD | ⏳ |
| Batch 100 (parallel) | < 10 ms | TBD | ⏳ |
| Memory per Solve | < 5 KB | TBD | ⏳ |

---

## Analysis Guidelines

### Interpreting Results

1. **Mean Time** - Average execution time
2. **StdDev** - Consistency indicator (lower is better)
3. **Gen 0/1/2** - Garbage collection pressure
4. **Allocated** - Memory allocated per operation

### Identifying Bottlenecks

- High allocation → Review string operations, LINQ usage
- High Gen 2 → Check for long-lived object creation
- High StdDev → Look for external dependencies, GC interference
- Slow baseline → Profile with dotTrace or PerfView

### Optimization Strategies

1. **Reduce Allocations**
   - Use `Span<T>` for parsing
   - Pool frequently allocated objects
   - Avoid LINQ in hot paths

2. **Improve Caching**
   - Optimize hash computation
   - Use faster cache key generation
   - Consider distributed cache for scale

3. **Parallel Processing**
   - Tune `MaxDegreeOfParallelism`
   - Use `ParallelOptions` for cancellation
   - Balance throughput vs. latency

---

## Continuous Integration

### GitHub Actions Workflow

```yaml
name: Benchmarks

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  benchmarks:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Run Benchmarks
        run: dotnet run --project NeoBenchmark/NeoBenchmark.csproj --configuration Release -- --exporters json
      
      - name: Upload Results
        uses: actions/upload-artifact@v3
        with:
          name: benchmark-results
          path: BenchmarkDotNet.Artifacts/
```

---

## Reporting

### Monthly Performance Report

Generate monthly reports tracking:

- Mean execution time trends
- Memory allocation trends
- Regression detection (>10% slowdown)
- Improvement highlights

### Dashboard Integration

Consider integrating with:

- **BenchmarkDotNet.Annotations** for historical tracking
- **InfluxDB + Grafana** for visualization
- **Azure DevOps** for CI/CD integration

---

## References

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [MathNet.Numerics Performance](https://numerics.mathdotnet.com/Performance)
- [.NET Performance Best Practices](https://docs.microsoft.com/dotnet/core/performance/)

---

*Last Updated: February 22, 2026*
