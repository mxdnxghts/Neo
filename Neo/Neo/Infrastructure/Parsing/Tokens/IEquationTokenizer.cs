using System;
using System.Collections.Generic;

namespace Neo.Infrastructure.Parsing.Tokens;

public interface IEquationTokenizer
{
    IEnumerable<EquationToken> Tokenize(ReadOnlySpan<char> input);

    IEnumerable<EquationToken> Tokenize(string input);
}