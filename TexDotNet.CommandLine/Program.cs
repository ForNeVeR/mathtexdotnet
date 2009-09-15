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

            const string testExpr = @"1+2*8-3/27+4-13*([4-2]*5)";
            var parseTree = TexHelper.CreateParseTree(testExpr);
            var treeRenderer = new TreeTextRenderer(Console.Out);
            treeRenderer.Render(parseTree);

#if DEBUG
            Console.ReadKey(true);
#endif
        }
    }
}
