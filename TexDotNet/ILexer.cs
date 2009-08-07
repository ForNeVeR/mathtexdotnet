using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerable<Token>;

    public interface ILexer
    {
        TokenStream Tokenise(TextReader reader);
    }
}
