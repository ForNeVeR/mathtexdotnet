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
    public class ParserTests
    {
        public ParserTests()
        {
        }

        public TestContext TestContext
        {
            get;
            set;
        }

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
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
        public void QuickParserTest()
        {
            TexHelper.CreateParseTree("");
        }
    }
}
