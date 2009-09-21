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
            var state = CreateDefaultState();
            tokenStream.ForceMoveNext();
            var rootNode = ParseExpression(tokenStream, ref state);
            if (tokenStream.Current.Symbol != SymbolKind.EndOfStream)
                throw new ParserException(tokenStream.Current,
                    "Expected end of token stream.");
            return new ParseTree(rootNode);
        }

        private ParserState CreateDefaultState()
        {
            var state = new ParserState();
            state.IsModulusBracketOpen = false;
            state.IsNormBracketOpen = false;
            return state;
        }

        private ParseNode ParseExpression(TokenStream tokenStream, ref ParserState state)
        {
            var node = new ParseNode(ParseNodeKind.InfixOperator);
            node.Children.Add(ParseTerm(tokenStream, ref state));
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
            node.Children.Add(ParseExpression(tokenStream, ref state));
            return node;
        }

        private ParseNode ParseTerm(TokenStream tokenStream, ref ParserState state)
        {
            var node = new ParseNode(ParseNodeKind.InfixOperator);
            node.Children.Add(ParseFirstImplicitTermOptional(tokenStream, ref state));
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Cross:
                case SymbolKind.Dot:
                case SymbolKind.Star:
                case SymbolKind.Divide:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    node.Children.Add(ParseTerm(tokenStream, ref state));
                    break;
            }
            return node;
        }

        private ParseNode ParseFirstImplicitTermOptional(TokenStream tokenStream, ref ParserState state)
        {
            var node = new ParseNode(ParseNodeKind.PrefixOperator);
            node.Children.Add(new ParseNode(Token.FromKind(SymbolKind.Dot, tokenStream.Current.Position)));
            node.Children.Add(ParseSignedValue(tokenStream, ref state));
            var implicitTermNode = ParseImplicitTermOptional(tokenStream, ref state);
            if (implicitTermNode != null)
                node.Children.Add(implicitTermNode);
            return node;
        }

        private ParseNode ParseImplicitTermOptional(TokenStream tokenStream, ref ParserState state)
        {
            var valueNode = ParseValueOptional(tokenStream, ref state);
            if (valueNode == null)
                return null;
            var node = new ParseNode(ParseNodeKind.PrefixOperator);
            node.Children.Add(new ParseNode(Token.FromKind(SymbolKind.Dot, tokenStream.Current.Position)));
            node.Children.Add(valueNode);
            var implicitTermNode = ParseImplicitTermOptional(tokenStream, ref state);
            if (implicitTermNode != null)
                node.Children.Add(implicitTermNode);
            return node;
        }

        private ParseNode ParseSignedValue(TokenStream tokenStream, ref ParserState state)
        {
            var node = ParseSignedValueOptional(tokenStream, ref state);
            if (node == null)
                throw new ParserException(tokenStream.Current,
                    "Expected a value.");
            return node;
        }

        private ParseNode ParseSignedValueOptional(TokenStream tokenStream, ref ParserState state)
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
            node.Children.Add(ParseFactorialValue(tokenStream, ref state));
            return node;
        }

        private ParseNode ParseFactorialValue(TokenStream tokenStream, ref ParserState state)
        {
            var node = new ParseNode(ParseNodeKind.PostfixOperator);
            node.Children.Add(ParseIndexedValue(tokenStream, ref state));
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Factorial:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    break;
            }
            return node;
        }

        private ParseNode ParseIndexedValue(TokenStream tokenStream, ref ParserState state)
        {
            var node = new ParseNode(ParseNodeKind.PostfixOperator);
            node.Children.Add(ParseValue(tokenStream, ref state));
            node.Children.Add(ParseIndicesPairOptional(tokenStream, ref state));
            return node;
        }

        private ParseNode ParseValue(TokenStream tokenStream, ref ParserState state)
        {
            var node = ParseValueOptional(tokenStream, ref state);
            if (node == null)
                throw new ParserException(tokenStream.Current,
                    "Expected one of the following: number, letter, open bracket, fraction, binomial, root, function.");
            return node;
        }

        private ParseNode ParseValueOptional(TokenStream tokenStream, ref ParserState state)
        {
            ParseNode node;
            node = ParseRawValueOptional(tokenStream, ref state);
            if (node == null)
                node = ParseGroupOptional(tokenStream, ref state);
            if (node == null)
                node = ParseBracketedExpressionOptional(tokenStream, ref state);
            if (node == null)
                node = ParseFractionOptional(tokenStream, ref state);
            if (node == null)
                node = ParseBinomialOptional(tokenStream, ref state);
            if (node == null)
                node = ParseRootOptional(tokenStream, ref state);
            if (node == null)
                node = ParseFunctionOptional(tokenStream, ref state);
            if (node == null)
                node = ParseBigOperatorOptional(tokenStream, ref state);
            if (node == null)
                node = ParseTextOptional(tokenStream, ref state);
            return node;
        }

        private ParseNode ParseRawValueOptional(TokenStream tokenStream, ref ParserState state)
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

        private ParseNode ParseIndicesPairOptional(TokenStream tokenStream, ref ParserState state)
        {
            var node = new ParseNode(ParseNodeKind.Indices);
            var firstSymbol = tokenStream.Current.Symbol;
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.RaiseToIndex:
                case SymbolKind.LowerToIndex:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    node.Children.Add(ParseIndex(tokenStream, ref state));
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
                    node.Children.Add(ParseIndex(tokenStream, ref state));
                    break;
            }
            return node;
        }

        private ParseNode ParseIndex(TokenStream tokenStream, ref ParserState state)
        {
            ParseNode node;
            node = ParseRawValueOptional(tokenStream, ref state);
            if (node == null)
                node = ParseGroupOptional(tokenStream, ref state);
            if (node == null)
                throw new ParserException(tokenStream.Current,
                    "Expected a single value or group expression.");
            return node;
        }

        private ParseNode ParseGroup(TokenStream tokenStream, ref ParserState state)
        {
            var node = ParseGroupOptional(tokenStream, ref state);
            if (node == null)
                throw new ParserException(tokenStream.Current, new[] {
                    SymbolKind.GroupOpen });
            return node;
        }

        private ParseNode ParseGroupOptional(TokenStream tokenStream, ref ParserState state)
        {
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.GroupOpen:
                    tokenStream.ForceMoveNext();
                    var node = ParseExpression(tokenStream, ref state);
                    if (tokenStream.Current.Symbol != SymbolKind.GroupClose)
                        throw new ParserException(tokenStream.Current, new[] {
                            SymbolKind.GroupClose });
                    tokenStream.ForceMoveNext();
                    return node;
                default:
                    return null;
            }
        }

        private ParseNode ParseBracketedExpressionOptional(TokenStream tokenStream, ref ParserState state)
        {
            var newState = state;
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
                case SymbolKind.ModulusBracket:
                    if (newState.IsModulusBracketOpen)
                        return null;
                    bracketCloseToken = SymbolKind.ModulusBracket;
                    newState.IsModulusBracketOpen = true;
                    break;
                case SymbolKind.NormBracket:
                    if (newState.IsNormBracketOpen)
                        return null;
                    bracketCloseToken = SymbolKind.NormBracket;
                    newState.IsNormBracketOpen = true;
                    break;
                default:
                    return null;
            }
            tokenStream.ForceMoveNext();
            var node = ParseExpression(tokenStream, ref newState);
            if (tokenStream.Current.Symbol != bracketCloseToken)
                throw new ParserException(tokenStream.Current, new[] {
                    bracketCloseToken });
            tokenStream.ForceMoveNext();
            return node;
        }

        private ParseNode ParseFractionOptional(TokenStream tokenStream, ref ParserState state)
        {
            if (tokenStream.Current.Symbol != SymbolKind.Fraction)
                return null;
            var functionNode = new ParseNode(tokenStream.Current);
            tokenStream.ForceMoveNext();
            var node = new ParseNode(ParseNodeKind.PrefixOperator, new[] {
                functionNode, ParseGroup(tokenStream, ref state), ParseGroup(tokenStream, ref state)});
            return node;
        }

        private ParseNode ParseBinomialOptional(TokenStream tokenStream, ref ParserState state)
        {
            if (tokenStream.Current.Symbol != SymbolKind.Binomial)
                return null;
            var functionNode = new ParseNode(tokenStream.Current);
            tokenStream.ForceMoveNext();
            var node = new ParseNode(ParseNodeKind.PrefixOperator, new[] {
                functionNode, ParseGroup(tokenStream, ref state), ParseGroup(tokenStream, ref state)});
            return node;
        }

        private ParseNode ParseRootOptional(TokenStream tokenStream, ref ParserState state)
        {
            if (tokenStream.Current.Symbol != SymbolKind.Root)
                return null;
            var functionNode = new ParseNode(tokenStream.Current);
            tokenStream.ForceMoveNext();
            var node = new ParseNode(ParseNodeKind.PrefixOperator);
            node.Children.Add(functionNode);
            var argumentNode = ParseArgumentOptional(tokenStream, ref state);
            if (argumentNode != null)
                node.Children.Add(argumentNode);
            node.Children.Add(ParseGroup(tokenStream, ref state));
            return node;
        }

        private ParseNode ParseFunctionOptional(TokenStream tokenStream, ref ParserState state)
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
                    var node = new ParseNode(ParseNodeKind.PrefixOperator);
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    var indicesNode = ParseIndicesPairOptional(tokenStream, ref state);
                    indicesNode.IsArgument = true;
                    node.Children.Add(indicesNode);
                    node.Children.Add(ParseExpression(tokenStream, ref state));
                    return node;
                default:
                    return null;
            }
        }

        private ParseNode ParseArgumentOptional(TokenStream tokenStream, ref ParserState state)
        {
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.SquareBracketOpen:
                    tokenStream.ForceMoveNext();
                    var node = ParseExpression(tokenStream, ref state);
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

        private ParseNode ParseBigOperatorOptional(TokenStream tokenStream, ref ParserState state)
        {
            switch (tokenStream.Current.Symbol)
            {
                case SymbolKind.Sum:
                case SymbolKind.Product:
                case SymbolKind.Coproduct:
                case SymbolKind.Integral:
                case SymbolKind.DoubleIntegral:
                case SymbolKind.TripleIntegral:
                case SymbolKind.QuadrupleIntegral:
                case SymbolKind.NtupleIntegral:
                case SymbolKind.ClosedIntegral:
                case SymbolKind.ClosedDoubleIntegral:
                case SymbolKind.ClosedTripleIntegral:
                case SymbolKind.ClosedQuadrupleIntegral:
                case SymbolKind.ClosedNtupleIntegral:
                case SymbolKind.BigOPlus:
                case SymbolKind.BigOTimes:
                case SymbolKind.BigODot:
                case SymbolKind.BigCup:
                case SymbolKind.BigCap:
                case SymbolKind.BigCupPlus:
                case SymbolKind.BigSquareCup:
                case SymbolKind.BigSquareCap:
                case SymbolKind.BigVee:
                case SymbolKind.BigWedge:
                    var node = new ParseNode(ParseNodeKind.PrefixOperator);
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    var indicesNode = ParseIndicesPairOptional(tokenStream, ref state);
                    indicesNode.IsArgument = true;
                    node.Children.Add(indicesNode);
                    node.Children.Add(ParseExpression(tokenStream, ref state));
                    return node;
                default:
                    return null;
            }
        }

        private ParseNode ParseTextOptional(TokenStream tokenStream, ref ParserState state)
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
                    return new ParseNode(Token.FromValue(SymbolKind.Text, sb.ToString(), tokenStream.Current.Position));
                default:
                    throw new ParserException(tokenStream.Current, new[] {
                        SymbolKind.GroupOpen});
            }
        }

        private struct ParserState
        {
            public bool IsModulusBracketOpen;
            public bool IsNormBracketOpen;
        }
    }
}
