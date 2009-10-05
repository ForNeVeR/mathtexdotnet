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
    [TestClass()]
    public class ParserTests
    {
        private static TestCaseSet testCaseSet;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            testCaseSet = TestCaseSet.FromStream(SystemHelper.GetResourceStream("Input.TestCases1.txt"));
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
        }

        public ParserTests()
        {
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
        public void TestParserBasic()
        {
            TestGroup("Basic");
        }

        [TestMethod()]
        public void TestParserText()
        {
            TestGroup("Text");
        }

        [TestMethod()]
        public void TestParserGroups()
        {
            TestGroup("Groups");
        }

        [TestMethod()]
        public void TestParserBrackets()
        {
            TestGroup("Brackets");
        }

        [TestMethod()]
        public void TestParserBracketedFunctions()
        {
            TestGroup("Bracketed Functions");
        }

        [TestMethod()]
        public void TestParserFunctions()
        {
            TestGroup("Functions");
        }

        [TestMethod()]
        public void TestParserBigOperators()
        {
            TestGroup("Big Operators");
        }

        private void TestGroup(string groupName)
        {
            var group = testCaseSet[groupName];
            foreach (var testCase in group)
            {
                try
                {
                    var text = testCase.ExpressionText;
                    Trace.WriteLine("Original text: " + text);
                    var exprTree = TexHelper.CreateExpressionTree(text);
                    Trace.WriteLine("Original expression tree:");
                    Trace.Write(TreeTextRenderer.GetText(exprTree));
                    var recreatedText = TexHelper.CreateText(exprTree);
                    Trace.WriteLine("Recreated text: " + recreatedText);
                    var recreatedExprTree = TexHelper.CreateExpressionTree(recreatedText);
                    Trace.WriteLine("Recreated expression tree:");
                    Trace.Write(TreeTextRenderer.GetText(recreatedExprTree));
                    AssertAreTreesEqual(exprTree, recreatedExprTree);
                    Trace.WriteLine(string.Empty);
                }
                catch (LexerException exLexer)
                {
                    Assert.Fail(string.Format("Lexer error. {0}", exLexer.Message));
                }
                catch (ParserException exParser)
                {
                    Assert.Fail(string.Format("Parser error. {0}", exParser.Message));
                }
            }
        }

        public void AssertAreTreesEqual(TexExpressionNode a, TexExpressionNode b)
        {
            Assert.AreEqual(a.Symbol, b.Symbol);
            Assert.AreEqual(a.Value, b.Value);
            Assert.AreEqual(a.Children.Count, b.Children.Count);
            for (int i = 0; i < a.Children.Count; i++)
                AssertAreTreesEqual(a.Children[i], b.Children[i]);
        }
    }
}
