using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection.Metadata;

namespace NeoAndroidApp.Models;

/// <summary>
/// Represents a single equation row in the matrix builder.
/// Contains coefficients for each variable and a constant term.
/// </summary>
public partial class EquationRow : ObservableObject
{
    private readonly List<string> _variableNames = [ "x", "y", "z", "w", "v", "u" ];
    /// <summary>
    /// Gets the collection of coefficient values (as strings for two-way binding).
    /// Each entry corresponds to a variable (x1, x2, etc.).
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _coefficients = new();

    /// <summary>
    /// Gets or sets the constant term (as string for two-way binding).
    /// </summary>
    [ObservableProperty]
    private string _constant = string.Empty;

    /// <summary>
    /// Gets the collection of variable labels (x1, x2, etc.) for display.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _variableLabels = new();

    /// <summary>
    /// Gets the collection of coefficient-variable term items for display.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<EquationTermItem> _termItems = new();

    /// <summary>
    /// Initializes a new equation row with the specified number of variables.
    /// </summary>
    /// <param name="variableCount">The number of variables (coefficients) in this row.</param>
    public EquationRow(int variableCount)
    {
        for (int i = 0; i < variableCount; i++)
        {
            var variableName = _variableNames[i];
            var term = new EquationTermItem { VariableLabel = variableName, IsLast = i == variableCount - 1 };
            AddEquationTerm(term, variableName, i);
        }

        // Subscribe to collection changes to sync coefficients with term items and update IsLast
        Coefficients.CollectionChanged += (s, e) =>
        {
            // Update IsLast for all terms when collection changes
            UpdateTermIsLastFlags();

            if (e.NewItems != null && e.NewStartingIndex >= 0 && e.NewStartingIndex < TermItems.Count)
            {
                // Sync new coefficient to term item
                TermItems[e.NewStartingIndex].Coefficient = Coefficients[e.NewStartingIndex];
            }
        };
    }

    /// <summary>
    /// Handles property changes in term items to sync coefficient back to the collection.
    /// </summary>
    private void Term_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EquationTermItem.Coefficient) && sender is EquationTermItem term)
        {
            var index = TermItems.IndexOf(term);
            if (index >= 0 && index < Coefficients.Count)
            {
                Coefficients[index] = term.Coefficient;
            }
        }
    }

    /// <summary>
    /// Updates the IsLast flag for all term items.
    /// </summary>
    private void UpdateTermIsLastFlags()
    {
        for (int i = 0; i < TermItems.Count; i++)
        {
            TermItems[i].IsLast = (i == TermItems.Count - 1);
        }
        OnPropertyChanged(nameof(IsLastTerm));
    }

    /// <summary>
    /// Gets a value indicating whether there is only one term (for UI display).
    /// </summary>
    public bool IsLastTerm => TermItems.Count <= 1;

    /// <summary>
    /// Updates the coefficients collection when variable count changes.
    /// Adds new entries with empty strings or removes extra entries.
    /// </summary>
    /// <param name="newCount">The new variable count.</param>
    public void UpdateCoefficientCount(int newCount)
    {
        var currentCount = Coefficients.Count;

        if (newCount > currentCount)
        {
            // Add new coefficients with empty strings and variable labels
            for (int i = currentCount; i < newCount; i++)
            {
                var variableName = _variableNames[i];
                var newTerm = new EquationTermItem { VariableLabel = variableName, IsLast = false };
                AddEquationTerm(newTerm, variableName, i);
            }
            // Update last item
            if (TermItems.Count > 0)
                TermItems[TermItems.Count - 1].IsLast = true;
        }
        else if (newCount < currentCount)
        {
            // Remove extra coefficients and labels
            while (Coefficients.Count > newCount)
            {
                Coefficients.RemoveAt(Coefficients.Count - 1);
                VariableLabels.RemoveAt(VariableLabels.Count - 1);
                TermItems.RemoveAt(TermItems.Count - 1);
            }
            // Update last item
            if (TermItems.Count > 0)
                TermItems[TermItems.Count - 1].IsLast = true;
        }

        OnPropertyChanged(nameof(IsLastTerm));
    }

    /// <summary>
    /// Validates that all coefficients and the constant are valid numbers.
    /// Empty strings are treated as 0 for validation purposes.
    /// </summary>
    /// <returns>True if all values can be parsed as double or are empty; otherwise, false.</returns>
    public bool IsValid()
    {
        foreach (var coeff in Coefficients)
        {
            // Empty string is treated as 0 (valid)
            if (!string.IsNullOrEmpty(coeff) &&
                !double.TryParse(coeff, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                return false;
        }

        // Empty constant is treated as 0 (valid)
        return string.IsNullOrEmpty(Constant) ||
               double.TryParse(Constant, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }

    private void AddEquationTerm(EquationTermItem term, string variableName, int index)
    {
        term.PropertyChanged += Term_PropertyChanged;
        Coefficients.Add(string.Empty);
        VariableLabels.Add(variableName);
        TermItems.Add(term);
    }
}

/// <summary>
/// Represents a single term (coefficient + variable) in an equation row.
/// </summary>
public class EquationTermItem : ObservableObject
{
    private string _coefficient = string.Empty;
    private string _variableLabel = string.Empty;
    private bool _isLast;

    /// <summary>
    /// Gets or sets the coefficient value for this term.
    /// </summary>
    public string Coefficient
    {
        get => _coefficient;
        set => SetProperty(ref _coefficient, value);
    }

    /// <summary>
    /// Gets or sets the variable label (e.g., "x1", "x2") for this term.
    /// </summary>
    public string VariableLabel
    {
        get => _variableLabel;
        set => SetProperty(ref _variableLabel, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this is the last term in the equation.
    /// </summary>
    public bool IsLast
    {
        get => _isLast;
        set => SetProperty(ref _isLast, value);
    }
}