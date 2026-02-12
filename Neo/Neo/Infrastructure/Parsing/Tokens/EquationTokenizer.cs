using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Parsing.Tokens;

// Ref struct tokenizer that returns lightweight TokenInfo array
public ref struct EquationTokenizer
{
    private ReadOnlySpan<char> _input;
    private int _position;
    private TokenInfo[] _tokenBuffer;
    private int _tokenCount;

    // Rent from ArrayPool but we can't use static ArrayPool in ref struct
    // So we'll allocate a fixed-size buffer on the stack
    private Span<TokenInfo> _tokenSpan;

    public EquationTokenizer(ReadOnlySpan<char> input, Span<TokenInfo> buffer)
    {
        _input = input;
        _position = 0;
        _tokenSpan = buffer;
        _tokenCount = 0;
    }

    // Stack-allocated version for small inputs
    public EquationTokenizer(ReadOnlySpan<char> input)
    {
        _input = input;
        _position = 0;
        //_tokenSpan = stackalloc TokenInfo[64];
        _tokenCount = 0;
    }

    public ReadOnlySpan<TokenInfo> Tokenize()
    {
        while (_position < _input.Length && _tokenCount < _tokenSpan.Length)
        {
            var current = _input[_position];

            // Skip whitespace
            if (char.IsWhiteSpace(current))
            {
                SkipWhitespace();
                continue;
            }

            // Process based on character type
            if (IsDigit(current) || current == ParsingConstants.NegativeSymbol)
            {
                if (!ReadNumber())
                    break;
            }
            else if (IsVariableStart(current))
            {
                if (!ReadVariable())
                    break;
            }
            else if (current == ParsingConstants.PlusSymbol || current == ParsingConstants.NegativeSymbol)
            {
                if (!ReadOperator())
                    break;
            }
            else if (current == ParsingConstants.EqualsSymbol)
            {
                if (!ReadEquals())
                    break;
            }
            else if (current == ParsingConstants.SplitSymbol)
            {
                if (!ReadSeparator())
                    break;
            }
            else
            {
                // Unexpected character - skip
                _position++;
            }
        }

        // Add end token
        if (_tokenCount < _tokenSpan.Length)
        {
            _tokenSpan[_tokenCount++] = new TokenInfo(TokenType.End, _position, 0);
        }

        return _tokenSpan.Slice(0, _tokenCount);
    }

    private bool EnsureCapacity()
    {
        if (_tokenCount < _tokenSpan.Length)
            return true;

        // Out of space - can't resize in ref struct
        return false;
    }

    private void SkipWhitespace()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
        {
            _position++;
        }
    }

    private bool ReadNumber()
    {
        if (!EnsureCapacity())
            return false;

        var start = _position;

        // Handle negative sign
        if (_input[_position] == ParsingConstants.NegativeSymbol)
        {
            _position++;
        }

        // Read integer part
        while (_position < _input.Length && IsDigit(_input[_position]))
        {
            _position++;
        }

        // Read decimal part
        if (_position < _input.Length &&
            (_input[_position] == ParsingConstants.FloatSymbolDot ||
             _input[_position] == ParsingConstants.FloatSymbolComma))
        {
            _position++; // Skip decimal point

            // Read fractional part
            while (_position < _input.Length && IsDigit(_input[_position]))
            {
                _position++;
            }
        }

        var length = _position - start;
        _tokenSpan[_tokenCount++] = new TokenInfo(TokenType.Number, start, length);
        return true;
    }

    private bool ReadVariable()
    {
        if (!EnsureCapacity())
            return false;

        var start = _position;

        // Variables are single letters in the old system
        while (_position < _input.Length && IsVariableStart(_input[_position]))
        {
            _position++;
        }

        var length = _position - start;
        _tokenSpan[_tokenCount++] = new TokenInfo(TokenType.Variable, start, length);
        return true;
    }

    private bool ReadOperator()
    {
        if (!EnsureCapacity())
            return false;

        var start = _position;

        if (_input[_position] == ParsingConstants.PlusSymbol)
        {
            _position++;
            _tokenSpan[_tokenCount++] = new TokenInfo(TokenType.Operator, start, 1);
        }
        else
        {
            // Minus is handled as part of number
            _position++;
        }
        return true;
    }

    private bool ReadEquals()
    {
        if (!EnsureCapacity())
            return false;

        var start = _position;
        _position++;
        _tokenSpan[_tokenCount++] = new TokenInfo(TokenType.Equals, start, 1);
        return true;
    }

    private bool ReadSeparator()
    {
        if (!EnsureCapacity())
            return false;

        var start = _position;
        _position++;
        _tokenSpan[_tokenCount++] = new TokenInfo(TokenType.Separator, start, 1);
        return true;
    }

    private static bool IsDigit(char c) => c >= '0' && c <= '9';
    private static bool IsVariableStart(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
}
