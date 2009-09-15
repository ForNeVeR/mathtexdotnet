using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<Token>;

    public static class TexHelper
    {
        public static ExpressionTree CreateExpressionTree(string expression)
        {
            return ExpressionTree.FromParseTree(CreateParseTree(expression));
        }

        public static ParseTree CreateParseTree(string expression)
        {
            var parser = new TexParser();
            return parser.Parse(CreateTokenStream(expression));
        }

        public static TokenStream CreateTokenStream(string expression)
        {
            var lexer = new TexLexer();
            return lexer.Tokenise(expression);
        }
    }
}
