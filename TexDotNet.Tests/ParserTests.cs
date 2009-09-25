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
    using TokenStream = IEnumerable<TexToken>;

    [TestClass]
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

        class MyType
        {
        }

        class MyObj
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

        private void TestGroup(string groupName)
        {
            var group = testCaseSet[groupName];
            foreach (var testCase in group)
            {
                try
                {
                    var text = testCase.ExpressionText;
                    var exprTree = TexHelper.CreateExpressionTree(text);
                    var recreatedText = TexHelper.CreateText(exprTree);
                    var recreatedExprTree = TexHelper.CreateExpressionTree(recreatedText);
                    Assert.AreEqual(exprTree, recreatedExprTree);
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
    }
}
