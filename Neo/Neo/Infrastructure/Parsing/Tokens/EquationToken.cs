using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Parsing.Tokens;

// Lightweight token info (only indices, no strings)
public readonly struct TokenInfo
{
    public TokenType Type { get; }
    public int Start { get; }
    public int Length { get; }

    public TokenInfo(TokenType type, int start, int length)
    {
        Type = type;
        Start = start;
        Length = length;
    }
}

// Full token with value (allocated when needed)
public readonly struct EquationToken
{
    public TokenType Type { get; }
    public ReadOnlyMemory<char> Value { get; }
    public int Position { get; }

    public EquationToken(TokenType type, ReadOnlyMemory<char> value, int position)
    {
        Type = type;
        Value = value;
        Position = position;
    }

    public static EquationToken FromInfo(TokenInfo info, ReadOnlyMemory<char> source)
    {
        var value = source.Slice(info.Start, info.Length);
        return new EquationToken(info.Type, value, info.Start);
    }
}