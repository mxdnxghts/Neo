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
/// Handles user input, validation, and equation solving.
/// </summary>
public partial class MatrixBuilderViewModel : ObservableObject
{
    private readonly IEquationSolver _solver;
    private readonly List<string> _variableOptions = new() { "2", "3", "4", "5" };

    /// <summary>
    /// Gets the list of variable count options (2-5).
    /// </summary>
    public List<string> VariableOptions => _variableOptions;

    [ObservableProperty]
    private int _variableCount = 2;

    /// <summary>
    /// Gets or sets the variable count index (0-based index for the picker).
    /// Index 0 = 2 variables, Index 1 = 3 variables, etc.
    /// </summary>
    public int VariableCountIndex
    {
        get => VariableCount - 2;
        set
        {
            if (value >= 0 && value < _variableOptions.Count)
            {
                VariableCount = value + 2;
            }
        }
    }

    [ObservableProperty]
    private ObservableCollection<EquationRow> _equations = new();

    [ObservableProperty]
    private string _resultText = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Gets whether all equations are valid (all coefficients and constants are valid numbers).
    /// </summary>
    public bool IsValid => Equations.All(e => e.IsValid());

    /// <summary>
    /// Gets whether there is an error message to display.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Gets the variable labels for display (x1, x2, etc.).
    /// </summary>
    public List<string> VariableLabels =>
        Enumerable.Range(0, VariableCount).Select(i => $"x{i + 1}").ToList();

    /// <summary>
    /// Initializes a new instance of the MatrixBuilderViewModel.
    /// </summary>
    /// <param name="solver">The equation solver service.</param>
    public MatrixBuilderViewModel(IEquationSolver solver)
    {
        _solver = solver;
        InitializeEquations();
    }

    /// <summary>
    /// Initializes the equations collection with one default equation.
    /// </summary>
    private void InitializeEquations()
    {
        Equations.Clear();
        Equations.Add(new EquationRow(VariableCount));
        OnPropertyChanged(nameof(IsValid));
    }

    /// <summary>
    /// Updates all equation rows when the variable count changes.
    /// </summary>
    partial void OnVariableCountChanged(int value)
    {
        foreach (var equation in Equations)
            equation.UpdateCoefficientCount(value);

        OnPropertyChanged(nameof(VariableLabels));
        OnPropertyChanged(nameof(IsValid));
    }

    /// <summary>
    /// Adds a new equation row with default values.
    /// </summary>
    [RelayCommand]
    private void AddEquation()
    {
        Equations.Add(new EquationRow(VariableCount));
        OnPropertyChanged(nameof(IsValid));
        ClearError();
    }

    /// <summary>
    /// Removes the specified equation row.
    /// </summary>
    /// <param name="equation">The equation row to remove.</param>
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

    /// <summary>
    /// Determines if an equation can be removed (more than one equation exists).
    /// </summary>
    /// <param name="equation">The equation to check.</param>
    /// <returns>True if more than one equation exists.</returns>
    private bool CanRemoveEquation(EquationRow equation) => Equations.Count > 1;

    /// <summary>
    /// Validates all equations and returns true if all are valid.
    /// </summary>
    /// <returns>True if all equations are valid.</returns>
    private bool ValidateAll()
    {
        return Equations.All(e => e.IsValid());
    }

    /// <summary>
    /// Builds the equation string from the current input.
    /// </summary>
    /// <returns>The formatted equation string.</returns>
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

            var constant = equation.Constant;
            if (double.TryParse(constant, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                rows.Add($"{string.Join(" ", terms)} = {constant}");
            }
        }

        return string.Join("; ", rows);
    }

    /// <summary>
    /// Formats a single term (coefficient * variable).
    /// </summary>
    /// <param name="coeff">The coefficient value.</param>
    /// <param name="varIndex">The variable index (0-based).</param>
    /// <returns>The formatted term string.</returns>
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

    /// <summary>
    /// Solves the equation system.
    /// </summary>
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
            
            // Solve using Neo solver
            var result = await Task.Run(() => _solver.Solve(equationString));

            if (result.IsSuccess && result.Value != null)
            {
                DisplaySolution(result.Value);
            }
            else
            {
                DisplayError(result.Error?.Code ?? "UNKNOWN_ERROR", 
                           result.Error?.Message ?? "An unknown error occurred.");
            }
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

    /// <summary>
    /// Determines if the solve command can execute.
    /// </summary>
    /// <returns>True if not busy and at least one equation exists.</returns>
    private bool CanSolve() => !IsBusy && Equations.Count > 0;

    /// <summary>
    /// Displays the solution in the result text.
    /// </summary>
    /// <param name="solution">The solution to display.</param>
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

    /// <summary>
    /// Displays an error message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    private void DisplayError(string code, string message)
    {
        ErrorMessage = $"[{code}] {message}";
    }

    /// <summary>
    /// Clears the error message.
    /// </summary>
    private void ClearError()
    {
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Called when any equation property changes to revalidate.
    /// </summary>
    partial void OnEquationsChanged(ObservableCollection<EquationRow> value)
    {
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(CanSolve));
        ClearError();
    }
}
