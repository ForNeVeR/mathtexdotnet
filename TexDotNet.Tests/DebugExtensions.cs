using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet.Tests
{
    using TokenStream = IEnumerable<Token>;

    public static class DebugExtensions
    {
        public static string ToTokenString(this TokenStream stream)
        {
            return string.Join(" ", stream.Select(t => t.ToString()).ToArray());
        }
    }
}
