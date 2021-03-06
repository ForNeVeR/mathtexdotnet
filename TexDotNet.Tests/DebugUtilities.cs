﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syracuse.Common;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public static class DebugExtensions
    {
        public static string ToTokenString(this TokenStream stream)
        {
            return ToTokenString(stream.AsEnumerable().ToArray());
        }

        public static string ToTokenString(this IEnumerable<TexToken> stream)
        {
            return string.Join(" ", stream.Select(t => t.ToString()).ToArray());
        }
    }
}
