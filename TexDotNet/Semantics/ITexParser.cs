using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public interface IParser
    {
        ParseNode Parse(TokenStream tokenStream);
    }
}
