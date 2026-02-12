using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Parsing;

public static class ParsingConstants
{
    public const char SplitSymbol = ';';
    public const char NegativeSymbol = '-';
    public const char FloatSymbolDot = '.';
    public const char FloatSymbolComma = ',';
    public const char PlusSymbol = '+';
    public const char EqualsSymbol = '=';

    // Optimized character sets
    public static readonly HashSet<char> Operators = new() { '+', '-', '=' };
    public static readonly HashSet<char> Digits = new() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
    public static readonly HashSet<char> VariableStartChars = new()
    {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
        'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
        'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
    };
}
