using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<Token>;

    public static class TexHelper
    {
        public static string CreateText(this ExpressionTree tree)
        {
            using (var stringWriter = new StringWriter())
            {
                var texWriter = new TexWriter(stringWriter);
                texWriter.Write(tree);
                return stringWriter.ToString();
            }
        }

        public static string CreateText(this ExpressionNode node)
        {
            using (var stringWriter = new StringWriter())
            {
                var texWriter = new TexWriter(stringWriter);
                texWriter.Write(node);
                return stringWriter.ToString();
            }
        }

        public static ExpressionTree CreateExpressionTree(string expression)
        {
            return ExpressionTreeBuilder.FromParseTree(CreateParseTree(expression));
        }

        public static ExpressionTree CreateExpressionTree(TokenStream tokenStream)
        {
            return ExpressionTreeBuilder.FromParseTree(CreateParseTree(tokenStream));
        }

        public static ParseTree CreateParseTree(string expression)
        {
            return CreateParseTree(CreateTokenStream(expression));
        }
        public static ParseTree CreateParseTree(TokenStream tokenStream)
        {
            var parser = new TexParser();
            return parser.Parse(tokenStream);
        }


        public static TokenStream CreateTokenStream(string expression)
        {
            var lexer = new TexLexer();
            return lexer.Tokenise(expression);
        }

        internal static void ForceMoveNext(this TokenStream tokenStream)
        {
            do
            {
                ForceMoveNextIncludeFormatting(tokenStream);
            } while (tokenStream.Current.Symbol.IsFormattingSymbol());
        }

        internal static void ForceMoveNextIncludeFormatting(this TokenStream tokenStream)
        {
            if (!tokenStream.MoveNext())
                throw new ParserException(Token.Null, "Unexpected end of token stream.");
        }

        internal static bool IsFormattingSymbol(this SymbolKind symbol)
        {
            switch (symbol)
            {
                case SymbolKind.Separator:
                case SymbolKind.Left:
                case SymbolKind.Right:
                    return true;
                default:
                    return false;
            }
        }
    }
}
