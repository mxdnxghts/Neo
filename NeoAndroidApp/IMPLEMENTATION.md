# Neo Mobile App - Implementation Guide

## Quick Start

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022 with MAUI workload
- Windows 10/11 (for Windows development)

### Build and Run

```bash
# Restore dependencies
dotnet restore NeoAndroidApp/NeoAndroidApp.csproj

# Build
dotnet build NeoAndroidApp/NeoAndroidApp.csproj

# Run on Windows
dotnet run -f net10.0-windows10.0.19041.0 -c Debug --project NeoAndroidApp/NeoAndroidApp.csproj
```

---

## Implementation Details

### 1. Project Setup

#### NeoAndroidApp.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0-windows10.0.19041.0</TargetFrameworks>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageReference Include="Microsoft.Maui.Controls" Version="10.0.41" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\Neo\Neo\Neo.csproj" />
  </ItemGroup>
</Project>
```

#### Neo.csproj (Core Library)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0</TargetFrameworks>
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="MathNet.Numerics" Version="5.0.0" />
    <PackageReference Include="FluentResults" Version="4.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.2" />
  </ItemGroup>
</Project>
```

---

### 2. Model Implementation

#### EquationRow.cs

```csharp
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NeoAndroidApp.Models;

/// <summary>
/// Represents a single equation row in the matrix builder.
/// Contains coefficients for each variable and a constant term.
/// </summary>
public partial class EquationRow : ObservableObject
{
    /// <summary>
    /// Gets the collection of coefficient values (as strings for two-way binding).
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _coefficients = new();

    /// <summary>
    /// Gets or sets the constant term (as string for two-way binding).
    /// </summary>
    [ObservableProperty]
    private string _constant = "0";

    /// <summary>
    /// Initializes a new equation row with the specified number of variables.
    /// </summary>
    public EquationRow(int variableCount)
    {
        for (int i = 0; i < variableCount; i++)
            Coefficients.Add("0");
    }

    /// <summary>
    /// Updates the coefficients collection when variable count changes.
    /// </summary>
    public void UpdateCoefficientCount(int newCount)
    {
        var currentCount = Coefficients.Count;

        if (newCount > currentCount)
        {
            for (int i = currentCount; i < newCount; i++)
                Coefficients.Add("0");
        }
        else if (newCount < currentCount)
        {
            while (Coefficients.Count > newCount)
                Coefficients.RemoveAt(Coefficients.Count - 1);
        }
    }

    /// <summary>
    /// Validates that all coefficients and the constant are valid numbers.
    /// </summary>
    public bool IsValid()
    {
        foreach (var coeff in Coefficients)
        {
            if (!double.TryParse(coeff, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                return false;
        }

        return double.TryParse(Constant, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }
}
```

---

### 3. ViewModel Implementation

#### MatrixBuilderViewModel.cs

```csharp
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Neo.Application.Solver.Equation;
using Neo.Domain.Solution;
using NeoAndroidApp.Models;

namespace NeoAndroidApp.ViewModels;

/// <summary>
/// ViewModel for the matrix builder page.
/// </summary>
public partial class MatrixBuilderViewModel : ObservableObject
{
    private readonly IEquationSolver _solver;
    private readonly List<string> _variableOptions = new() { "2", "3", "4", "5" };

    public List<string> VariableOptions => _variableOptions;

    [ObservableProperty]
    private int _variableCount = 2;

    [ObservableProperty]
    private ObservableCollection<EquationRow> _equations = new();

    [ObservableProperty]
    private string _resultText = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Gets whether all equations are valid.
    /// </summary>
    public bool IsValid => Equations.All(e => e.IsValid());

    /// <summary>
    /// Gets whether there is an error message.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Gets the variable count index for picker binding (0-based).
    /// </summary>
    public int VariableCountIndex
    {
        get => VariableCount - 2;
        set
        {
            if (value >= 0 && value < _variableOptions.Count)
                VariableCount = value + 2;
        }
    }

    public MatrixBuilderViewModel(IEquationSolver solver)
    {
        _solver = solver;
        InitializeEquations();
    }

    private void InitializeEquations()
    {
        Equations.Clear();
        Equations.Add(new EquationRow(VariableCount));
        OnPropertyChanged(nameof(IsValid));
    }

    partial void OnVariableCountChanged(int value)
    {
        foreach (var equation in Equations)
            equation.UpdateCoefficientCount(value);

        OnPropertyChanged(nameof(IsValid));
    }

    [RelayCommand]
    private void AddEquation()
    {
        Equations.Add(new EquationRow(VariableCount));
        OnPropertyChanged(nameof(IsValid));
        ClearError();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveEquation))]
    private void RemoveEquation(EquationRow equation)
    {
        if (equation != null && Equations.Count > 1)
        {
            Equations.Remove(equation);
            OnPropertyChanged(nameof(IsValid));
            ClearError();
        }
    }

    private bool CanRemoveEquation(EquationRow equation) => Equations.Count > 1;

    [RelayCommand(CanExecute = nameof(CanSolve))]
    private async Task Solve()
    {
        if (!ValidateAll())
        {
            ErrorMessage = "Please enter valid numbers for all coefficients and constants.";
            return;
        }

        IsBusy = true;
        ClearError();
        ResultText = string.Empty;

        try
        {
            var equationString = BuildEquationString();
            var result = await Task.Run(() => _solver.Solve(equationString));

            if (result.IsSuccess && result.Value != null)
                DisplaySolution(result.Value);
            else
                DisplayError(
                    result.Error?.Code ?? "UNKNOWN_ERROR",
                    result.Error?.Message ?? "An unknown error occurred.");
        }
        catch (Exception ex)
        {
            DisplayError("SOLVE_ERROR", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSolve() => !IsBusy && Equations.Count > 0;

    private bool ValidateAll() => Equations.All(e => e.IsValid());

    private string BuildEquationString()
    {
        var rows = new List<string>();

        foreach (var equation in Equations)
        {
            var terms = new List<string>();

            for (int i = 0; i < equation.Coefficients.Count; i++)
            {
                if (double.TryParse(equation.Coefficients[i], NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var coeff))
                {
                    var term = FormatTerm(coeff, i);
                    if (!string.IsNullOrEmpty(term))
                        terms.Add(term);
                }
            }

            if (double.TryParse(equation.Constant, NumberStyles.Any, 
                CultureInfo.InvariantCulture, out _))
            {
                rows.Add($"{string.Join(" ", terms)} = {equation.Constant}");
            }
        }

        return string.Join("; ", rows);
    }

    private string FormatTerm(double coeff, int varIndex)
    {
        if (Math.Abs(coeff) < 1e-10)
            return string.Empty;

        var sign = coeff >= 0 ? "+" : "-";
        var abs = Math.Abs(coeff);

        var coeffStr = abs switch
        {
            1 => "",
            _ => abs.ToString("G6", CultureInfo.InvariantCulture)
        };

        return $"{sign}{coeffStr}x{varIndex + 1}";
    }

    private void DisplaySolution(Solution solution)
    {
        if (solution.Status != SolutionStatus.Success)
        {
            DisplayError(solution.Status.ToString(), solution.Message ?? "Unknown error");
            return;
        }

        var lines = solution.Values
            .Select(kvp => $"{kvp.Key} = {kvp.Value:F6}")
            .ToList();

        ResultText = string.Join(Environment.NewLine, lines);
    }

    private void DisplayError(string code, string message)
    {
        ErrorMessage = $"[{code}] {message}";
    }

    private void ClearError()
    {
        ErrorMessage = string.Empty;
    }

    partial void OnEquationsChanged(ObservableCollection<EquationRow> value)
    {
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(CanSolve));
        ClearError();
    }
}
```

---

### 4. View Implementation

#### MatrixBuilderPage.xaml

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:converters="clr-namespace:NeoAndroidApp.Converters"
             xmlns:viewmodels="clr-namespace:NeoAndroidApp.ViewModels"
             x:Class="NeoAndroidApp.Views.MatrixBuilderPage"
             x:DataType="viewmodels:MatrixBuilderViewModel"
             Title="Matrix Builder">

    <ContentPage.Resources>
        <converters:IsValidDoubleConverter x:Key="IsValidDoubleConverter" />
        <converters:InverseBoolConverter x:Key="InverseBoolConverter" />
    </ContentPage.Resources>

    <ScrollView>
        <VerticalStackLayout Spacing="15" Padding="20">

            <!-- Variable count picker -->
            <Frame BorderColor="#CCCCCC" Padding="15" CornerRadius="10">
                <VerticalStackLayout Spacing="10">
                    <Label Text="Number of Variables"
                           FontSize="16"
                           FontAttributes="Bold"/>
                    <Picker x:Name="VariablePicker"
                            SelectedIndex="{Binding VariableCountIndex, Mode=TwoWay}"
                            HorizontalOptions="Fill"
                            BackgroundColor="#F5F5F5">
                        <Picker.Items>
                            <x:String>2</x:String>
                            <x:String>3</x:String>
                            <x:String>4</x:String>
                            <x:String>5</x:String>
                        </Picker.Items>
                    </Picker>
                </VerticalStackLayout>
            </Frame>

            <!-- Headers -->
            <Frame BorderColor="#CCCCCC" Padding="15" CornerRadius="10">
                <HorizontalStackLayout Spacing="10" HorizontalOptions="Fill">
                    <Label Text="Coef." WidthRequest="50" FontAttributes="Bold"/>
                    <HorizontalStackLayout Spacing="5" HorizontalOptions="FillAndExpand">
                        <BindableLayout.ItemsSource>
                            <Binding Path="VariableLabels" />
                        </BindableLayout.ItemsSource>
                        <BindableLayout.ItemTemplate>
                            <DataTemplate>
                                <Label Text="{Binding .}"
                                       WidthRequest="60"
                                       FontAttributes="Bold"
                                       HorizontalTextAlignment="Center"/>
                            </DataTemplate>
                        </BindableLayout.ItemTemplate>
                    </HorizontalStackLayout>
                    <Label Text="=" WidthRequest="20" FontAttributes="Bold"/>
                    <Label Text="Const." WidthRequest="60" FontAttributes="Bold"/>
                    <Label WidthRequest="40"/>
                </HorizontalStackLayout>
            </Frame>

            <!-- Equations list -->
            <CollectionView ItemsSource="{Binding Equations}">
                <CollectionView.ItemTemplate>
                    <DataTemplate>
                        <Frame BorderColor="#DDDDDD" Padding="10" Margin="0,5" CornerRadius="8">
                            <HorizontalStackLayout Spacing="10">
                                <!-- Coefficient entries -->
                                <HorizontalStackLayout Spacing="5" HorizontalOptions="FillAndExpand">
                                    <BindableLayout.ItemsSource>
                                        <Binding Path="Coefficients" />
                                    </BindableLayout.ItemsSource>
                                    <BindableLayout.ItemTemplate>
                                        <DataTemplate>
                                            <Entry Text="{Binding .}"
                                                   Keyboard="Numeric"
                                                   WidthRequest="60">
                                                <Entry.Triggers>
                                                    <DataTrigger TargetType="Entry"
                                                                 Binding="{Binding ., Converter={StaticResource IsValidDoubleConverter}}"
                                                                 Value="False">
                                                        <Setter Property="BackgroundColor" Value="#FFDDDD"/>
                                                        <Setter Property="TextColor" Value="Red"/>
                                                    </DataTrigger>
                                                </Entry.Triggers>
                                            </Entry>
                                        </DataTemplate>
                                    </BindableLayout.ItemTemplate>
                                </HorizontalStackLayout>

                                <!-- Equals sign -->
                                <Label Text="=" VerticalOptions="Center" FontSize="18"/>

                                <!-- Constant entry -->
                                <Entry Text="{Binding Constant}"
                                       Keyboard="Numeric"
                                       WidthRequest="70">
                                    <Entry.Triggers>
                                        <DataTrigger TargetType="Entry"
                                                     Binding="{Binding Constant, Converter={StaticResource IsValidDoubleConverter}}"
                                                     Value="False">
                                            <Setter Property="BackgroundColor" Value="#FFDDDD"/>
                                            <Setter Property="TextColor" Value="Red"/>
                                        </DataTrigger>
                                    </Entry.Triggers>
                                </Entry>

                                <!-- Delete button -->
                                <Button Text="✖"
                                        TextColor="White"
                                        BackgroundColor="#DC3545"
                                        CornerRadius="20"
                                        WidthRequest="40"
                                        Command="{Binding Source={RelativeSource AncestorType={x:Type viewmodels:MatrixBuilderViewModel}}, Path=RemoveEquationCommand}"
                                        CommandParameter="{Binding .}"/>
                            </HorizontalStackLayout>
                        </Frame>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>

            <!-- Add equation button -->
            <Button Text="+ Add Equation"
                    Command="{Binding AddEquationCommand}"
                    BackgroundColor="#28A745"
                    TextColor="White"
                    CornerRadius="10"
                    HeightRequest="50"/>

            <!-- Solve button -->
            <Button Text="Solve System"
                    Command="{Binding SolveCommand}"
                    BackgroundColor="#007BFF"
                    TextColor="White"
                    CornerRadius="10"
                    HeightRequest="50"
                    FontAttributes="Bold"/>

            <!-- Activity indicator -->
            <ActivityIndicator IsRunning="{Binding IsBusy}"
                               IsVisible="{Binding IsBusy}"
                               Color="#007BFF"/>

            <!-- Error message -->
            <Frame BorderColor="#DC3545"
                   BackgroundColor="#FFF5F5"
                   Padding="15"
                   CornerRadius="10"
                   IsVisible="{Binding HasError}">
                <Label Text="{Binding ErrorMessage}"
                       TextColor="#DC3545"
                       LineBreakMode="WordWrap"/>
            </Frame>

            <!-- Result -->
            <Frame BorderColor="#28A745"
                   BackgroundColor="#F0FFF4"
                   Padding="20"
                   CornerRadius="10"
                   IsVisible="{Binding ResultText, Converter={StaticResource StringIsEmptyConverter}, ConverterParameter=inverse}">
                <VerticalStackLayout Spacing="10">
                    <Label Text="Solution:"
                           FontSize="18"
                           FontAttributes="Bold"
                           TextColor="#28A745"/>
                    <Label Text="{Binding ResultText}"
                           FontSize="16"
                           LineBreakMode="WordWrap"/>
                </VerticalStackLayout>
            </Frame>

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

---

### 5. Converter Implementation

#### MatrixBuilderConverters.cs

```csharp
using System.Globalization;

namespace NeoAndroidApp.Converters;

/// <summary>
/// Converts a string to true if it's a valid double value.
/// </summary>
public class IsValidDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Inverts a boolean value.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return true;
    }
}
```

---

### 6. App Configuration

#### MauiProgram.cs

```csharp
using Microsoft.Extensions.Logging;
using Neo.Application.Solver.Equation;
using Neo.CompositionRoot;
using NeoAndroidApp.Converters;
using NeoAndroidApp.ViewModels;
using NeoAndroidApp.Views;

namespace NeoAndroidApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register converters
        builder.Services.AddSingleton<IsValidDoubleConverter>();
        builder.Services.AddSingleton<InverseBoolConverter>();

        // Register Neo equation solver
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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```

#### AppShell.xaml.cs

```csharp
using NeoAndroidApp.Views;

namespace NeoAndroidApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes
        Routing.RegisterRoute(nameof(MatrixBuilderPage), typeof(MatrixBuilderPage));
    }
}
```

---

## Usage Examples

### Example 1: Simple 2x2 System

**Equations:**
- 2x₁ + 3x₂ = 8
- x₁ - x₂ = 1

**Input:**
1. Set variables to 2
2. Row 1: Coefficients [2, 3], Constant [8]
3. Row 2: Coefficients [1, -1], Constant [1]
4. Tap "Solve System"

**Result:**
```
x1 = 2.200000
x2 = 1.200000
```

### Example 2: 3x3 System

**Equations:**
- x₁ + x₂ + x₃ = 6
- 2x₁ - x₂ + x₃ = 3
- x₁ + 2x₂ - x₃ = 2

**Input:**
1. Set variables to 3
2. Add 3 equation rows
3. Enter coefficients and constants
4. Tap "Solve System"

**Result:**
```
x1 = 1.000000
x2 = 2.000000
x3 = 3.000000
```

---

## Troubleshooting

### Build Errors

**Error: XamlCompilation failed**
- Ensure legacy Xamarin files are removed from Neo project
- Check Neo.csproj targets `net10.0` not `net10.0-windows`

**Error: PRI generation failed**
- Neo.Core should target `net10.0` (not Windows-specific)
- MAUI app targets Windows framework

**Error: Dependency resolution failed**
- Verify `AddNeoEquationSolver()` called in MauiProgram
- Check all services registered with correct lifetime

### Runtime Issues

**Solve button disabled**
- Check all coefficients and constants are valid numbers
- Invalid fields highlighted in red

**No result displayed**
- Check ErrorMessage property for error details
- Verify Neo.Core is functioning correctly

**Picker not updating**
- Ensure `VariableCountIndex` property used (not `VariableCount`)
- Check TwoWay binding mode in XAML

---

## Performance Tips

1. **Limit equation count** - Keep under 20 equations for best performance
2. **Use appropriate variable count** - 2-5 variables recommended
3. **Avoid rapid changes** - Let validation complete before solving
4. **Clear errors** - Errors auto-clear on input change

---

## Code Style Guidelines

### Naming Conventions

- **ViewModels:** `{Feature}ViewModel` (e.g., `MatrixBuilderViewModel`)
- **Views:** `{Feature}Page` (e.g., `MatrixBuilderPage`)
- **Models:** Plain names (e.g., `EquationRow`)
- **Converters:** `{Description}Converter` (e.g., `IsValidDoubleConverter`)

### MVVM Pattern

- Use `ObservableObject` from CommunityToolkit
- Use `[ObservableProperty]` attribute for properties
- Use `[RelayCommand]` attribute for commands
- Keep views code-behind minimal

### XAML Style

- Use `x:DataType` for compiled bindings
- Define resources at page level
- Use styles for consistent appearance
- Comment section purposes

---

## Testing Checklist

- [ ] Build succeeds without errors
- [ ] App launches successfully
- [ ] Navigation to Matrix Builder works
- [ ] Variable picker updates UI
- [ ] Add equation adds new row
- [ ] Remove equation removes row (when >1)
- [ ] Invalid input highlighted in red
- [ ] Solve button disabled when invalid
- [ ] Valid system produces correct solution
- [ ] Error messages display correctly
- [ ] Activity indicator shows during solve
- [ ] Results clear on new input

---

## Next Steps

1. **Add unit tests** for ViewModel logic
2. **Implement file import/export** for equation sets
3. **Add undo/redo** functionality
4. **Create tutorial** for first-time users
5. **Add unit tests** for edge cases
6. **Implement dark mode** support
7. **Add accessibility** features

---

## References

- [MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [XAML Bindings](https://docs.microsoft.com/dotnet/maui/xaml/)
- [Neo Core API](../Neo/Neo/Application/Solver/Equation/IEquationSolver.cs)
