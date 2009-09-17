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

            const string testExpr = @"\frac{1}{3}+2^{-6+\alpha}*8-\sqrt[4]{\cos 3}/27+4-13*([4-2]*5)";

            var treeRenderer = new TreeTextRenderer(Console.Out);

            Console.WriteLine("Input:");
            Console.WriteLine(testExpr);
            Console.WriteLine();

            var tokenStream = TexHelper.CreateTokenStream(testExpr).ToTokenString();
            Console.WriteLine("Token stream:");
            Console.WriteLine(tokenStream);
            Console.WriteLine();

            var exprTree = TexHelper.CreateExpressionTree(testExpr);
            Console.WriteLine("Expression tree:");
            treeRenderer.Render(exprTree);
            Console.Out.WriteLine();

#if DEBUG
            Console.ReadKey(true);
#endif
        }
    }
}
