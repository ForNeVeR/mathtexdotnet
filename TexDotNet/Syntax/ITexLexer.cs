using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public interface ILexer
    {
        TokenStream Tokenise(TextReader reader);
    }
}
