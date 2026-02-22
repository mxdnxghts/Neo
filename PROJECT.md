# Neo Project - Complete Documentation

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Implementation](#implementation)
4. [Features](#features)
5. [Technology Stack](#technology-stack)
6. [Solution Structure](#solution-structure)
7. [Building and Running](#building-and-running)
8. [Key Components](#key-components)
9. [Development Conventions](#development-conventions)
10. [Testing](#testing)
11. [Deployment](#deployment)
12. [Future Roadmap](#future-roadmap)

---

## Project Overview

**Neo** is a multi-platform linear equation and matrix solver application targeting Android, iOS, and Windows platforms. The application enables users to solve systems of linear equations using various numerical algorithms including LU decomposition, QR decomposition, Cholesky decomposition, and Singular Value Decomposition (SVD).

### Core Capabilities

- **Equation Parsing**: Parse linear equation systems from string input (e.g., `"2x + 3y = 5; x - y = 1"`)
- **Matrix Operations**: Convert equations to matrix form and solve using MathNet.Numerics
- **Multiple Algorithms**: Support for LU, QR, Cholesky, and SVD decomposition methods
- **Batch Processing**: Solve multiple equation systems in parallel
- **Streaming Solutions**: Process equation streams with `IAsyncEnumerable`
- **Intelligent Caching**: SHA-256 hash-based caching with configurable TTL
- **Performance Monitoring**: Thread-safe performance tracking and statistics
- **Visual Matrix Builder**: MAUI-based UI for constructing equations visually

### Target Audience

- Students learning linear algebra
- Engineers solving system equations
- Researchers needing quick matrix solutions
- Educational institutions

---

## Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐  │
│  │   MAUI App      │  │  Console App    │  │  (Future)   │  │
│  │  (Android/iOS)  │  │                 │  │   Web API   │  │
│  └─────────────────┘  └─────────────────┘  └─────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │EquationSolver│  │BatchProcessor│  │  Caching Service │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │  Validators  │  │  Options     │  │ Performance Mgr  │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      DOMAIN LAYER                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │EquationSystem│  │   Solution   │  │   Variable       │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │ LinearEquation│  │ Result<T>    │  │  Domain Events   │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   INFRASTRUCTURE LAYER                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │   Parsing    │  │   Matrix     │  │  Performance     │   │
│  │  (Parser)    │  │  Conversion  │  │   Monitoring     │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │   Caching    │  │  Integration │  │    (Future)      │   │
│  │  (SHA-256)   │  │  (Database)  │  │   Telemetry      │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Flow

```
Presentation → Application → Domain ← Infrastructure
```

**Key Principles:**
- Domain layer has **zero dependencies** on external packages
- Application layer depends only on Domain
- Infrastructure implements interfaces from Application/Domain
- Presentation depends on Application interfaces (Dependency Inversion)

---

## Implementation

### Domain Layer

#### EquationSystem
Represents a system of linear equations with validation and matrix conversion.

```csharp
public sealed class EquationSystem
{
    public IReadOnlyList<LinearEquation> Equations { get; }
    public IReadOnlyList<Variable> Variables { get; }
    public int EquationCount { get; }
    public int VariableCount { get; }
    public bool IsSquare => EquationCount == VariableCount;
    
    public EquationSystem Normalize();
    public (double[,] Coefficients, double[] Constants) ToMatrix();
}
```

#### Solution
Represents the solution result with status tracking.

```csharp
public sealed class Solution
{
    public SolutionStatus Status { get; } // Success, NoSolution, InfiniteSolutions
    public IReadOnlyDictionary<Variable, double> Values { get; }
    public EquationSystem System { get; }
    public string? Message { get; }
    
    public static Solution Success(EquationSystem system, Dictionary<Variable, double> values);
    public static Solution NoSolution(EquationSystem system);
    public static Solution InfiniteSolutions(EquationSystem system);
}
```

#### Result<T> Pattern
Functional error handling without exceptions.

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Error? Error { get; }
    
    public static Result<T> Success(T value);
    public static Result<T> Failure(Error error);
    
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper);
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> mapper);
}
```

### Application Layer

#### IEquationSolver
Primary interface for solving equations.

```csharp
public interface IEquationSolver
{
    // Synchronous
    Result<Solution> Solve(string equationInput);
    Result<Solution> Solve(EquationSystem system);
    Result<Solution> Solve(Matrix<double> coefficients, Vector<double> constants);
    
    // Asynchronous
    Task<Result<Solution>> SolveAsync(string equationInput, CancellationToken ct = default);
    Task<Result<Solution>> SolveAsync(EquationSystem system, CancellationToken ct = default);
    
    // Batch processing
    Task<Result<IReadOnlyList<Solution>>> SolveBatchAsync(
        IEnumerable<string> inputs, CancellationToken ct = default);
    
    // Streaming
    IAsyncEnumerable<Solution> SolveStreamAsync(
        IAsyncEnumerable<string> equationStream, CancellationToken ct = default);
    
    // With options
    Result<Solution> SolveWithOptions(string input, SolvingOptions options);
    Result<Solution> SolveWithAlgorithm(string input, SolvingAlgorithm algorithm);
}
```

#### SolvingOptions
Configuration record for solving behavior.

```csharp
public record SolvingOptions
{
    public bool EnableCaching { get; set; } = true;
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromMinutes(30);
    public SolvingAlgorithm DefaultAlgorithm { get; set; } = SolvingAlgorithm.LU;
    public double ValidationTolerance { get; set; } = 1e-10;
    public int MaxDegreeOfParallelism { get; set; } = -1; // Use default
}
```

#### SolvingAlgorithm
Available solving algorithms.

```csharp
public enum SolvingAlgorithm
{
    LU,         // LU decomposition (fast for square matrices)
    QR,         // QR decomposition (non-square matrices)
    Cholesky,   // Cholesky decomposition (symmetric positive-definite)
    SVD         // Singular Value Decomposition (most stable)
}
```

### Infrastructure Layer

#### PerformanceMonitor
Thread-safe performance tracking.

```csharp
public sealed class PerformanceMonitor
{
    public IDisposable StartActivity(string operationName);
    public PerformanceStats GetStats();
    public void Reset();
}

public record PerformanceStats
{
    public long TotalOperations { get; }
    public long SuccessfulOperations { get; }
    public TimeSpan TotalDuration { get; }
    public TimeSpan MinDuration { get; }
    public TimeSpan MaxDuration { get; }
    public TimeSpan AverageDuration { get; }
    public double SuccessRate { get; }
}
```

#### IEquationCache
Caching abstraction with null object pattern.

```csharp
public interface IEquationCache
{
    void SetSolution(string cacheKey, Solution solution, TimeSpan ttl);
    Result<Solution?> GetSolution(string cacheKey);
    void Invalidate(string cacheKey);
    void Clear();
}

// Null object pattern - used when caching is disabled
public sealed class NullEquationCache : IEquationCache
{
    public void SetSolution(string cacheKey, Solution solution, TimeSpan ttl) { }
    public Result<Solution?> GetSolution(string cacheKey) => Result<Solution?>.Success(null);
    public void Invalidate(string cacheKey) { }
    public void Clear() { }
}
```

---

## Features

### Current Features

| Feature | Status | Description |
|---------|--------|-------------|
| Equation Parsing | ✅ Complete | Parse string input to EquationSystem |
| Matrix Conversion | ✅ Complete | Convert equations to matrix form |
| LU Decomposition | ✅ Complete | Fast solving for square matrices |
| QR Decomposition | ✅ Complete | Handle non-square matrices |
| Cholesky | ✅ Complete | Optimized for symmetric positive-definite |
| SVD | ✅ Complete | Most numerically stable |
| Result Pattern | ✅ Complete | Functional error handling |
| Caching | ✅ Complete | SHA-256 hash-based with TTL |
| Batch Processing | ✅ Complete | Parallel solving of multiple systems |
| Streaming | ✅ Complete | IAsyncEnumerable support |
| Performance Monitor | ✅ Complete | Thread-safe statistics |
| MAUI Visual Builder | ✅ Complete | Visual equation construction |
| Dark Theme | ✅ Complete | AppThemeBinding support |
| Input Validation | ✅ Complete | Real-time validation with visual feedback |

### Planned Features

| Feature | Priority | Target Version |
|---------|----------|----------------|
| OCR Recognition | High | 2.0 |
| Named Variables | Medium | 1.5 |
| Equation History | Medium | 1.5 |
| Export Results | Low | 2.0 |
| Step-by-Step Solution | Low | 2.5 |
| Graph Visualization | Low | 3.0 |
| Cloud Sync | Low | 3.0 |

---

## Technology Stack

### Core Technologies

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | .NET | 10.0 |
| Mobile UI | MAUI | 10.0 |
| MVVM Toolkit | CommunityToolkit.Mvvm | 8.4.0 |
| Matrix Math | MathNet.Numerics | 5.0.0 |
| DI Container | Microsoft.Extensions.DependencyInjection | 10.0.2 |
| OCR (planned) | Tesseract | 5.2.0 |

### Project Target Frameworks

```xml
<!-- Core Library -->
<TargetFrameworks>net10.0</TargetFrameworks>

<!-- MAUI App -->
<TargetFramework>net10.0-android</TargetFramework>
<SupportedOSPlatformVersion Condition="'$(TargetPlatformIdentifier)' == 'android'">21.0</SupportedOSPlatformVersion>
```

### NuGet Packages

**Neo.Core:**
- MathNet.Numerics (5.0.0) - Matrix operations
- FluentResults (4.0.0) - Result pattern (alternative to custom implementation)
- Microsoft.Extensions.* (10.0.x) - DI, Configuration
- Dapper (2.1.66) - Database access (future)
- Npgsql (5.0.18) - PostgreSQL provider (future)
- System.Reactive (6.1.0) - Reactive extensions

**NeoAndroidApp:**
- CommunityToolkit.Mvvm (8.4.0) - MVVM helpers
- Microsoft.Maui.Controls (10.0.41) - MAUI framework
- Tesseract (5.2.0) - OCR engine

---

## Solution Structure

```
Neo/
├── Neo.sln                          # Main solution file
├── Neo/
│   └── Neo/                         # Core library
│       ├── Application/             # Application services
│       │   ├── Caching/             # Cache abstractions
│       │   ├── Solver/
│       │   │   ├── Equation/        # EquationSolver
│       │   │   └── Matrix/          # Matrix solving
│       │   └── Validators/          # Input/solution validators
│       ├── Domain/                  # Domain entities
│       │   ├── Equation/            # EquationSystem, LinearEquation
│       │   ├── Solution/            # Solution, SolutionStatus
│       │   ├── Result/              # Result<T>, Error
│       │   └── Variables/           # Variable, Coefficient
│       ├── Infrastructure/          # Implementations
│       │   ├── Integration/         # External integrations
│       │   ├── Matrix/              # Matrix operations
│       │   ├── Parsing/             # Equation parsing
│       │   └── Performance/         # Performance monitoring
│       ├── CompositionRoot/         # DI registration
│       └── Neo.csproj
├── NeoAndroidApp/                   # MAUI mobile app
│   ├── Views/                       # XAML pages
│   ├── ViewModels/                  # ViewModels
│   ├── Models/                      # Data models
│   ├── Converters/                  # Value converters
│   ├── Resources/                   # Assets, fonts, images
│   └── NeoAndroidApp.csproj
├── NeoConsole/                      # Console application
├── NeoBenchmark/                    # BenchmarkDotNet benchmarks
├── TestNeoSoftware/                 # Unit/integration tests
├── NeoTelemetry/                    # OpenTelemetry (WIP)
└── packages/                        # Local NuGet packages
```

---

## Building and Running

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022 with MAUI workload (for mobile apps)
- NuGet packages folder configured in `NuGet.Config`

### Build Commands

```bash
# Restore all dependencies
dotnet restore Neo.sln

# Build entire solution
dotnet build Neo.sln

# Build specific projects
dotnet build Neo/Neo/Neo.csproj
dotnet build NeoAndroidApp/NeoAndroidApp.csproj
dotnet build TestNeoSoftware/TestNeoSoftware.csproj

# Run tests
dotnet test TestNeoSoftware/TestNeoSoftware.csproj

# Run benchmarks
dotnet run --project NeoBenchmark/NeoBenchmark.csproj --configuration Release
```

### Run Applications

```bash
# Run console app
dotnet run --project NeoConsole/NeoConsole.csproj

# Run MAUI app (Windows)
dotnet run -f net10.0-windows -c Debug --project NeoAndroidApp/NeoAndroidApp.csproj

# Run MAUI app (Android emulator)
dotnet build -t:Run -f net10.0-android -c Debug NeoAndroidApp/NeoAndroidApp.csproj

# Publish Android APK (Release)
dotnet publish NeoAndroidApp/NeoAndroidApp.csproj -f net10.0-android -c Release
```

---

## Key Components

### EquationSolver

The central orchestrator for solving equations:

1. **Parse** input string to `EquationSystem`
2. **Convert** to matrix form (coefficients + constants)
3. **Validate** system solvability
4. **Select** optimal algorithm based on matrix properties
5. **Solve** using MathNet.Numerics
6. **Validate** solution accuracy
7. **Cache** result if enabled

### Algorithm Selection Logic

```
Is matrix non-square? → Use QR
Is matrix symmetric AND positive-definite? → Use Cholesky
Otherwise → Use configured default (LU)
```

### Caching Strategy

- **Key Generation**: SHA-256 hash of normalized input
- **TTL**: Configurable (default 30 minutes)
- **Null Object**: `NullEquationCache` when disabled
- **Thread-Safe**: Concurrent dictionary implementation

### MAUI Matrix Builder

Visual equation construction with:
- Dynamic row/column management (2-6 variables)
- Real-time validation with visual feedback
- Dark/light theme support
- Horizontal scrolling for wide equations
- Add/remove equations with validation

---

## Development Conventions

### Code Style

| Setting | Value |
|---------|-------|
| Nullable Reference Types | Disabled (core), Enabled (MAUI) |
| C# Language Version | `preview` |
| Documentation File | Enabled (XML docs) |
| Naming | PascalCase (public), camelCase (private) |

### Architecture Patterns

- **Clean Architecture**: Strict layer separation
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Result Pattern**: Functional error handling
- **Null Object Pattern**: Optional dependencies
- **Options Pattern**: Configuration records

### Git Conventions

```
Branch naming:
  feature/<description>
  bugfix/<description>
  hotfix/<description>

Commit messages (Conventional Commits):
  feat: Add new feature
  fix: Fix bug
  docs: Update documentation
  refactor: Code refactoring
  test: Add tests
  chore: Maintenance
```

---

## Testing

### Test Framework

- **Unit Tests**: NUnit
- **Assertions**: FluentAssertions
- **Mocking**: Moq
- **Benchmarks**: BenchmarkDotNet

### Test Categories

```csharp
[TestFixture]
public class EquationSolver_Solve_InvalidInput
{
    [Test]
    public void EmptyString_ReturnsError() { }
    
    [Test]
    public void MalformedEquation_ReturnsError() { }
    
    [Test]
    public void UnderdeterminedSystem_ReturnsError() { }
}
```

### Running Tests

```bash
# All tests
dotnet test TestNeoSoftware/TestNeoSoftware.csproj

# With coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Specific test category
dotnet test --filter "Category=Unit"
```

---

## Deployment

### Android Deployment

```bash
# Debug APK
dotnet build -t:Run -f net10.0-android -c Debug NeoAndroidApp/NeoAndroidApp.csproj

# Release APK
dotnet publish NeoAndroidApp/NeoAndroidApp.csproj \
  -f net10.0-android \
  -c Release \
  /p:AndroidPackageFormat=apk \
  /p:AndroidKeyStore=true \
  /p:AndroidSigningKeyStore=<keystore> \
  /p:AndroidSigningKeyAlias=<alias>
```

### Configuration

| Setting | Value |
|---------|-------|
| Application ID | `com.companyname.neoandroidapp` |
| Display Version | 1.0 |
| Version Code | 1 |
| Min SDK | API 21 (Android 5.0) |
| Package Format | APK |

---

## Future Roadmap

### Version 1.5 (Next Release)

- [ ] Named variables support (x, y, z instead of x1, x2, x3)
- [ ] Equation history with save/load
- [ ] Improved error messages with suggestions
- [ ] Tutorial overlay for first-time users

### Version 2.0 (Major Release)

- [ ] OCR equation recognition using Tesseract
- [ ] Export results to PDF/image
- [ ] Step-by-step solution display
- [ ] Cloud backup integration

### Version 2.5+ (Future)

- [ ] Graph visualization for 2-variable systems
- [ ] Complex number support
- [ ] Matrix operations (inverse, determinant, eigenvalues)
- [ ] Web API for remote solving

---

## Known Issues & Technical Debt

### High Priority

1. **Target Framework**: `net10.0-windows` is non-standard; migrate to `net8.0` or `net9.0`
2. **Legacy Code**: Old `Solver` class coexists with new `EquationSolver`
3. **Domain Events**: Defined but not published/dispatched
4. **Async Implementation**: `SolveAsync` wraps sync `Solve` (thread pool overhead)

### Medium Priority

5. **Validation**: `EquationSystem.Validate()` incomplete
6. **DI Registration**: `IEquationSolver` not registered via extension method
7. **Test Coverage**: Many tests reference old `Solver`
8. **Documentation**: XML docs enabled but many APIs lack comments

---

## References

- [Architecture Analysis](./ARCHITECTURE_ANALYSIS.md)
- [Mobile App Implementation](./mobile-app.md)
- [Security Policy](./SECURITY.md)
- [Updates Log](./UPDATES.md)
- [MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [MathNet.Numerics](https://numerics.mathdotnet.com/)

---

*Last Updated: February 22, 2026*
