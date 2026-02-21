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
    /// Each entry corresponds to a variable (x1, x2, etc.).
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
    /// <param name="variableCount">The number of variables (coefficients) in this row.</param>
    public EquationRow(int variableCount)
    {
        for (int i = 0; i < variableCount; i++)
            Coefficients.Add("0");
    }

    /// <summary>
    /// Updates the coefficients collection when variable count changes.
    /// Adds new entries with "0" or removes extra entries.
    /// </summary>
    /// <param name="newCount">The new variable count.</param>
    public void UpdateCoefficientCount(int newCount)
    {
        var currentCount = Coefficients.Count;
        
        if (newCount > currentCount)
        {
            // Add new coefficients
            for (int i = currentCount; i < newCount; i++)
                Coefficients.Add("0");
        }
        else if (newCount < currentCount)
        {
            // Remove extra coefficients
            while (Coefficients.Count > newCount)
                Coefficients.RemoveAt(Coefficients.Count - 1);
        }
    }

    /// <summary>
    /// Validates that all coefficients and the constant are valid numbers.
    /// </summary>
    /// <returns>True if all values can be parsed as double; otherwise, false.</returns>
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
