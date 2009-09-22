using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public static class TexHelper
    {
        public static string CreateText(this TexExpressionNode tree)
        {
            using (var stringWriter = new StringWriter())
            {
                var texWriter = new TexWriter(stringWriter);
                texWriter.Write(CreateTokenSream(tree));
                return stringWriter.ToString();
            }
        }

        public static TokenStream CreateTokenSream(this TexExpressionNode tree)
        {
            var texComposer = new TexComposer();
            return texComposer.Write(tree);
        }

        public static TexExpressionNode CreateExpressionTree(string expression)
        {
            return TexExpressionTreeBuilder.FromParseTree(CreateParseTree(expression));
        }

        public static TexExpressionNode CreateExpressionTree(TokenStream tokenStream)
        {
            return TexExpressionTreeBuilder.FromParseTree(CreateParseTree(tokenStream));
        }

        public static ParseNode CreateParseTree(string expression)
        {
            return CreateParseTree(CreateTokenStream(expression));
        }
        public static ParseNode CreateParseTree(TokenStream tokenStream)
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
                throw new ParserException(TexToken.Null, "Unexpected end of token stream.");
        }

        internal static bool IsFormattingSymbol(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.Separator:
                case TexSymbolKind.Left:
                case TexSymbolKind.Right:
                    return true;
                default:
                    return false;
            }
        }
    }
}
