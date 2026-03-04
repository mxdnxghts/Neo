# Architecture Plan for the Linear Equation Solver System

This document outlines the architecture of a high‑performance, production‑ready linear equation solver built with C# and .NET. The system follows **Clean Architecture** principles to ensure maintainability, testability, and flexibility. It is designed to parse human‑readable equations, convert them to matrices, solve them using various algorithms (LU, QR, Cholesky, SVD), and return solutions with validation and caching.

## 1. Architectural Overview

The system is divided into four concentric layers:

- **Domain Layer** – Core business logic and entities.
- **Application Layer** – Use case orchestration (the solver service).
- **Infrastructure Layer** – Concrete implementations of external dependencies (parsing, matrix conversion, caching, performance monitoring).
- **Presentation / Composition Root** – Entry point (console app, web API) that wires everything together via Dependency Injection.

Dependencies point **inward**: the Application layer depends only on Domain and abstractions; Infrastructure implements those abstractions; Presentation references all layers.

```
┌─────────────────────────────────────────────┐
│          Presentation / Composition Root     │
│  (Console, Web API, DI Container, Startup)   │
└─────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────┐
│            Application Layer                 │
│  (IEquationSolver, EquationSolver, Options)  │
└─────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────┐
│            Domain Layer                      │
│  (Variable, Coefficient, LinearEquation,     │
│   EquationSystem, Solution, Result<T>)       │
└─────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────┐
│          Infrastructure Layer                │
│  (EquationParser, MatrixConverter,           │
│   MathNetMatrixSolver, SolutionValidator,    │
│   MemoryEquationCache, PerformanceMonitor)   │
└─────────────────────────────────────────────┘
```

## 2. Layer Details

### 2.1 Domain Layer (Neo.Domain)

**Purpose:** Represent the core business concepts of linear equations, independent of any external concerns.

**Key Components:**

- **`Variable`** – Value object representing a variable name (e.g., "x"). Immutable, equatable.
- **`Coefficient`** – Value object pairing a `double` value with a `Variable`.
- **`LinearEquation`** – Entity containing a dictionary of `Variable` → coefficient, and a constant term.
  - Validates that at least one coefficient is non‑zero.
  - Provides methods like `GetCoefficient`, `HasVariable`, `WithZeroCoefficient` (used during normalisation).
  - Implements `IEquatable<LinearEquation>`.
- **`EquationSystem`** – Collection of `LinearEquation` objects.
  - Exposes `Variables` (distinct variables ordered by appearance).
  - `Normalize()` adds missing variables with coefficient 0 to every equation.
  - `ToMatrix()` converts the system to `(double[,] coefficients, double[] constants)` for further processing.
- **`Solution`** – Result of solving a system. Contains status (`Success`, `NoSolution`, `InfiniteSolutions`, `Error`), dictionary of variable → value, and metadata.
- **`Result<T>`** – Functional error‑handling type with `IsSuccess`, `Value`, and `Error` (record with code, message, exception). Used throughout to avoid exceptions for expected failures.

**Dependencies:** None (only .NET base class library).

### 2.2 Application Layer (Neo.Application)

**Purpose:** Orchestrate the solving process, define use‑case interfaces, and hold configuration objects.

**Key Components:**

- **`IEquationSolver`** – Primary interface for solving equations:
  ```csharp
  Result<Solution> Solve(string input);
  Result<Solution> Solve(EquationSystem system);
  Result<Solution> Solve(Matrix<double> coefficients, Vector<double> constants);
  Task<Result<Solution>> SolveAsync(...);
  Task<Result<IReadOnlyList<Solution>>> SolveBatchAsync(...);
  IAsyncEnumerable<Solution> SolveStreamAsync(...);
  Result<Solution> SolveWithOptions(string input, SolvingOptions options);
  Result<Solution> SolveWithAlgorithm(string input, SolvingAlgorithm algorithm);
  ```
- **`EquationSolver`** – Concrete implementation of `IEquationSolver`.
  - Injected dependencies: `IEquationParser`, `IMatrixConverter`, `IMatrixSolver`, `ISolutionValidator`, `IEquationCache`, `PerformanceMonitor`, `SolvingOptions`.
  - Handles caching (via hash of input), parsing, matrix conversion, algorithm selection, solving, validation, and result packaging.
- **`SolvingOptions`** – Configuration record with properties: `EnableCaching`, `CacheTtl`, `DefaultAlgorithm`, `ValidationTolerance`, `MaxDegreeOfParallelism`.
- **`SolvingAlgorithm`** – Enum: `LU`, `QR`, `Cholesky`, `SVD`.
- **Performance monitoring abstractions** (optional) – `IPerformanceMonitor` could be defined here, but we currently use a concrete `PerformanceMonitor` in infrastructure; the Application layer depends on its interface if needed.

**Dependencies:** Domain + abstractions of infrastructure services (defined as interfaces). No concrete infrastructure types.

### 2.3 Infrastructure Layer (Neo.Infrastructure)

**Purpose:** Implement the interfaces defined in the Application layer, and provide concrete services for external concerns.

#### 2.3.1 Parsing

- **`IEquationParser`** – Interface (defined in Application or a separate Contracts project).
- **`EquationParser`** – High‑performance parser using `Span<T>`, `stackalloc`, and `ArrayPool`.
  - Uses a custom **tokenizer** (`EquationTokenizer`, ref struct) that produces `TokenInfo` structs (type, start, length).
  - Adaptive token buffer: stack‑allocated for up to 128 tokens, falls back to pooled array for larger inputs.
  - Parses tokens into an `EquationSystem`, handling signs, implicit coefficients (1 or -1), left‑side constants, and repeated variables (coefficients summed).
  - Returns `Result<EquationSystem>`.

#### 2.3.2 Matrix Conversion

- **`IMatrixConverter`** – Interface (Application layer).
- **`PooledMatrixConverter`** – Converts an `EquationSystem` to MathNet matrices using `ArrayPool<double>` to minimise allocations.
  - Uses `Parallel.For` for large systems to fill arrays concurrently.
  - Returns `Result<Matrix<double>>` and `Result<Vector<double>>`.

#### 2.3.3 Linear Algebra Solving

- **`IMatrixSolver`** – Interface (Application layer).
- **`MathNetMatrixSolver`** – Wraps MathNet.Numerics methods.
  - Provides `SolveLU`, `SolveQR`, `SolveCholesky`, `SolveSVD`.
  - `ConditionNumber` method using SVD.
  - Returns `Result<Vector<double>>` with detailed error on singular or ill‑conditioned matrices.

#### 2.3.4 Solution Validation

- **`ISolutionValidator`** – Interface (Application layer).
- **`SolutionValidator`** – Computes residuals `Ax - b` and checks against tolerance.
  - `DetermineStatus` uses rank comparison (via `a.Append(b.ToColumnMatrix()).Rank()`) to classify system as no solution, infinite solutions, or unique.
  - Returns `Result<bool>` indicating validity.

#### 2.3.5 Caching

- **`IEquationCache`** – Interface (Application layer).
- **`MemoryEquationCache`** – In‑memory cache with expiration. Uses `ConcurrentDictionary` and a simple `CacheEntry` record with expiry.
- **`NullEquationCache`** – No‑op implementation for when caching is disabled.

#### 2.3.6 Performance Monitoring

- **`PerformanceMonitor`** – Concrete class (could implement an interface).
  - Tracks operation durations, success rates, and statistics via `ConcurrentDictionary`.
  - Provides `IDisposable` `StartActivity` to measure scoped operations.

**Dependencies:** Application abstractions, MathNet.Numerics, and .NET runtime libraries. No dependencies on other infrastructure components (they are composed via DI).

### 2.4 Presentation / Composition Root

**Purpose:** Assemble the application, configure dependency injection, and expose functionality to end users.

- Example: A **console application** that reads equations from command line arguments or a file, calls `IEquationSolver`, and prints results.
- Example: An **ASP.NET Core Web API** with a controller that injects `IEquationSolver` and exposes endpoints like `POST /api/solve`.

**DI Registration** (using `Microsoft.Extensions.DependencyInjection`):

```csharp
services.AddSingleton<SolvingOptions>(sp => ...);
services.AddSingleton<IEquationParser, EquationParser>();
services.AddSingleton<IMatrixConverter, PooledMatrixConverter>();
services.AddSingleton<IMatrixSolver, MathNetMatrixSolver>();
services.AddSingleton<ISolutionValidator, SolutionValidator>();
services.AddSingleton<IEquationCache, MemoryEquationCache>();
services.AddSingleton<PerformanceMonitor>();
services.AddSingleton<IEquationSolver, EquationSolver>();
```

## 3. Data Flow (Example: Solve(string input))

1. Client calls `EquationSolver.Solve("2x + 3y = 5; x - y = 1")`.
2. `EquationSolver` computes a cache key (SHA‑256 of normalized input) and checks `IEquationCache.GetSolution(key)`. If found, returns cached solution.
3. Otherwise, it calls `IEquationParser.Parse(input)`.
   - `EquationParser` tokenizes the input using `EquationTokenizer`.
   - Builds an `EquationSystem` from tokens.
4. `EquationSolver` then uses `IMatrixConverter.ToMathNetMatrix()` and `.ToMathNetVector()` to get `Matrix<double>` and `Vector<double>`.
5. It optionally calls `ISolutionValidator.DetermineStatus()` to pre‑check consistency.
6. It selects an algorithm via `SelectAlgorithm()` (based on matrix properties and `SolvingOptions.DefaultAlgorithm`).
7. It calls `IMatrixSolver.Solve(a, b, algorithm)` to obtain the solution vector.
8. It builds a `Solution` domain object (mapping variables to values).
9. It validates the solution using `ISolutionValidator.Validate()`.
10. If caching enabled, it stores the solution in `IEquationCache`.
11. It returns `Result<Solution>` to the caller.

## 4. Performance Optimizations

The system is designed with high performance in mind, especially for parsing and matrix conversion, as these can be bottlenecks with large equation systems.

| Technique | Where Used | Benefit |
|-----------|------------|---------|
| **`Span<T>` and `ReadOnlySpan<char>`** | Tokenizer, number parsing, `ParseLeftSide` | Zero‑allocation string slicing; avoids `Substring` allocations. |
| **`stackalloc` for token buffer** | `EquationParser.TokenizeWithAdaptiveBuffer` | No heap allocation for typical systems (<128 tokens). |
| **`ArrayPool<T>`** | Token buffer fallback, equation list, matrix conversion | Reduces GC pressure by reusing arrays. |
| **`StringBuilder` pooling (not shown, but could be added)** | – | Avoid repeated `StringBuilder` allocations if needed. |
| **Fast integer parsing** | `TryParseNumber` | Custom integer parser avoids `double.Parse` for integers. |
| **`double.TryParse` on `Span<char>`** | `TryParseNumber` | .NET Core supports span‑based parsing, allocation‑free. |
| **Parallel matrix filling** | `PooledMatrixConverter.FillArraysParallel` | Uses `Parallel.For` for systems with >10 equations. |
| **Caching** | `EquationSolver` + `MemoryEquationCache` | Avoids re‑parsing and re‑solving identical inputs. |
| **Lazy algorithm selection** | `EquationSolver.SelectAlgorithm` | Chooses optimal solver based on matrix properties (e.g., Cholesky for SPD). |
| **`Result<T>` instead of exceptions** | All layers | Avoids expensive exception stack traces for expected errors. |

## 5. Key Design Patterns

| Pattern | Usage |
|---------|-------|
| **Clean Architecture** | Overall layering and dependency rule. |
| **Dependency Injection** | All components are wired via DI for loose coupling and testability. |
| **Result Object** | `Result<T>` pattern for explicit error handling. |
| **Strategy** | `IMatrixSolver` with multiple implementations (LU, QR, etc.) selected at runtime. |
| **Repository** | `IEquationCache` abstracts caching storage. |
| **Adapter** | `IMatrixConverter` adapts domain `EquationSystem` to MathNet types. |
| **Factory** | `EquationTokenizer` is created per parse operation (implicit). |
| **Disposable** | `TokenizedResult` implements `IDisposable` to return rented arrays. |

## 6. Testing Strategy

- **Unit Tests** – Focus on domain entities and `EquationSolver` (with mocked dependencies). Use xUnit/NUnit + Moq.
- **Integration Tests** – Test `EquationParser` with real inputs, `MathNetMatrixSolver` with small matrices, and the full pipeline end‑to‑end.
- **Performance Tests** – Benchmark parsing and solving with varying input sizes (using BenchmarkDotNet) to catch regressions.

**Example unit test for `EquationSolver`:**

```csharp
[Fact]
public void Solve_Valid2x2_ReturnsCorrectSolution()
{
    // Arrange
    var parserMock = new Mock<IEquationParser>();
    var system = ... // build test system
    parserMock.Setup(p => p.Parse("2x+3y=5;x-y=1"))
              .Returns(Result<EquationSystem>.Success(system));

    var converterMock = new Mock<IMatrixConverter>();
    var a = Matrix<double>.Build.DenseOfArray(new double[,] { {2,3},{1,-1} });
    var b = Vector<double>.Build.Dense(new double[] {5,1});
    converterMock.Setup(c => c.ToMathNetMatrix(system)).Returns(Result<Matrix<double>>.Success(a));
    converterMock.Setup(c => c.ToMathNetVector(system)).Returns(Result<Vector<double>>.Success(b));

    var solverMock = new Mock<IMatrixSolver>();
    var xVec = Vector<double>.Build.Dense(new double[] {2,1});
    solverMock.Setup(s => s.Solve(a, b, SolvingAlgorithm.LU))
              .Returns(Result<Vector<double>>.Success(xVec));

    var validatorMock = new Mock<ISolutionValidator>();
    validatorMock.Setup(v => v.Validate(It.IsAny<EquationSystem>(), It.IsAny<Solution>(), It.IsAny<double>()))
                 .Returns(Result<bool>.Success(true));

    var equationSolver = new EquationSolver(parserMock.Object, converterMock.Object, solverMock.Object, validatorMock.Object);

    // Act
    var result = equationSolver.Solve("2x+3y=5;x-y=1");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.GetValue(Variable.Create("x")));
    Assert.Equal(1, result.Value.GetValue(Variable.Create("y")));
}
```

## 7. Extensibility Points

- **New solving algorithms** – Implement `IMatrixSolver` and register in DI.
- **New input formats** – Implement `IEquationParser` (e.g., JSON, XML).
- **Different caching backends** – Implement `IEquationCache` (e.g., Redis, distributed cache).
- **Alternative validation rules** – Implement `ISolutionValidator`.
- **Enhanced performance monitoring** – Implement `IPerformanceMonitor` (if an interface is introduced).

## 8. Summary

This architecture provides a clean separation of concerns, making the system easy to maintain, test, and extend. Performance‑critical parts (parsing, matrix conversion) are optimised using modern .NET features, while the domain remains pure and independent. The design is ready for production use and can be adapted to various front‑ends (CLI, API, GUI) with minimal changes.