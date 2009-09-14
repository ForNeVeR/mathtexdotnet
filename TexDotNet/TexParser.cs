using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<Token>;

    public class TexParser : IParser
    {
        public TexParser()
        {
        }

        public ParseTree Parse(TokenStream tokenStream)
        {
            tokenStream.MoveNext();
            var rootNode = ParseExpression(tokenStream);
            if (tokenStream.Current.Kind != TokenKind.EndOfStream)
                throw new ParserException(tokenStream.Current,
                    "Expected end of token stream.");
            return new ParseTree(rootNode);
        }

        private ParseNode ParseExpression(TokenStream tokenStream)
        {
            var node = new ParseNode(ParseNodeKind.Expression);
            node.Children.Add(ParseTerm(tokenStream));
            switch (tokenStream.Current.Kind)
            {
                case TokenKind.Plus:
                case TokenKind.Minus:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.MoveNext();
                    break;
                case TokenKind.EndOfStream:
                    return node;
                default:
                    return node;
            }
            node.Children.Add(ParseExpression(tokenStream));
            return node;
        }

        private ParseNode ParseTerm(TokenStream tokenStream)
        {
            var node = new ParseNode(ParseNodeKind.Term);
            node.Children.Add(ParseValue(tokenStream));
            switch (tokenStream.Current.Kind)
            {
                case TokenKind.RaiseToIndex:
                case TokenKind.LowerToIndex:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.MoveNext();
                    node.Children.Add(ParseIndex(tokenStream));
                    break;
            }
            switch (tokenStream.Current.Kind)
            {
                case TokenKind.Cross:
                case TokenKind.Dot:
                case TokenKind.Divide:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.MoveNext();
                    node.Children.Add(ParseTerm(tokenStream));
                    break;
            }
            return node;
        }

        private ParseNode ParseIndex(TokenStream tokenStream)
        {
            switch (tokenStream.Current.Kind)
            {
                case TokenKind.GroupOpen:
                    var node = ParseExpression(tokenStream);
                    if (tokenStream.Current.Kind != TokenKind.GroupClose)
                        throw new ParserException(tokenStream.Current, new[] {
                            TokenKind.GroupClose });
                    return node;
                default:
                    return ParseValue(tokenStream);
            }
        }

        private ParseNode ParseValue(TokenStream tokenStream)
        {
            ParseNode node;
            switch (tokenStream.Current.Kind)
            {
                case TokenKind.Number:
                case TokenKind.Letter:
                    node = new ParseNode(tokenStream.Current);
                    tokenStream.MoveNext();
                    break;
                default:
                    node = ParseBracketedExpression(tokenStream, true);
                    if (node != null)
                        break;
                    throw new ParserException(tokenStream.Current, new[] {
                        TokenKind.Number, TokenKind.Letter });
            }
            return node;
        }

        private ParseNode ParseBracketedExpression(TokenStream tokenStream, bool ignoreFailure)
        {
            TokenKind bracketCloseToken;
            switch (tokenStream.Current.Kind)
            {
                case TokenKind.RoundBracketOpen:
                    bracketCloseToken = TokenKind.RoundBracketClose;
                    break;
                case TokenKind.SquareBracketOpen:
                    bracketCloseToken = TokenKind.SquareBracketClose;
                    break;
                case TokenKind.CurlyBracketOpen:
                    bracketCloseToken = TokenKind.CurlyBracketClose;
                    break;
                default:
                    if (ignoreFailure)
                        return null;
                    throw new ParserException(tokenStream.Current, new[] {
                        TokenKind.RoundBracketOpen, TokenKind.SquareBracketOpen, TokenKind.CurlyBracketOpen });
            }
            tokenStream.MoveNext();
            var node = ParseExpression(tokenStream);
            if (tokenStream.Current.Kind != bracketCloseToken)
                throw new ParserException(tokenStream.Current, new[] {
                    bracketCloseToken });
            tokenStream.MoveNext();
            return node;
        }
    }
}
