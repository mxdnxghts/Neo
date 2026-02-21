# 📱 User‑Friendly Equation System Builder – Implementation Plan

This document provides a detailed technical plan for implementing a **visual equation system builder** in a .NET MAUI mobile application. The builder is designed to help students and engineers construct linear equation systems without typing raw equation strings, while seamlessly integrating with the existing solver backend.

## 1. Overview

The builder allows users to:

- Select the number of variables (e.g., 2–5) via a picker.
- For each equation, input coefficients for each variable and a constant term.
- Add or remove equations dynamically.
- See a live preview of the system in standard mathematical notation.
- Validate input in real time (invalid fields highlighted).
- Solve the system with a single tap and view results or errors.

Internally, the builder constructs an equation string in the format expected by `IEquationSolver` (e.g., `"2x1 + 3x2 = 5; x1 - x2 = 1"`) and delegates solving to the core service.

## 2. Architecture

The builder resides entirely in the **presentation layer** (MAUI app) and follows the **MVVM pattern**. It reuses the existing `IEquationSolver` service from the application layer.

### 2.1 Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                   MAUI Presentation Layer                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │              MatrixBuilderPage (XAML)                 │  │
│  └───────────────────────────┬───────────────────────────┘  │
│                              │ (Binding)                     │
│                              ▼                                │
│  ┌───────────────────────────────────────────────────────┐  │
│  │            MatrixBuilderViewModel (C#)                 │  │
│  │  - Observable properties for UI state                  │  │
│  │  - Commands for Add/Remove/Solve                       │  │
│  │  - Validation logic                                     │  │
│  │  - Equation string construction                         │  │
│  └───────────────────────────┬───────────────────────────┘  │
│                              │ (Calls)                       │
│                              ▼                                │
│  ┌───────────────────────────────────────────────────────┐  │
│  │           IEquationSolver (injected)                   │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Dependencies

- `CommunityToolkit.Mvvm` – for `ObservableObject`, `RelayCommand`, `ObservableProperty`.
- `Microsoft.Extensions.DependencyInjection` – for injecting `IEquationSolver`.
- (Optional) `CommunityToolkit.Maui` – for converters (e.g., `BoolToObjectConverter`, `InvertedBoolConverter`).

## 3. Data Model

Define a simple model to represent one equation row.

```csharp
public class EquationRow : ObservableObject
{
    // Coefficients as strings for two-way binding with validation
    public ObservableCollection<string> Coefficients { get; } = new();

    private string _constant = "0";
    public string Constant
    {
        get => _constant;
        set => SetProperty(ref _constant, value);
    }
}
```

The number of coefficient entries in each `EquationRow` is dynamically adjusted when the variable count changes.

## 4. ViewModel Design

### 4.1 Properties

| Property | Type | Description |
|----------|------|-------------|
| `VariableCount` | `int` | Number of variables (2–5). Notify property changed to update UI. |
| `Equations` | `ObservableCollection<EquationRow>` | List of equation rows. |
| `ResultText` | `string` | Formatted solution text (displayed on success). |
| `ErrorMessage` | `string` | Error message (displayed on failure). |
| `IsBusy` | `bool` | Indicates solving in progress; disables buttons. |
| `HasError` | `bool` (derived) | True if `ErrorMessage` not empty (for UI visibility). |
| `IsValid` | `bool` (derived) | True if all entries are valid numbers. |

### 4.2 Commands

| Command | Action |
|---------|--------|
| `AddEquationCommand` | Adds a new `EquationRow` with default values (all zero). |
| `RemoveEquationCommand` | Removes the specified row. |
| `SolveCommand` | Validates input, constructs equation string, calls `IEquationSolver.Solve`, updates result/error. |

### 4.3 Validation

- Each coefficient and constant is validated by attempting `double.TryParse` with `CultureInfo.InvariantCulture`.
- Invalid fields are highlighted in the UI (e.g., red border). This can be achieved with a `Style` trigger based on a validation state.
- The “Solve” button is enabled only when `IsValid` is true and `IsBusy` is false.

### 4.4 Equation String Construction

Given a list of rows and variable count, produce a string like:

```
"2x1 + 3x2 = 5; x1 - x2 = 1"
```

Rules:
- For each row, build a list of terms: `$"{sign}{coeffPart}x{index+1}"` where `sign` is `+` or `-`, `coeffPart` is the absolute value formatted without trailing zeros, omitted if absolute value is 1.
- Join terms with `" "` (space). Trim leading `+`.
- Append `" = "` + constant (formatted).
- Join rows with `"; "`.

Use `CultureInfo.InvariantCulture` for number formatting.

### 4.5 Result/Error Display

- On success, set `ResultText` to multi‑line string: each variable on its own line, e.g., `"x1 = 2.0\nx2 = 1.0"`.
- On error, set `ErrorMessage` to the error code and message from `Result.Error`.
- Clear `ErrorMessage` when user starts editing (optional: on any property change of coefficients/constants).

## 5. UI Design (XAML)

### 5.1 Layout Structure

- **Variable count picker** – HorizontalStackLayout with Label and Picker.
- **Headers** – Grid with column definitions: for each variable a “x₁”, “x₂”, … label, plus “Const.” label.
- **Equations list** – `CollectionView` bound to `Equations`, with a `DataTemplate` containing a `Grid` with:
  - Coefficient entries: each an `Entry` bound to the corresponding string in `Coefficients` (using `BindableLayout` or a custom `ItemTemplateSelector`).
  - Constant entry.
  - Delete button (visible only if more than one equation?).
- **Add equation button** – Below the list.
- **Solve button** – Below the add button.
- **Error label** – Red text bound to `ErrorMessage`.
- **Result label** – Bold text bound to `ResultText`.

### 5.2 Dynamic Coefficient Entries

Because the number of coefficients per row changes with `VariableCount`, we can use a `BindableLayout` inside the row's `HorizontalStackLayout`:

```xml
<HorizontalStackLayout BindableLayout.ItemsSource="{Binding Coefficients}">
    <BindableLayout.ItemTemplate>
        <DataTemplate>
            <Entry Text="{Binding .}" 
                   Keyboard="Numeric" 
                   WidthRequest="60" 
                   Margin="2"/>
        </DataTemplate>
    </BindableLayout.ItemTemplate>
</HorizontalStackLayout>
```

This automatically creates one entry per coefficient.

### 5.3 Validation Highlighting

Add a visual cue for invalid entries. One approach: create a `Style` that sets the `BackgroundColor` to light red when the entry’s text is not a valid number. We can use a `Trigger` with a converter:

```xml
<Entry.Triggers>
    <DataTrigger TargetType="Entry" 
                 Binding="{Binding ., Converter={StaticResource IsValidDoubleConverter}}" 
                 Value="False">
        <Setter Property="BackgroundColor" Value="#FFDDDD" />
    </DataTrigger>
</Entry.Triggers>
```

The `IsValidDoubleConverter` returns `true` if the string can be parsed as a double.

### 5.4 Example XAML Skeleton

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Neo.Mobile.Converters"
             x:Class="Neo.Mobile.Views.MatrixBuilderPage"
             Title="Matrix Builder">
    <ContentPage.Resources>
        <local:IsValidDoubleConverter x:Key="IsValidDoubleConverter" />
    </ContentPage.Resources>

    <ScrollView>
        <VerticalStackLayout Spacing="10" Padding="10">
            <!-- Variable count picker -->
            <HorizontalStackLayout>
                <Label Text="Variables:" VerticalOptions="Center"/>
                <Picker x:Name="VariablePicker"
                        ItemsSource="{Binding VariableOptions}"
                        SelectedItem="{Binding VariableCount}"
                        WidthRequest="100"/>
            </HorizontalStackLayout>

            <!-- Headers -->
            <Grid x:Name="HeadersGrid" ColumnDefinitions="Auto,Auto,Auto,Auto,Auto">
                <!-- Dynamically generate based on VariableCount? Simpler: fixed max columns with visibility toggles. -->
                <!-- For simplicity, we can have a fixed set of columns and hide extras via Binding. -->
            </Grid>

            <!-- Equations -->
            <CollectionView ItemsSource="{Binding Equations}">
                <CollectionView.ItemTemplate>
                    <DataTemplate>
                        <Grid Margin="0,5">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto" />
                                <ColumnDefinition Width="Auto" />
                                <ColumnDefinition Width="Auto" />
                                <ColumnDefinition Width="Auto" />
                                <ColumnDefinition Width="Auto" />
                            </Grid.ColumnDefinitions>
                            <!-- Coefficient entries via BindableLayout inside a HorizontalStackLayout -->
                            <HorizontalStackLayout Grid.ColumnSpan="4" Spacing="5">
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
                                                </DataTrigger>
                                            </Entry.Triggers>
                                        </Entry>
                                    </DataTemplate>
                                </BindableLayout.ItemTemplate>
                            </HorizontalStackLayout>

                            <!-- Constant entry -->
                            <Entry Text="{Binding Constant}"
                                   Keyboard="Numeric"
                                   WidthRequest="60"
                                   Grid.Column="4">
                                <Entry.Triggers>
                                    <DataTrigger TargetType="Entry"
                                                 Binding="{Binding Constant, Converter={StaticResource IsValidDoubleConverter}}"
                                                 Value="False">
                                        <Setter Property="BackgroundColor" Value="#FFDDDD"/>
                                    </DataTrigger>
                                </Entry.Triggers>
                            </Entry>

                            <!-- Delete button -->
                            <Button Text="✖"
                                    Grid.Column="5"
                                    Command="{Binding Source={RelativeSource AncestorType={x:Type local:MatrixBuilderViewModel}}, Path=RemoveEquationCommand}"
                                    CommandParameter="{Binding .}"
                                    BackgroundColor="Transparent"
                                    TextColor="Red"/>
                        </Grid>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>

            <!-- Add equation button -->
            <Button Text="+ Add Equation" Command="{Binding AddEquationCommand}"/>

            <!-- Solve button -->
            <Button Text="Solve"
                    Command="{Binding SolveCommand}"
                    IsEnabled="{Binding IsValid, Converter={StaticResource InverseBoolConverter}}"/>

            <!-- Error message -->
            <Label Text="{Binding ErrorMessage}"
                   TextColor="Red"
                   IsVisible="{Binding HasError}"/>

            <!-- Result -->
            <Label Text="{Binding ResultText}"
                   FontAttributes="Bold"
                   LineBreakMode="WordWrap"/>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

## 6. Implementation Steps

1. **Create Converters** – `IsValidDoubleConverter` (returns `true` if input can be parsed as double). Also `InverseBoolConverter` if not already available.
2. **Define EquationRow** class with `ObservableCollection<string> Coefficients` and `string Constant`.
3. **Create MatrixBuilderViewModel**:
   - Implement `INotifyPropertyChanged` (use `CommunityToolkit.Mvvm`).
   - Properties: `VariableCount`, `Equations`, `ResultText`, `ErrorMessage`, `IsBusy`.
   - Commands: `AddEquationCommand`, `RemoveEquationCommand`, `SolveCommand`.
   - Helper methods: `ValidateAll()`, `BuildEquationString()`, `UpdateCoefficientCount()` (called when `VariableCount` changes).
4. **Implement Validation**:
   - In `ValidateAll()`, iterate through all rows and check each coefficient and constant with `double.TryParse`.
   - Set an internal dictionary of validity flags (or use per‑entry validation via converter).
   - Compute `IsValid` as a derived property.
5. **Implement SolveCommand**:
   - Set `IsBusy = true`, clear previous `ErrorMessage`.
   - Call `BuildEquationString()`.
   - Offload solver call to background thread using `Task.Run`.
   - On completion, update `ResultText` or `ErrorMessage` on the UI thread (use `MainThread.BeginInvokeOnMainThread` or rely on property change notifications).
   - Set `IsBusy = false`.
6. **Wire up UI** in XAML as described.
7. **Register ViewModel and Page** in `MauiProgram.cs` (transient).
8. **Test thoroughly** with various inputs, including edge cases (all zeros, negative numbers, floats, empty fields).

## 7. Edge Cases and Handling

- **Variable count change**: When user changes variable count, update each row’s `Coefficients` collection: add new entries with "0" or remove extras. Notify UI.
- **Empty equations**: Allow removing all equations? Better to keep at least one. Disable remove if count = 1.
- **Invalid entries**: Prevent solving if any entry invalid. Show red fields.
- **Floating point formatting**: Use `"G"` format to avoid unnecessary trailing zeros.
- **Large systems**: The grid may become tall; wrap in `ScrollView`.
- **Performance**: With up to 5 variables and ~10 equations, UI remains fast.

## 8. Future Extensions

- **Named variables**: Let user input variable names (e.g., `x`, `y`, `z`) via an editable field.
- **Load/save**: Persist equation sets locally.
- **Undo/redo**: Implement command stack.
- **Matrix input**: Alternative mode for entering full matrix.
- **Responsive design**: Adjust column widths on larger screens (tablets).

## 9. Integration with Existing Code

- Ensure `IEquationSolver` is registered in DI and injected into `MatrixBuilderViewModel`.
- Use the same `SolvingOptions` as the rest of the app (can be injected or use defaults).

---

This plan provides a clear, actionable guide for implementing a user‑friendly equation builder in the MAUI app. The AI agent can follow these steps to produce a functional, well‑integrated feature.