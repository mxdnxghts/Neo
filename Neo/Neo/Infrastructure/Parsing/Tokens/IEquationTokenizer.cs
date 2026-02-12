using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Parsing.Tokens;

public interface IEquationTokenizer
{
    IEnumerable<EquationToken> Tokenize(ReadOnlySpan<char> input);
    IEnumerable<EquationToken> Tokenize(string input);
}
