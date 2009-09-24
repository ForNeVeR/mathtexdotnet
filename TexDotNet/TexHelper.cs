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

        public static bool IsBracket(this TexSymbolKind symbol)
        {
            return IsOpenBracket(symbol) || IsCloseBracket(symbol);
        }

        public static bool IsOpenBracket(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.GroupOpen:
                    return true;
                case TexSymbolKind.RoundBracketOpen:
                    return true;
                case TexSymbolKind.SquareBracketOpen:
                    return true;
                case TexSymbolKind.ModulusBracket:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsCloseBracket(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.GroupClose:
                    return true;
                case TexSymbolKind.RoundBracketClose:
                    return true;
                case TexSymbolKind.SquareBracketClose:
                    return true;
                case TexSymbolKind.ModulusBracket:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFormattingSymbol(this TexSymbolKind symbol)
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
