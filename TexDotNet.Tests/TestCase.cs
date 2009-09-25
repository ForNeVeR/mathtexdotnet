using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet.Tests
{
    using TokenStream = IEnumerable<TexToken>;

    public class TestCase
    {
        public TestCase(string expressionText)
        {
            this.ExpressionText = expressionText;
        }

        public string ExpressionText
        {
            get;
            private set;
        }
    }
}
