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
            tokenStream.ForceMoveNext();
            var rootNode = ParseExpression(tokenStream);
            if (tokenStream.Current.Symbol != SymbolKind.EndOfStream)
                throw new ParserException(tokenStream.Current,
                    "Expected end of token stream.");
            return new ParseTree(rootNode);
        }

        private ParseNode ParseExpression(TokenStream tokenStream)
        {
            var node = new ParseNode(ParseNodeKind.Operation);
            node.Children.Add(ParseTerm(tokenStream));
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Plus:
                case SymbolKind.Minus:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    break;
                case SymbolKind.EndOfStream:
                    return node;
                default:
                    return node;
            }
            node.Children.Add(ParseExpression(tokenStream));
            return node;
        }

        private ParseNode ParseTerm(TokenStream tokenStream)
        {
            var node = new ParseNode(ParseNodeKind.Operation);
            node.Children.Add(ParseIndexedValue(tokenStream));
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Cross:
                case SymbolKind.Dot:
                case SymbolKind.Star:
                case SymbolKind.Divide:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    node.Children.Add(ParseTerm(tokenStream));
                    break;
            }
            return node;
        }

        private ParseNode ParseIndexedValue(TokenStream tokenStream)
        {
            var node = new ParseNode(ParseNodeKind.Operation);
            node.Children.Add(ParseValue(tokenStream));
            var firstSymbol = tokenStream.Current.Symbol;
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.RaiseToIndex:
                case SymbolKind.LowerToIndex:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    node.Children.Add(ParseIndex(tokenStream));
                    break;
                default:
                    return node;
            }
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.RaiseToIndex:
                case SymbolKind.LowerToIndex:
                    if (tokenStream.Current.Symbol == firstSymbol)
                    {
                        throw new ParserException(tokenStream.Current, new[] {
                            firstSymbol == SymbolKind.RaiseToIndex ? SymbolKind.LowerToIndex : 
                            SymbolKind.RaiseToIndex});
                    }
                    else
                    {
                        node.Children.Add(new ParseNode(tokenStream.Current));
                        tokenStream.ForceMoveNext();
                        node.Children.Add(ParseIndex(tokenStream));
                        return node;
                    }
                default:
                    return node;
            }
        }

        private ParseNode ParseIndex(TokenStream tokenStream)
        {
            ParseNode node;
            node = ParseRawValueOptional(tokenStream);
            if (node == null)
                node = ParseGroupOptional(tokenStream);
            if (node == null)
                throw new ParserException(tokenStream.Current, "Expected a single value or group expression.");
            return node;
        }

        private ParseNode ParseGroup(TokenStream tokenStream)
        {
            var node = ParseGroupOptional(tokenStream);
            if (node == null)
                throw new ParserException(tokenStream.Current, new[] {
                    SymbolKind.GroupOpen });
            return node;
        }

        private ParseNode ParseGroupOptional(TokenStream tokenStream)
        {
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.GroupOpen:
                    tokenStream.ForceMoveNext();
                    var node = ParseExpression(tokenStream);
                    if (tokenStream.Current.Symbol != SymbolKind.GroupClose)
                        throw new ParserException(tokenStream.Current, new[] {
                            SymbolKind.GroupClose });
                    tokenStream.ForceMoveNext();
                    return node;
                default:
                    return null;
            }
        }

        private ParseNode ParseValue(TokenStream tokenStream)
        {
            bool hasModifier = false;
            var modifierToken = tokenStream.Current;
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Plus:
                    tokenStream.ForceMoveNext();
                    break;
                case SymbolKind.Minus:
                    hasModifier = true;
                    tokenStream.ForceMoveNext();
                    break;
            }

            ParseNode node;
            node = ParseRawValueOptional(tokenStream);
            if (node == null)
                node = ParseGroupOptional(tokenStream);
            if (node == null)
                node = ParseBracketedExpressionOptional(tokenStream);
            if (node == null)
                node = ParseFractionOptional(tokenStream);
            if (node == null)
                node = ParseBinomialOptional(tokenStream);
            if (node == null)
                node = ParseRootOptional(tokenStream);
            if (node == null)
                throw new ParserException(tokenStream.Current, new[] {
                    SymbolKind.Number, SymbolKind.Letter });

            if (hasModifier)
                return new ParseNode(ParseNodeKind.FunctionCall, new[] { new ParseNode(modifierToken), node });
            else
                return node;
        }

        //private ParseNode ParseRawValue(TokenStream tokenStream)
        //{
        //    var node = ParseRawValueOptional(tokenStream);
        //    if (node == null)
        //        throw new ParserException(tokenStream.Current, new[] {
        //            SymbolKind.Number, SymbolKind.Letter });
        //    return node;
        //}

        private ParseNode ParseRawValueOptional(TokenStream tokenStream)
        {
            ParseNode node;
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Number:
                case SymbolKind.Letter:
                case SymbolKind.GreekLetter:
                    node = new ParseNode(tokenStream.Current);
                    tokenStream.ForceMoveNext();
                    break;
                default:
                    return null;
            }
            return node;
        }

        //private ParseNode ParseBracketedExpression(TokenStream tokenStream)
        //{
        //    var node = ParseBracketedExpressionOptional(tokenStream);
        //    if (node == null)
        //        throw new ParserException(tokenStream.Current, new[] {
        //            SymbolKind.RoundBracketOpen, SymbolKind.SquareBracketOpen, SymbolKind.CurlyBracketOpen });
        //    return node;
        //}

        private ParseNode ParseBracketedExpressionOptional(TokenStream tokenStream)
        {
            SymbolKind bracketCloseToken;
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.RoundBracketOpen:
                    bracketCloseToken = SymbolKind.RoundBracketClose;
                    break;
                case SymbolKind.SquareBracketOpen:
                    bracketCloseToken = SymbolKind.SquareBracketClose;
                    break;
                case SymbolKind.CurlyBracketOpen:
                    bracketCloseToken = SymbolKind.CurlyBracketClose;
                    break;
                case SymbolKind.AngleBracketOpen:
                    bracketCloseToken = SymbolKind.AngleBracketClose;
                    break;
                case SymbolKind.FloorBracketOpen:
                    bracketCloseToken = SymbolKind.FloorBracketClose;
                    break;
                case SymbolKind.CeilingBracketOpen:
                    bracketCloseToken = SymbolKind.CeilingBracketClose;
                    break;
                case SymbolKind.NormBracket:
                    bracketCloseToken = SymbolKind.NormBracket;
                    break;
                default:
                    return null;
            }
            tokenStream.ForceMoveNext();
            var node = ParseExpression(tokenStream);
            if (tokenStream.Current.Symbol != bracketCloseToken)
                throw new ParserException(tokenStream.Current, new[] {
                    bracketCloseToken });
            tokenStream.ForceMoveNext();
            return node;
        }

        private ParseNode ParseFractionOptional(TokenStream tokenStream)
        {
            if (tokenStream.Current.Symbol != SymbolKind.Fraction)
                return null;
            var functionNode = new ParseNode(tokenStream.Current);
            tokenStream.ForceMoveNext();
            var node = new ParseNode(ParseNodeKind.FunctionCall, new[] {
                functionNode, ParseGroup(tokenStream), ParseGroup(tokenStream)});
            return node;
        }

        private ParseNode ParseBinomialOptional(TokenStream tokenStream)
        {
            if (tokenStream.Current.Symbol != SymbolKind.Binomial)
                return null;
            var functionNode = new ParseNode(tokenStream.Current);
            tokenStream.ForceMoveNext();
            var node = new ParseNode(ParseNodeKind.FunctionCall, new[] {
                functionNode, ParseGroup(tokenStream), ParseGroup(tokenStream)});
            return node;
        }

        private ParseNode ParseRootOptional(TokenStream tokenStream)
        {
            if (tokenStream.Current.Symbol != SymbolKind.Root)
                return null;
            var functionNode = new ParseNode(tokenStream.Current);
            tokenStream.ForceMoveNext();
            var node = new ParseNode(ParseNodeKind.FunctionCall);
            node.Children.Add(functionNode);
            var argumentNode = ParseArgumentOptional(tokenStream);
            if (argumentNode != null)
                node.Children.Add(argumentNode);
            node.Children.Add(ParseGroup(tokenStream));
            return node;
        }

        private ParseNode ParseArgumentOptional(TokenStream tokenStream)
        {
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.SquareBracketOpen:
                    tokenStream.ForceMoveNext();
                    var node = ParseExpression(tokenStream);
                    if (tokenStream.Current.Symbol != SymbolKind.SquareBracketClose)
                        throw new ParserException(tokenStream.Current, new[] {
                            SymbolKind.SquareBracketClose });
                    tokenStream.ForceMoveNext();
                    node.IsArgument = true;
                    return node;
                default:
                    return null;
            }
        }
    }
}
