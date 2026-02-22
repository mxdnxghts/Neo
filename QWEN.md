# Neo Project - Context Guide

## Project Overview

**Neo** is a multi-platform linear equation and matrix solver application targeting Android, iOS, and Windows platforms. The app allows users to solve systems of linear equations using various numerical algorithms (LU, QR, Cholesky, SVD decomposition).

### Key Features
- Parse and solve linear equation systems from string input (e.g., `"2x + 3y = 5; x - y = 1"`)
- Matrix-based solving with multiple algorithm support
- Batch processing and streaming solutions
- Caching with configurable TTL
- Performance monitoring
- MAUI-based mobile UI with visual equation builder

### Tech Stack
- **.NET 10.0** (multi-platform: `net10.0`, `net10.0-android`, `net10.0-windows`)
- **MAUI** for cross-platform mobile/desktop UI
- **MathNet.Numerics** for matrix operations
- **CommunityToolkit.Mvvm** for MVVM pattern
- **NUnit** for testing
- **Tesseract** for OCR (planned equation recognition)

---

## Solution Structure

```
Neo/
├── Neo.sln                          # Main solution file
├── Neo/
│   └── Neo/                         # Core library (Domain + Application + Infrastructure)
│       ├── Domain/                  # Domain layer (EquationSystem, Solution, Result<T>)
│       ├── Application/             # Application services (EquationSolver, Caching, Validators)
│       ├── Infrastructure/          # Parsing, Matrix operations, Integration
│       └── Services/                # Legacy solver (being phased out)
├── NeoAndroidApp/                   # MAUI Android/iOS app
├── NeoConsole/                      # Console application
├── NeoBenchmark/                    # BenchmarkDotNet benchmarks
├── TestNeoSoftware/                 # Unit and integration tests
├── NeoTelemetry/                    # Telemetry/OpenTelemetry project (WIP)
└── packages/                        # Local NuGet packages
```

### Clean Architecture Layers

```
┌─────────────────────────────────────┐
│   Presentation Layer                │
│   (MAUI, Android, Console)          │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Application Layer                 │
│   - EquationSolver                  │
│   - Batch Processing                │
│   - Caching                         │
│   - Validators                      │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Domain Layer                      │
│   - EquationSystem                  │
│   - Solution                        │
│   - Variable                        │
│   - Result<T> Pattern               │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Infrastructure Layer              │
│   - Parsing                         │
│   - Matrix Conversion               │
│   - Performance Monitoring          │
└─────────────────────────────────────┘
```

---

## Building and Running

### Prerequisites
- .NET 10.0 SDK or later
- Visual Studio 2022 with MAUI workload (for mobile apps)
- NuGet packages folder: `G:\NuGetPackages` (configured in `NuGet.Config`)

### Build Commands

```bash
# Restore all dependencies
dotnet restore Neo.sln

# Build entire solution
dotnet build Neo.sln

# Build specific project
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
```

---

## Key Components

### Domain Layer

#### `EquationSystem` (`Neo/Neo/Domain/Equation/EquationSystem.cs`)
Represents a system of linear equations. Key methods:
- `ToMatrix()` - Converts to matrix form (coefficients matrix + constants vector)
- `Normalize()` - Ensures all equations contain all variables
- Properties: `EquationCount`, `VariableCount`, `IsSquare`

#### `Solution` (`Neo/Neo/Domain/Solution/`)
Represents the solution result with status (`Success`, `NoSolution`, `InfiniteSolutions`).

#### `Result<T>` Pattern (`Neo/Neo/Domain/Result/`)
Functional error handling without exceptions:
```csharp
Result<Solution> result = solver.Solve("2x + 3y = 5; x - y = 1");
if (result.IsSuccess) { /* use result.Value */ }
else { /* handle result.Error */ }
```

### Application Layer

#### `IEquationSolver` / `EquationSolver` (`Neo/Neo/Application/Solver/Equation/`)
Primary interface for solving equations:
```csharp
// Synchronous
Result<Solution> Solve(string equationInput);
Result<Solution> Solve(EquationSystem system);

// Asynchronous
Task<Result<Solution>> SolveAsync(string input, CancellationToken ct);

// Batch processing
Task<Result<IReadOnlyList<Solution>>> SolveBatchAsync(IEnumerable<string> inputs, CancellationToken ct);

// Streaming
IAsyncEnumerable<Solution> SolveStreamAsync(IAsyncEnumerable<string> stream, CancellationToken ct);

// With options
Result<Solution> SolveWithOptions(string input, SolvingOptions options);
Result<Solution> SolveWithAlgorithm(string input, SolvingAlgorithm algorithm);
```

#### `SolvingOptions` (record)
```csharp
public record SolvingOptions
{
    public bool EnableCaching { get; set; } = true;
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromMinutes(30);
    public SolvingAlgorithm DefaultAlgorithm { get; set; } = SolvingAlgorithm.LU;
    public double ValidationTolerance { get; set; } = 1e-10;
    public int MaxDegreeOfParallelism { get; set; } = -1;
}
```

#### `SolvingAlgorithm` (enum)
- `LU` - LU decomposition (fast for square matrices)
- `QR` - QR decomposition (non-square matrices)
- `Cholesky` - Cholesky decomposition (symmetric positive-definite)
- `SVD` - Singular Value Decomposition (most stable)

### Infrastructure

#### `PerformanceMonitor` (`Neo/Neo/Infrastructure/Performance/`)
Thread-safe performance tracking with counters for operations, duration, success rate.

#### `IEquationCache` (`Neo/Neo/Application/Caching/`)
Cache abstraction with SHA-256 hash keys. Uses `NullEquationCache` when caching disabled.

---

## Development Conventions

### Code Style
- **Nullable reference types**: Disabled in core (`Nullable>disable</Nullable>`), enabled in MAUI app
- **C# language version**: `preview` (latest features)
- **Naming**: PascalCase for public APIs, camelCase for private members
- **Interfaces**: `I` prefix (e.g., `IEquationSolver`, `IMatrixConverter`)

### Architecture Patterns
- **Clean Architecture**: Strict layer separation (Domain → Application → Infrastructure → Presentation)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection throughout
- **Result Pattern**: Functional error handling instead of exceptions
- **Repository/Service**: Infrastructure abstractions via interfaces

### Testing Practices
- **Framework**: NUnit with FluentAssertions and Moq
- **Test types**: Unit tests, integration tests, benchmarks
- **Naming**: `{ClassUnderTest}{MethodUnderTest}{Scenario}` (e.g., `EquationSolver_Solve_InvalidInputReturnsError`)

### Git Conventions
- **Branch naming**: `feature/`, `bugfix/`, `hotfix/` prefixes
- **Commit messages**: Conventional commits format
- **PR reviews**: Required for main branch

---

## Known Issues / Technical Debt

### High Priority
1. **Target framework**: `net10.0-windows` is non-standard; should use `net8.0` or `net9.0`
2. **Legacy code coexistence**: Old `Solver` class in `Neo.Services` vs new `EquationSolver`
3. **Incomplete migration**: Domain events defined but not published/dispatched
4. **Async implementation**: `SolveAsync` wraps sync `Solve` in `Task.Run` (thread pool overhead)

### Medium Priority
5. **Incomplete validation**: `EquationSystem.Validate()` needs full implementation
6. **Missing DI registration**: `IEquationSolver` not registered via extension method
7. **Test coverage**: Many tests reference old `Solver`, not `EquationSolver`
8. **Documentation**: XML docs enabled but many APIs lack comments

---

## File Locations Reference

| Component | Path |
|-----------|------|
| Core library | `Neo/Neo/Neo.csproj` |
| MAUI app | `NeoAndroidApp/NeoAndroidApp.csproj` |
| Tests | `TestNeoSoftware/TestNeoSoftware.csproj` |
| Benchmarks | `NeoBenchmark/NeoBenchmark.csproj` |
| EquationSolver | `Neo/Neo/Application/Solver/Equation/EquationSolver.cs` |
| EquationSystem | `Neo/Neo/Domain/Equation/EquationSystem.cs` |
| Solution | `Neo/Neo/Domain/Solution/` |
| Result pattern | `Neo/Neo/Domain/Result/` |
| MAUI ViewModel | `NeoAndroidApp/ViewModels/` |
| MAUI Views | `NeoAndroidApp/Views/` |

---

## Useful Commands

```bash
# Check for build errors
dotnet build Neo.sln --no-restore

# Run with verbose output
dotnet run --project NeoConsole/NeoConsole.csproj --verbosity detailed

# Publish Android APK (Release)
dotnet publish NeoAndroidApp/NeoAndroidApp.csproj -f net10.0-android -c Release

# Analyze code (if analyzers configured)
dotnet format Neo.sln --verify-no-changes

# View NuGet package tree
dotnet list Neo/Neo/Neo.csproj package
```

---

## External Resources
- [Architecture Analysis](./ARCHITECTURE_ANALYSIS.md) - Detailed architecture review
- [Mobile App Implementation](./mobile-app.md) - MAUI equation builder plan
- [Security Policy](./SECURITY.md) - Vulnerability reporting
- [Updates Log](./UPDATES.md) - Planned features and changelog
