using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TexDotNet.Tests
{
    using TokenStream = IEnumerable<Token>;

    [TestClass]
    public class LexerTest
    {
        private static TestExample[] examples;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            using (var examplesReader = new TestExamplesReader(SystemHelper.GetResourceStream("Examples.txt")))
                LexerTest.examples = examplesReader.ReadAllExamples().ToArray();
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
        }

        private TexLexer lexer;
        
        public LexerTest()
        {
            lexer = new TexLexer();
        }

        public TestContext TestContext
        {
            get;
            set;
        }

        [TestInitialize()]
        public void TestInitialize()
        {
        }

        [TestCleanup()]
        public void TestCleanup()
        {
        }

        [TestMethod()]
        public void LexerExamplesTest()
        {
            foreach (var example in examples)
            {
                example.Initialise(lexer);

                Trace.WriteLine("Token string:");
                Trace.WriteLine(example.Text);
                Trace.WriteLine("Lexed token stream:");
                Trace.WriteLine(example.Tokens.ToTokenString());
                Trace.WriteLine("Expected token stream:");
                Trace.WriteLine(example.ExpectedTokens.ToTokenString());
                Trace.WriteLine(null);

                //example.TestLexer();
            }
        }
    }
}
