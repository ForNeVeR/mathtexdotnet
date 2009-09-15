using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public static class TexHelper
    {
        public static ExpressionTree CreateExpressionTree(string expression)
        {
            return ExpressionTree.FromParseTree(CreateParseTree(expression));
        }

        public static ParseTree CreateParseTree(string expression)
        {
            var lexer = new TexLexer();
            var parser = new TexParser();
            var tokenStream = lexer.Tokenise(expression);
            return parser.Parse(tokenStream);
        }
    }
}
