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
            var node = new ParseNode(ParseNodeKind.InfixOperator);
            node.Children.Add(ParseTerm(tokenStream));
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.EndOfStream:
                    return node;
                case SymbolKind.Plus:
                case SymbolKind.Minus:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    break;
                default:
                    return node;
            }
            node.Children.Add(ParseExpression(tokenStream));
            return node;
        }

        private ParseNode ParseTerm(TokenStream tokenStream)
        {
            var node = new ParseNode(ParseNodeKind.InfixOperator);
            node.Children.Add(ParseImplicitTermOptional(tokenStream));
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

        private ParseNode ParseImplicitTermOptional(TokenStream tokenStream)
        {
            var node = new ParseNode(ParseNodeKind.PostfixOperator);
            node.Children.Add(ParseSignedValue(tokenStream));
            ParseNode valueNode;
            while ((valueNode = ParseValueOptional(tokenStream)) != null)
                node.Children.Add(valueNode);
            node.Children.Add(new ParseNode(Token.FromKind(SymbolKind.Dot)));
            return node;
        }

        private ParseNode ParseSignedValue(TokenStream tokenStream)
        {
            var node = ParseSignedValueOptional(tokenStream);
            if (node == null)
                throw new ParserException(tokenStream.Current,
                    "Expected a value.");
            return node;
        }

        private ParseNode ParseSignedValueOptional(TokenStream tokenStream)
        {
            var node = new ParseNode(ParseNodeKind.PrefixOperator);
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Plus:
                case SymbolKind.Minus:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    break;
            }
            node.Children.Add(ParseFactorialValue(tokenStream));
            return node;
        }

        private ParseNode ParseFactorialValue(TokenStream tokenStream)
        {
            var node = new ParseNode(ParseNodeKind.PostfixOperator);
            node.Children.Add(ParseIndexedValue(tokenStream));
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Factorial:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    break;
            }
            return node;
        }

        private ParseNode ParseIndexedValue(TokenStream tokenStream)
        {
            var node = new ParseNode(ParseNodeKind.InfixOperator);
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
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    node.Children.Add(ParseIndex(tokenStream));
                    break;
            }
            return node;
        }

        private ParseNode ParseValue(TokenStream tokenStream)
        {
            var node = ParseValueOptional(tokenStream);
            if (node == null)
                throw new ParserException(tokenStream.Current,
                    "Expected one of the following: number, letter, open bracket, fraction, binomial, root, function.");
            return node;
        }

        private ParseNode ParseValueOptional(TokenStream tokenStream)
        {
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
                node = ParseFunctionOptional(tokenStream);
            if (node == null)
                node = ParseTextOptional(tokenStream);
            return node;
        }

        private ParseNode ParseRawValueOptional(TokenStream tokenStream)
        {
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Number:
                case SymbolKind.Letter:
                case SymbolKind.GreekLetter:
                    var node = new ParseNode(tokenStream.Current);
                    tokenStream.ForceMoveNext();
                    return node;
                default:
                    return null;
            }
        }

        private ParseNode ParseIndex(TokenStream tokenStream)
        {
            ParseNode node;
            node = ParseRawValueOptional(tokenStream);
            if (node == null)
                node = ParseGroupOptional(tokenStream);
            if (node == null)
                throw new ParserException(tokenStream.Current,
                    "Expected a single value or group expression.");
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
            var node = new ParseNode(ParseNodeKind.PrefixOperator, new[] {
                functionNode, ParseGroup(tokenStream), ParseGroup(tokenStream)});
            return node;
        }

        private ParseNode ParseBinomialOptional(TokenStream tokenStream)
        {
            if (tokenStream.Current.Symbol != SymbolKind.Binomial)
                return null;
            var functionNode = new ParseNode(tokenStream.Current);
            tokenStream.ForceMoveNext();
            var node = new ParseNode(ParseNodeKind.PrefixOperator, new[] {
                functionNode, ParseGroup(tokenStream), ParseGroup(tokenStream)});
            return node;
        }

        private ParseNode ParseRootOptional(TokenStream tokenStream)
        {
            if (tokenStream.Current.Symbol != SymbolKind.Root)
                return null;
            var functionNode = new ParseNode(tokenStream.Current);
            tokenStream.ForceMoveNext();
            var node = new ParseNode(ParseNodeKind.PrefixOperator);
            node.Children.Add(functionNode);
            var argumentNode = ParseArgumentOptional(tokenStream);
            if (argumentNode != null)
                node.Children.Add(argumentNode);
            node.Children.Add(ParseGroup(tokenStream));
            return node;
        }

        private ParseNode ParseFunctionOptional(TokenStream tokenStream)
        {
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Minimum:
                case SymbolKind.Maximum:
                case SymbolKind.GreatestCommonDenominator:
                case SymbolKind.LowestCommonMultiple:
                case SymbolKind.Exponent:
                case SymbolKind.Log:
                case SymbolKind.NaturalLog:
                case SymbolKind.Argument:
                case SymbolKind.Limit:
                case SymbolKind.LimitInferior:
                case SymbolKind.LimitSuperior:
                case SymbolKind.Sine:
                case SymbolKind.Cosine:
                case SymbolKind.Tangent:
                case SymbolKind.Cosecant:
                case SymbolKind.Secant:
                case SymbolKind.Cotangent:
                case SymbolKind.ArcSine:
                case SymbolKind.ArcCosine:
                case SymbolKind.ArcTangent:
                case SymbolKind.ArcCosecant:
                case SymbolKind.ArcSecant:
                case SymbolKind.ArcCotangent:
                    var functionNode = new ParseNode(tokenStream.Current);
                    tokenStream.ForceMoveNext();
                    return new ParseNode(ParseNodeKind.PrefixOperator, new[] {
                        functionNode, ParseExpression(tokenStream) });
                default:
                    return null;
            }
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

        private ParseNode ParseTextOptional(TokenStream tokenStream)
        {
            if (tokenStream.Current.Symbol != SymbolKind.Text)
                return null;
            tokenStream.ForceMoveNext();
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.GroupOpen:
                    tokenStream.ForceMoveNext();
                    var sb = new StringBuilder();
                    while (tokenStream.Current.Symbol != SymbolKind.GroupClose)
                    {
                        sb.Append((char)tokenStream.Current.Value);
                        tokenStream.ForceMoveNext();
                    }
                    if (sb.Length == 0)
                        throw new ParserException(tokenStream.Current,
                            "A text value must contain at least one character.");
                    tokenStream.ForceMoveNext();
                    return new ParseNode(Token.FromValue(SymbolKind.Text, sb.ToString()));
                default:
                    throw new ParserException(tokenStream.Current, new[] {
                        SymbolKind.GroupOpen});
            }
        }
    }
}
