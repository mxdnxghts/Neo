using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Infrastructure.Parsing.Tokens;

public enum TokenType
{
    Number,
    Variable,
    Operator,
    Equals,
    Separator,
    Whitespace,
    End
}
