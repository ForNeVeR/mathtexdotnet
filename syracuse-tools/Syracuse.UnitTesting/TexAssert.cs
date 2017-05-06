using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TexDotNet;

namespace Syracuse.UnitTesting
{
    public static class TexAssert
    {
        public static void AreTreesEqual(TexExpressionNode a, TexExpressionNode b)
        {
            Assert.AreEqual(a.Symbol, b.Symbol);
            Assert.AreEqual(a.Value, b.Value);
            Assert.AreEqual(a.Children.Count, b.Children.Count);
            for (int i = 0; i < a.Children.Count; i++)
                AreTreesEqual(a.Children[i], b.Children[i]);
        }
    }
}
