using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerable<Token>;

    public interface IParser
    {
        ParseTree Parse(TokenStream tokenStream);
    }
}
