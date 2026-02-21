# Neo Mobile App - Architecture Documentation

## Overview

The Neo Mobile App is a .NET MAUI application that provides a visual equation system builder for students and engineers. It allows users to construct and solve linear equation systems without typing raw equation strings, leveraging the Neo Core solver engine.

**Target Platforms:** Windows, Android, iOS, macOS  
**Framework:** .NET MAUI 10.0  
**Architecture Pattern:** MVVM (Model-View-ViewModel)

---

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Neo Mobile App                            │
│                     (.NET MAUI Presentation)                     │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │   Views     │  │  ViewModels │  │      Models             │  │
│  │  (XAML UI)  │◄─┤   (Logic)   │◄─┤  (Data Structures)      │  │
│  └─────────────┘  └──────┬──────┘  └─────────────────────────┘  │
│                          │                                       │
│                          ▼                                       │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │              Dependency Injection Container                 │  │
│  │         (Microsoft.Extensions.DependencyInjection)          │  │
│  └────────────────────┬───────────────────────────────────────┘  │
│                       │                                          │
│                       ▼                                          │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │                    Neo Core Library                         │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │  IEquationSolver (Application Layer)                  │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │  Domain Layer (Entities, Value Objects)               │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │  Infrastructure Layer (Parsing, Matrix Operations)    │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Component Layers

#### 1. Presentation Layer (NeoAndroidApp)

The MAUI application containing all UI components:

- **Views** - XAML-based pages
- **ViewModels** - Business logic for UI
- **Models** - Data structures
- **Converters** - Value converters for data binding

#### 2. Application Layer (Neo.Core)

Core application services:

- `IEquationSolver` - Main solving interface
- `EquationSolver` - Solver orchestrator
- `SolvingOptions` - Configuration options
- `MemoryEquationCache` - Caching service

#### 3. Domain Layer (Neo.Core)

Business entities and value objects:

- `EquationSystem` - Collection of equations
- `LinearEquation` - Single linear equation
- `Variable` - Variable with coefficient
- `Solution` - Solution with status
- `Result<T>` - Monadic result type

#### 4. Infrastructure Layer (Neo.Core)

Technical implementations:

- `EquationParser` - Text parsing
- `EquationTokenizer` - Lexical analysis
- `MatrixConverter` - Matrix operations
- `PerformanceMonitor` - Performance tracking

---

## Directory Structure

```
NeoAndroidApp/
├── App.xaml                      # Application definition
├── AppShell.xaml                 # App shell with navigation
├── MainPage.xaml                 # Main entry page
├── MauiProgram.cs                # App entry & DI configuration
│
├── Views/
│   └── MatrixBuilderPage.xaml    # Equation builder UI
│
├── ViewModels/
│   └── MatrixBuilderViewModel.cs # Builder business logic
│
├── Models/
│   └── EquationRow.cs            # Equation row data model
│
├── Converters/
│   └── MatrixBuilderConverters.cs # Value converters
│
└── Resources/
    ├── Fonts/
    ├── Images/
    ├── Raw/
    └── Styles/
```

---

## Key Components

### MatrixBuilderPage (View)

**File:** `Views/MatrixBuilderPage.xaml`

Visual equation builder interface with:
- Variable count picker (2-5 variables)
- Dynamic coefficient entry grid
- Add/remove equation rows
- Real-time validation feedback
- Solve button with loading state
- Result/error display panels

**Key Features:**
```xml
<!-- Variable picker -->
<Picker SelectedIndex="{Binding VariableCountIndex, Mode=TwoWay}">
    <Picker.Items>
        <x:String>2</x:String>
        <x:String>3</x:String>
        <x:String>4</x:String>
        <x:String>5</x:String>
    </Picker.Items>
</Picker>

<!-- Dynamic coefficient entries -->
<BindableLayout.ItemsSource>
    <Binding Path="Coefficients" />
</BindableLayout.ItemsSource>

<!-- Validation triggers -->
<DataTrigger TargetType="Entry"
             Binding="{Binding ., Converter={StaticResource IsValidDoubleConverter}}"
             Value="False">
    <Setter Property="BackgroundColor" Value="#FFDDDD"/>
</DataTrigger>
```

### MatrixBuilderViewModel (ViewModel)

**File:** `ViewModels/MatrixBuilderViewModel.cs`

Implements MVVM pattern with CommunityToolkit.Mvvm:

**Properties:**
| Property | Type | Description |
|----------|------|-------------|
| `VariableCount` | `int` | Number of variables (2-5) |
| `VariableCountIndex` | `int` | Picker index (0-based) |
| `Equations` | `ObservableCollection<EquationRow>` | Equation list |
| `ResultText` | `string` | Solution display |
| `ErrorMessage` | `string` | Error display |
| `IsBusy` | `bool` | Loading state |
| `IsValid` | `bool` | Validation state |

**Commands:**
| Command | Type | Description |
|---------|------|-------------|
| `AddEquationCommand` | `RelayCommand` | Add new equation row |
| `RemoveEquationCommand` | `RelayCommand` | Remove equation row |
| `SolveCommand` | `AsyncRelayCommand` | Solve equation system |

**Key Methods:**
```csharp
// Validates all coefficients and constants
private bool ValidateAll() => Equations.All(e => e.IsValid());

// Builds equation string for solver
private string BuildEquationString()
{
    // Produces: "2x1 + 3x2 = 5; x1 - x2 = 1"
}

// Formats individual terms
private string FormatTerm(double coeff, int varIndex)
{
    // Handles sign, coefficient omission for 1, etc.
}
```

### EquationRow (Model)

**File:** `Models/EquationRow.cs`

Represents a single equation row:

```csharp
public class EquationRow : ObservableObject
{
    public ObservableCollection<string> Coefficients { get; }
    public string Constant { get; set; }
    
    public void UpdateCoefficientCount(int newCount)
    {
        // Adjusts coefficient collection size
    }
    
    public bool IsValid()
    {
        // Validates all values parse as double
    }
}
```

### Converters

**File:** `Converters/MatrixBuilderConverters.cs`

**IsValidDoubleConverter:**
- Converts string to boolean
- Returns `true` if value parses as double
- Used for validation highlighting

**InverseBoolConverter:**
- Inverts boolean values
- Used for enabling/disabling UI elements

---

## Data Flow

### Equation Solving Flow

```
User Input (UI)
     │
     ▼
Entry Binding (Two-way)
     │
     ▼
EquationRow.Coefficients
     │
     ▼
MatrixBuilderViewModel.SolveCommand
     │
     ├─► ValidateAll()
     │       │
     │       └─► double.TryParse() on each field
     │
     ├─► BuildEquationString()
     │       │
     │       └─► "2x1 + 3x2 = 5; x1 - x2 = 1"
     │
     └─► IEquationSolver.Solve()
             │
             ├─► Success ► DisplaySolution()
             │       └─► "x1 = 2.0\nx2 = 1.0"
             │
             └─► Error ► DisplayError()
                     └─► "[CODE] Message"
```

### Variable Count Change Flow

```
User selects variable count
     │
     ▼
Picker.SelectedIndex changed
     │
     ▼
VariableCountIndex property setter
     │
     ▼
VariableCount = value + 2
     │
     ▼
OnVariableCountChanged()
     │
     ▼
Update all EquationRow.UpdateCoefficientCount()
     │
     ▼
UI updates via INotifyPropertyChanged
```

---

## Dependency Injection

### Registration (MauiProgram.cs)

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder.UseMauiApp<App>()
           .ConfigureFonts(fonts => { ... });
    
    // Register converters
    builder.Services.AddSingleton<IsValidDoubleConverter>();
    builder.Services.AddSingleton<InverseBoolConverter>();
    
    // Register Neo solver with options
    builder.Services.AddNeoEquationSolver(options =>
    {
        options.EnableCaching = true;
        options.CacheTtl = TimeSpan.FromMinutes(30);
        options.DefaultAlgorithm = SolvingAlgorithm.LU;
        options.ValidationTolerance = 1e-10;
    });
    
    // Register ViewModels and Views
    builder.Services.AddTransient<MatrixBuilderViewModel>();
    builder.Services.AddTransient<MatrixBuilderPage>();
    
    return builder.Build();
}
```

### Service Lifetimes

| Service | Lifetime | Reason |
|---------|----------|--------|
| Converters | Singleton | Stateless, shared |
| IEquationSolver | Scoped | Maintains state per operation |
| ViewModel | Transient | New instance per navigation |
| Page | Transient | New instance per navigation |

---

## Validation Strategy

### Input Validation

1. **Per-field validation** via `IsValidDoubleConverter`
   - Runs on each keystroke
   - Highlights invalid fields in red
   - Uses `CultureInfo.InvariantCulture`

2. **Pre-solve validation** via `ValidateAll()`
   - Checks all fields before solving
   - Prevents invalid solve attempts
   - Shows error message if invalid

### Validation Rules

```csharp
// Valid inputs
"0"        ✓
"-5.5"     ✓
"3.14159"  ✓
"1e-10"    ✓
""         ✗ (empty)
"abc"      ✗ (non-numeric)
"-"        ✗ (incomplete)
```

---

## Error Handling

### Error Display Pattern

```csharp
private void DisplayError(string code, string message)
{
    ErrorMessage = $"[{code}] {message}";
}

private void ClearError()
{
    ErrorMessage = string.Empty;
}
```

### Error Sources

| Source | Error Code | Handling |
|--------|-----------|----------|
| Invalid input | `VALIDATION_ERROR` | Pre-solve check |
| Parse failure | `PARSE_ERROR` | Solver result |
| No solution | `NO_SOLUTION` | Solver result |
| Infinite solutions | `INFINITE_SOLUTIONS` | Solver result |
| System error | `SOLVE_ERROR` | Exception catch |

### Exception Handling

```csharp
try
{
    var result = await Task.Run(() => _solver.Solve(equationString));
    
    if (result.IsSuccess)
        DisplaySolution(result.Value);
    else
        DisplayError(result.Error.Code, result.Error.Message);
}
catch (Exception ex)
{
    DisplayError("SOLVE_ERROR", ex.Message);
}
```

---

## UI/UX Design

### Layout Structure

```
┌─────────────────────────────────────┐
│  Number of Variables: [Picker ▼]    │
├─────────────────────────────────────┤
│  Headers: x₁  x₂  ...  =  Const.    │
├─────────────────────────────────────┤
│  [Entry] [Entry] ... = [Entry] [✖] │
│  [Entry] [Entry] ... = [Entry] [✖] │
│  ...                                 │
├─────────────────────────────────────┤
│  [+ Add Equation]                    │
│  [Solve System]                      │
├─────────────────────────────────────┤
│  [Error Frame - Red]                 │
│  [Result Frame - Green]              │
└─────────────────────────────────────┘
```

### Visual States

| State | Indicator |
|-------|-----------|
| Valid input | Normal background |
| Invalid input | Red background (#FFDDDD) |
| Solving | ActivityIndicator visible |
| Success | Green result frame |
| Error | Red error frame |

### Responsive Design

- Wrapped in `ScrollView` for small screens
- Fixed entry widths (60px) for consistency
- Flexible button heights (50px) for touch
- Frame borders for visual grouping

---

## Performance Considerations

### Async Operations

- Solver runs on background thread via `Task.Run()`
- UI remains responsive during solving
- `IsBusy` flag prevents concurrent solves

### Memory Management

- `ObservableCollection` for efficient UI updates
- No unnecessary allocations in validation
- Equation cache configured with TTL (30 min)

### Optimization Opportunities

1. **Debounce validation** - Currently validates on every change
2. **Cache equation strings** - Rebuild only when changed
3. **Virtualize long lists** - For 10+ equations

---

## Testing Strategy

### Unit Tests (ViewModel)

```csharp
// Test validation
[Fact]
public void IsValid_WithAllValidEntries_ReturnsTrue()
{
    var viewModel = new MatrixBuilderViewModel(solver);
    viewModel.Equations[0].Coefficients[0] = "2.5";
    viewModel.Equations[0].Constant = "5";
    
    Assert.True(viewModel.IsValid);
}

// Test equation building
[Fact]
public void BuildEquationString_ProducesCorrectFormat()
{
    // Expected: "2x1 + 3x2 = 5"
}
```

### Integration Tests

- Test full solve flow with mock solver
- Verify navigation and DI resolution
- Test error handling paths

### UI Tests (Future)

- Use MAUI UITest framework
- Test user interactions
- Verify visual states

---

## Security Considerations

### Input Sanitization

- All input treated as strings initially
- `double.TryParse` prevents injection
- No raw equation string from user

### Dependency Security

- Neo.Core is trusted internal library
- No external API calls
- Local file access only via FilePicker

---

## Future Extensions

### Planned Features

1. **Named variables** - Custom variable names (x, y, z)
2. **Load/Save** - Persist equation sets
3. **Undo/Redo** - Command pattern implementation
4. **Matrix input mode** - Direct matrix entry
5. **Equation templates** - Common system presets
6. **Step-by-step solution** - Educational mode

### Platform-Specific Enhancements

| Platform | Enhancement |
|----------|-------------|
| Android | Back button navigation |
| iOS | Safe area layout |
| Windows | Keyboard shortcuts |
| macOS | Menu bar integration |

---

## References

- [MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [Neo Core Architecture](../Neo/Neo/architecture-plan.md)
- [Implementation Plan](../mobile-app.md)
