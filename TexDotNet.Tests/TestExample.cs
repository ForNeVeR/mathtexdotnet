using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TexDotNet.Tests
{
    using TokenStream = IEnumerable<TexToken>;

    public class TestExample
    {
        public TestExample(string text, TexToken[] expectedTokens)
        {
            this.Text = text;
            this.ExpectedTokens = expectedTokens;
        }

        public string Text
        {
            get;
            private set;
        }

        public TexToken[] Tokens
        {
            get;
            private set;
        }

        public TexToken[] ExpectedTokens
        {
            get;
            private set;
        }

        public void Initialise(TexLexer lexer)
        {
            using (var reader = new StringReader(this.Text))
                this.Tokens = lexer.Tokenise(reader).ToArray();
        }

        public void TestLexer()
        {
            CollectionAssert.AreEqual(this.ExpectedTokens, this.Tokens);
        }
    }
}
