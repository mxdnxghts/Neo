using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using Neo.Services;

namespace Neo.Utilities;

/// <summary>
/// describe extension class for <see cref="Parser"/>
/// </summary>
public static class ParserExtension
{
    /// <summary>
    /// returns string with unknown variables from system linear equations
    /// </summary>
    /// <param name="input">parsed string (expected from <see cref="Matrix{T}"/>)</param>
    /// <returns></returns>
    public static string GetUnknownVariables(this string input, bool fullInput = false)
    {
        if (!fullInput)
            return new string(input.Separate().GetLongestString().Where(char.IsLetter).Distinct().ToArray());

        var digits = new string(input.Separate().GetLongestString().ToArray());
        return new string(digits.Where(char.IsLetter).Distinct().ToArray());
    }

    /// <summary>
    /// removes elements every time when i in cycle will divide without a trace by every' value
    /// after removes white spaces and empty strings from list
    /// </summary>
    /// <param name="input">parsed string (expected from <see cref="Matrix{T}"/>)</param>
    /// <param name="every">position to delete</param>
    /// <param name="rows">count of rows from string of parsed matrix</param>
    /// <returns></returns>
    public static List<string> RemoveEvery(this List<string> input, int every, int rows)
    {
        if (Parser.ValidArray(input.ToArray(), nameof(input)) is null)
            return null;
        for (var i = 1; i <= rows; i++)
            input.RemoveAt(every * i);

        return input.Where(s => !string.IsNullOrWhiteSpace(s)).AsEnumerable().ToList();
    }

    /// <summary>
    /// returns new <see cref="List{T}"/> with elements which were at point equals every' value
    /// </summary>
    /// <param name="input">parsed string (expected from <see cref="Matrix{T}"/>)</param>
    /// <param name="every">position to get</param>
    /// <param name="rows">count of rows from string of parsed matrix</param>
    /// <returns></returns>
    public static List<string> AddEvery(this List<string> input, int every, int rows)
    {
        if (Parser.ValidArray(input.ToArray(), nameof(input)) is null)
            return null;
        var output = Enumerable.Empty<string>();
        for (var i = 1; i <= rows; i++)
            output = output.Append(input[every * i - 1]);

        return output.Where(s => !string.IsNullOrWhiteSpace(s)).AsEnumerable().ToList();
    }

    /// <summary>
    /// check passed string of input it's trash input or not
    /// </summary>
    /// <param name="input">read text</param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsTrash(this string input)
    {
        if (!input.Any(item => "1234567890".Any(numeric => item == numeric)))
            return true;
        var len = input.Count(x => x == Parser.SplitSymbol);
        if (len is <= 0 or > 5)
            return true;
        return input.GetUnknownVariables().Length != input.ConvertToDigits().Count() / len - 1;
    }

    /// <summary>
    /// counts symbols in <see cref="input"/>
    /// </summary>
    /// <param name="input">read text</param>
    /// <param name="symbol">symbol for counting</param>
    /// <returns></returns>
    public static int SymbolCount(this string input, char symbol)
    {
        return input.Count(x => x == symbol);
    }

    /// <summary>
    /// appends zero coefficients to <see cref="input"/>
    /// </summary>
    /// <param name="input">parsed equations</param>
    /// <param name="unknownVariables">string of unknown variables of equations</param>
    /// <returns></returns>
    public static string AppendZeroCoefficients(this string input, string unknownVariables)
    {
        if (input.IsSymmetric())
            return input;
        var equations = input.Separate();
        var appendableVariables = equations.GetAppendableEquations(unknownVariables);
        var digitAppendableEquations = appendableVariables.Combine().GetDigits().Separate();


        var digitsEquations = equations.Combine().GetDigits().Separate();

        var sb = new StringBuilder();
        var index = 0;
        foreach (var equation in digitsEquations)
        {
            if (index >= digitAppendableEquations.Count)
            {
                sb.Append($"{equation}{Parser.SplitSymbol}");
                break;
            }

            if (equation != digitAppendableEquations[index])
            {
                sb.Append($"{equation}{Parser.SplitSymbol}");
                continue;
            }

            sb.AppendZeroCoefficientsEquation(digitAppendableEquations[index],
                appendableVariables.GetVariableNames(unknownVariables, index));
            index++;
        }


        return sb.ToString();
    }

    /// <summary>
    /// checks if the <see cref="str"/> contains something from <see cref="contains"/>
    /// </summary>
    /// <param name="str">where will finding</param>
    /// <param name="contains">wherefrom will finding</param>
    /// <returns></returns>
    public static bool ContainsString(this string str, string contains)
    {
        if (str is null || contains is null)
            return false;
        return contains.Any(str.Contains);
    }

    /// <summary>
    /// cleans <see cref="input"/> from whitespaces before <see cref="splitSymbol"/>
    /// in order to verify validation of <see cref="input"/> in the next operations of solving
    /// </summary>
    /// <param name="input">text</param>
    /// <param name="splitSymbol">symbol for splitting equations</param>
    /// <returns></returns>
    public static string RemoveWhiteSpacesNearSeparator(this string input, char splitSymbol = Parser.SplitSymbol)
    {
        var strings = input.Separate(splitSymbol);
        for (var i = 0; i < strings.Count; i++)
            strings[i] = strings[i].Trim();
        return strings.Combine();
    }

    /// <summary>
    /// gets as arg string with/without other symbols like words, punctuations marks etc
    /// return new <see cref="string"/> only with digits
    /// </summary>
    /// <param name="input">parsed string (expected from <see cref="Matrix{T}"/>)</param>
    /// <returns></returns>
    public static string GetDigits(this string input)
    {
        var sb = new StringBuilder();

        // validate input
        switch (input)
        {
            case null:
                Error.Message = $"{nameof(input)} is null.";
                return input;
            case "":
                Error.Message = $"Lengths of {nameof(input)} equals or less than 0.";
                return input;
        }


        for (var i = 0; i < input.Length; i++)
        {
            if (AppendNegativeSymbol(input, sb, i) || AppendFloatSymbol(input, sb, i))
                continue;

            if (AppendUnitVariable(input, sb, i))
                continue;

            // if input[i] neither is digit nor is split symbol ";" cycle 'll iterate
            if (!char.IsDigit(input[i]) && input[i] != Parser.SplitSymbol)
                continue;

            sb.Append(input[i]);

            // stop cycle if "i" more than length of input string
            if (i >= input.Length - 1)
                break;

            // if the next input element is neither ";", nor ",", nor ".", nor a digit
            // adds whitespace
            if (!char.IsDigit(input[i + 1]) && input[i + 1] != Parser.SplitSymbol
                                            && input[i + 1] != Parser.FloatSymbolDot
                                            && input[i + 1] != Parser.FloatSymbolComma
                                            && input[i + 1] != ' ')
            {
                sb.Append(' ');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// checks if the converted <see cref="input"/> to matrix has symmetric sides
    /// </summary>
    /// <param name="input">string of system linear equations</param>
    /// <returns></returns>
    private static bool IsSymmetric(this string input)
    {
        var parser = new Parser(input);
        var matrix = parser.MatrixConversion(input.GetUnknownVariables());
        return matrix.RowCount == matrix.ColumnCount;
    }

    /// <summary>
    /// appends zero coefficients to equation
    /// </summary>
    /// <param name="sb"><see cref="StringBuilder"/> in which will added value of return</param>
    /// <param name="digitEquationValue">char of string with converted equations strings to digit strings</param>
    /// <param name="missedVariables">variables of equation with zero coefficient</param>
    /// <returns></returns>
    private static StringBuilder AppendZeroCoefficientsEquation(this StringBuilder sb, string digitEquation,
        List<Dictionary<char, int>> missedVariables)
    {
        var index = 0;
        var current = 0;
        for (var j = 0; j < digitEquation.Length; j++)
        {
            sb.AppendZero(missedVariables, ref index, current);
            sb.AppendValue(digitEquation[j], ref current);

            if (j != digitEquation.Length - 1)
                continue;
            var line = sb.ToString().Trim();
            sb.Clear();
            sb.Append($"{line}{Parser.SplitSymbol}");
        }

        return sb;
    }

    /// <summary>
    /// appends to the <see cref="sb"/> value of <see cref="digitEquationValue"/>
    /// </summary>
    /// <param name="sb"><see cref="StringBuilder"/> in which will added value of return</param>
    /// <param name="digitEquationValue">char of string with converted equations strings to digit strings</param>
    /// <param name="current">index of variable from <see cref="missedVariables"/></param>
    /// <returns></returns>
    private static StringBuilder AppendValue(this StringBuilder sb, char digitEquationValue, ref int current)
    {
        sb.Append(digitEquationValue);
        current++;
        return sb;
    }

    /// <summary>
    /// appends zero to the <see cref="sb"/> if equation has zero coefficients
    /// </summary>
    /// <param name="sb"><see cref="StringBuilder"/> in which will added value of return</param>
    /// <param name="missedVariables">variables of equation with zero coefficient</param>
    /// <param name="index">index of <see cref="missedVariables"/> iteration</param>
    /// <param name="current">index of variable from <see cref="missedVariables"/></param>
    /// <returns></returns>
    private static StringBuilder AppendZero(this StringBuilder sb, List<Dictionary<char, int>> missedVariables,
        ref int index, int current)
    {
        // avoid IndexOutOfBoundsException
        if (index >= missedVariables.Count)
            return sb;
        if (missedVariables[index].Values.All(x => x != current))
            return sb;
        sb.Append(" 0 ");
        index++;
        return sb;
    }

    /// <summary>
    /// gets longest string of passed list
    /// </summary>
    /// <param name="list">list of strings</param>
    /// <returns></returns>
    private static string GetLongestString(this List<string> list)
    {
        return list.Aggregate((longest, next) =>
            next.Length > longest.Length ? next : longest);
    }

    /// <summary>
    /// gets indices of <see cref="unknownVariables"/>
    /// </summary>
    /// <param name="unknownVariables">all unknown variables of equations</param>
    /// <returns></returns>
    private static Dictionary<char, int> GetIndices(this string unknownVariables)
    {
        var indices = new Dictionary<char, int>();
        for (var i = 0; i < unknownVariables.Length; i++)
            indices.Add(unknownVariables[i], i);
        return indices;
    }

    /// <summary>
    /// gets list of dictionaries which contains name of unknown variable as key and its index as value
    /// </summary>
    /// <param name="equations">parsed string from input</param>
    /// <param name="unknownVariables">all unknown variables of equations</param>
    /// <param name="index">index of equations</param>
    /// <returns></returns>
    private static List<Dictionary<char, int>> GetVariableNames(this List<string> equations, string unknownVariables,
        int index)
    {
        var needToAppend = new List<Dictionary<char, int>>();
        var indices = unknownVariables.GetIndices();

        for (var i = 0; i < unknownVariables.Length; i++)
        {
            if (!equations[index].Contains(unknownVariables[i]))
                needToAppend.Add(new Dictionary<char, int>
                {
                    { unknownVariables[i], indices.Values.FirstOrDefault(x => x == i) }
                });
        }

        return needToAppend;
    }

    /// <summary>
    /// gets list of equations which needs in appending zero (zero coefficients)
    /// </summary>
    /// <param name="equations">parsed string from input</param>
    /// <param name="unknownVariables">all unknown variables of equations</param>
    /// <returns></returns>
    private static List<string> GetAppendableEquations(this List<string> equations, string unknownVariables)
    {
        var needToAppend = new List<string>();

        foreach (var equation in equations)
        {
            foreach (var unknownVariable in unknownVariables)
            {
                if (!equation.Contains(unknownVariable))
                    needToAppend.Add(equation);
            }
        }

        return needToAppend.Distinct().ToList();
    }

    /// <summary>
    /// combines <see cref="List{T}"/> in one string with <see cref="splitSymbol"/>
    /// </summary>
    /// <param name="list">list of <see cref="T"/></param>
    /// <param name="splitSymbol">symbol for splitting</param>
    /// <returns><see cref="string"/> with <see cref="splitSymbol"/> in indices where list was ended</returns>
    private static string Combine<T>(this List<T> list, char splitSymbol = Parser.SplitSymbol)
    {
        var sb = new StringBuilder();
        foreach (var equation in list)
            sb.Append($"{equation}{splitSymbol}");
        return sb.ToString();
    }

    /// <summary>
    /// separates <see cref="input"/> in <see cref="List{T}"/> using <see cref="splitSymbol"/>
    /// </summary>
    /// <param name="input">string for separating</param>
    /// <param name="splitSymbol">symbol for splitting</param>
    /// <returns><see cref="List{T}"/> from separated <see cref="input"/></returns>
    private static List<string> Separate(this string input, char splitSymbol = Parser.SplitSymbol)
    {
        var list = new List<string>();
        var sb = new StringBuilder();
        foreach (var item in input)
        {
            if (item != splitSymbol)
            {
                sb.Append(item);
            }
            else
            {
                list.Add(sb.ToString().Trim());
                sb.Clear();
            }
        }

        if (list.Count == 0)
            list.Add(sb.ToString().Trim());

        return list;
    }

    /// <summary>
    /// returns sequence of float digits which was parsed from <see cref="input"/>
    /// </summary>
    /// <param name="input">parsed string (expected from <see cref="Matrix{T}"/>)</param>
    /// <returns></returns>
    private static IEnumerable<double> ConvertToDigits(this string input)
    {
        input = input.GetDigits()
            .Replace(Parser.SplitSymbol.ToString(), " ")
            .Replace(Parser.NegativeSymbol.ToString(), "");

        var sb = new StringBuilder();
        var digits = new List<double>();

        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] == ' ')
                continue;
            sb.Append(input[i]);

            if (input[i + 1] != ' ')
                continue;

            digits.Add(double.Parse(sb.ToString()));
            sb.Clear();
        }

        return digits;
    }

    /// <summary>
    /// adds to <see cref="StringBuilder"/> negative symbols if they are
    /// </summary>
    /// <param name="input">parsed string (expected from <see cref="Matrix{T}"/>)</param>
    /// <param name="sb">used <see cref="StringBuilder"/></param>
    /// <param name="i">current index</param>
    /// <returns></returns>
    private static bool AppendNegativeSymbol(string input, StringBuilder sb, int i)
    {
        if (input[i] != Parser.NegativeSymbol)
            return false;

        for (var j = i; j < input.Length; j++)
        {
            if (!char.IsDigit(input[j]))
                continue;
            sb.Append(Parser.NegativeSymbol);
            break;
        }

        return true;
    }

    /// <summary>
    /// adds to <see cref="StringBuilder"/> float symbols if they are
    /// </summary>
    /// <param name="input">parsed string (expected from <see cref="Matrix{T}"/>)</param>
    /// <param name="sb">used <see cref="StringBuilder"/></param>
    /// <param name="i">current index</param>
    /// <returns></returns>
    private static bool AppendFloatSymbol(string input, StringBuilder sb, int i)
    {
        if (input[i] != Parser.FloatSymbolDot && input[i] != Parser.FloatSymbolComma)
            return false;

        sb.Append(Parser.FloatSymbolDot);

        return true;
    }

    /// <summary>
    /// adds to <see cref="StringBuilder"/> "1 " if there isn't coefficient of the nearest unknown variable 
    /// </summary>
    /// <param name="input">parsed string (expected from <see cref="Matrix{T}"/>)</param>
    /// <param name="sb">used <see cref="StringBuilder"/></param>
    /// <param name="i">current index</param>
    /// <returns></returns>
    private static bool AppendUnitVariable(string input, StringBuilder sb, int i)
    {
        for (var j = 0; j < input.GetUnknownVariables().Length; j++)
        {
            if (input[i] == input.GetUnknownVariables()[j] && i == 0)
            {
                sb.Append("1 ");
                return true;
            }

            if (input[i] != input.GetUnknownVariables()[j] || char.IsDigit(input[i - 1]))
                continue;
            sb.Append("1 ");
            return true;
        }

        return false;
    }
}