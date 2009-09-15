using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<Token>;

    public static class DebugExtensions
    {
        public static string ToTokenString(this TokenStream stream)
        {
            return ToTokenString(stream.ToArray());
        }

        public static string ToTokenString(this IEnumerable<Token> stream)
        {
            return string.Join(" ", stream.Select(t => t.ToString()).ToArray());
        }
    }
}
