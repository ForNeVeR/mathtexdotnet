using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet.CommandLine
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
#if DEBUG
            Console.WindowHeight = 40;
#endif

            const string testExpr = @"1+2^{-6}*8-3/27+4-13*([4-2]*5)";

            var treeRenderer = new TreeTextRenderer(Console.Out);

            Console.WriteLine("Input:");
            Console.WriteLine(testExpr);
            Console.WriteLine();

            Console.WriteLine(TexHelper.CreateTokenStream(testExpr).ToTokenString());
            Console.WriteLine();

            var parseTree = TexHelper.CreateParseTree(testExpr);
            Console.WriteLine("Parse tree:");
            treeRenderer.Render(parseTree);
            Console.Out.WriteLine();

            var exprTree = ExpressionTree.FromParseTree(parseTree);
            Console.WriteLine("Expression tree:");
            treeRenderer.Render(exprTree);
            Console.Out.WriteLine();

#if DEBUG
            Console.ReadKey(true);
#endif
        }
    }
}
