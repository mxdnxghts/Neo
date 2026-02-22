# Neo

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MAUI](https://img.shields.io/badge/MAUI-10.0-512BD4?logo=dotnet)](https://docs.microsoft.com/dotnet/maui/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

**Neo** is a multi-platform linear equation and matrix solver application for Android, iOS, and Windows. Solve systems of linear equations using advanced numerical algorithms including LU, QR, Cholesky, and SVD decomposition.

![Neo Banner](./docs/images/banner.png)

---

## Features

### 🧮 Equation Solving
- Parse linear equations from string input (e.g., `"2x + 3y = 5; x - y = 1"`)
- Visual matrix builder for constructing equations without typing
- Support for 2 to 6 variables
- Real-time input validation with visual feedback

### ⚡ Multiple Algorithms
- **LU Decomposition** - Fast solving for square matrices
- **QR Decomposition** - Handle non-square matrices
- **Cholesky** - Optimized for symmetric positive-definite matrices
- **SVD** - Most numerically stable solution

### 📱 Cross-Platform
- **Android** - Native mobile experience
- **iOS** - Coming soon
- **Windows** - Desktop application
- **Console** - CLI tool for batch processing

### 🚀 Performance
- Intelligent caching with SHA-256 hash keys
- Parallel batch processing
- Streaming solutions for large datasets
- Thread-safe performance monitoring

---

## Quick Start

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Visual Studio 2022 with MAUI workload (for mobile apps)
- Android SDK 21.0 or later (for Android)

### Installation

```bash
# Clone the repository
git clone https://github.com/mxdnxghts/Neo.git
cd Neo

# Restore dependencies
dotnet restore Neo.sln

# Build the solution
dotnet build Neo.sln
```

### Running

```bash
# Run the console application
dotnet run --project NeoConsole/NeoConsole.csproj

# Run the MAUI app (Windows)
dotnet run -f net10.0-windows -c Debug --project NeoAndroidApp/NeoAndroidApp.csproj

# Run on Android device/emulator
dotnet build -t:Run -f net10.0-android -c Debug NeoAndroidApp/NeoAndroidApp.csproj
```

---

## Usage Examples

### Console Application

```csharp
using Neo.Application.Solver.Equation;

var solver = serviceProvider.GetRequiredService<IEquationSolver>();

// Solve a simple 2x2 system
var result = solver.Solve("2x + 3y = 8; x - y = 1");

if (result.IsSuccess)
{
    foreach (var variable in result.Value.Values)
    {
        Console.WriteLine($"{variable.Key} = {variable.Value}");
    }
}
```

### Visual Matrix Builder

1. Launch the MAUI application
2. Navigate to **Matrix Builder**
3. Enter coefficients for each variable
4. Add or remove equations as needed
5. Tap **Solve System** to see the solution

![Matrix Builder](./docs/images/matrix-builder.png)

### Batch Processing

```csharp
var equations = new[]
{
    "2x + 3y = 8; x - y = 1",
    "x + y + z = 6; 2x - y + z = 3; x + 2y - z = 2",
    "3a + 2b = 10; a - b = 1"
};

var solutions = await solver.SolveBatchAsync(equations, CancellationToken.None);
```

---

## Project Structure

```
Neo/
├── Neo/Neo/                    # Core library (Domain + Application + Infrastructure)
│   ├── Domain/                 # Business entities (EquationSystem, Solution, Result<T>)
│   ├── Application/            # Services (EquationSolver, Caching, Validators)
│   └── Infrastructure/         # Parsing, Matrix operations, Integration
├── NeoAndroidApp/              # MAUI mobile application
│   ├── Views/                  # XAML pages
│   ├── ViewModels/             # MVVM view models
│   └── Models/                 # Data models
├── NeoConsole/                 # Console application
├── NeoBenchmark/               # Performance benchmarks
├── TestNeoSoftware/            # Unit and integration tests
└── NeoTelemetry/               # OpenTelemetry integration (WIP)
```

---

## Architecture

Neo follows **Clean Architecture** principles with strict layer separation:

```
┌─────────────────────────────────────┐
│   Presentation Layer                │
│   (MAUI, Console, Web API)          │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Application Layer                 │
│   (EquationSolver, Caching, Batch)  │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Domain Layer                      │
│   (Entities, Value Objects, Events) │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Infrastructure Layer              │
│   (Parsing, Matrix, Database)       │
└─────────────────────────────────────┘
```

### Key Design Patterns

- **Result Pattern** - Functional error handling without exceptions
- **Dependency Injection** - Microsoft.Extensions.DependencyInjection
- **Null Object Pattern** - Optional dependencies (e.g., caching)
- **Options Pattern** - Configuration records

---

## Testing

```bash
# Run all tests
dotnet test TestNeoSoftware/TestNeoSoftware.csproj

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run benchmarks
dotnet run --project NeoBenchmark/NeoBenchmark.csproj --configuration Release
```

---

## Documentation

| Document | Description |
|----------|-------------|
| [PROJECT.md](./PROJECT.md) | Complete project documentation |
| [ARCHITECTURE_ANALYSIS.md](./ARCHITECTURE_ANALYSIS.md) | Detailed architecture review |
| [mobile-app.md](./mobile-app.md) | MAUI equation builder implementation plan |
| [SECURITY.md](./SECURITY.md) | Security policy and vulnerability reporting |
| [UPDATES.md](./UPDATES.md) | Changelog and planned features |

---

## Roadmap

### Version 1.5 (Next)
- [ ] Named variables support (x, y, z)
- [ ] Equation history with save/load
- [ ] Tutorial overlay

### Version 2.0 (Major)
- [ ] OCR equation recognition (Tesseract)
- [ ] Export results to PDF/image
- [ ] Step-by-step solution display

### Version 2.5+ (Future)
- [ ] Graph visualization for 2-variable systems
- [ ] Complex number support
- [ ] Matrix operations (inverse, determinant, eigenvalues)

---

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow existing code conventions (PascalCase for public APIs)
- Add unit tests for new features
- Update documentation as needed
- Use conventional commits format

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Support

- **Issues**: [GitHub Issues](https://github.com/mxdnxghts/Neo/issues)
- **Discussions**: [GitHub Discussions](https://github.com/mxdnxghts/Neo/discussions)
- **Email**: [Contact via GitHub](https://github.com/mxdnxghts)

---

## Acknowledgments

- [MathNet.Numerics](https://numerics.mathdotnet.com/) - Matrix operations library
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/dotnet/communitytoolkit/mvvm/) - MVVM helpers
- [.NET MAUI](https://docs.microsoft.com/dotnet/maui/) - Cross-platform UI framework

---

<p align="center">Made with ❤️ by <a href="https://github.com/mxdnxghts">mxdnxghts</a></p>
