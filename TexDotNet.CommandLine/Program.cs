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
            Console.WindowHeight = 50;
#endif

            const string testInput = @"xyz-{\int x^2+2y}+\frac{1}{3}+2^{{\log_7 -6.12}+\alpha}*8-\sqrt[4!]{\cos 3}/27+\text{foobar}-13/([4-2.2]*5)";
            
            var treeRenderer = new TreeTextRenderer(Console.Out);

            Console.WriteLine("Input:");
            Console.WriteLine(testInput);
            Console.WriteLine();

            var tokenStream = TexHelper.CreateTokenStream(testInput).ToTokenString();
            Console.WriteLine("Token stream:");
            Console.WriteLine(tokenStream);
            Console.WriteLine();

            var exprTree = TexHelper.CreateExpressionTree(testInput);
            Console.WriteLine("Expression tree:");
            treeRenderer.Render(exprTree);
            Console.Out.WriteLine();

            var recreatedInput = TexHelper.CreateText(exprTree);
            Console.WriteLine("Recreated input:");
            Console.WriteLine(recreatedInput);
            Console.WriteLine();

#if DEBUG
            Console.ReadKey(true);
#endif
        }
    }
}
