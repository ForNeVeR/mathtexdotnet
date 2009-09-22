using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public class TexParser : IParser
    {
        public TexParser()
        {
        }

        public ParseNode Parse(TokenStream tokenStream)
        {
            var state = CreateDefaultState();
            tokenStream.ForceMoveNext();
            var node = ParseExpression(tokenStream, ref state);
            if (tokenStream.Current.Symbol != TexSymbolKind.EndOfStream)
                throw new ParserException(tokenStream.Current,
                    "Expected end of token stream.");
            return node;
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
                case TexSymbolKind.EndOfStream:
                    return node;
                case TexSymbolKind.Plus:
                case TexSymbolKind.Minus:
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
                case TexSymbolKind.Cross:
                case TexSymbolKind.Dot:
                case TexSymbolKind.Star:
                case TexSymbolKind.Divide:
                case TexSymbolKind.Over:
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
            node.Children.Add(new ParseNode(TexToken.FromKind(TexSymbolKind.Dot, tokenStream.Current.SourcePosition,
                null)));
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
            node.Children.Add(new ParseNode(TexToken.FromKind(TexSymbolKind.Dot, tokenStream.Current.SourcePosition,
                null)));
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
                case TexSymbolKind.Plus:
                case TexSymbolKind.Minus:
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
                case TexSymbolKind.Factorial:
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
                case TexSymbolKind.Number:
                case TexSymbolKind.Letter:
                case TexSymbolKind.GreekLetter:
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
                case TexSymbolKind.RaiseToIndex:
                case TexSymbolKind.LowerToIndex:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    node.Children.Add(ParseIndex(tokenStream, ref state));
                    break;
                default:
                    return node;
            }
            switch (tokenStream.Current.Symbol)
            {
                case TexSymbolKind.RaiseToIndex:
                case TexSymbolKind.LowerToIndex:
                    if (tokenStream.Current.Symbol == firstSymbol)
                    {
                        throw new ParserException(tokenStream.Current, new[] {
                            firstSymbol == TexSymbolKind.RaiseToIndex ? TexSymbolKind.LowerToIndex : 
                            TexSymbolKind.RaiseToIndex});
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
                    TexSymbolKind.GroupOpen });
            return node;
        }

        private ParseNode ParseGroupOptional(TokenStream tokenStream, ref ParserState state)
        {
            switch (tokenStream.Current.Symbol)
            {
                case TexSymbolKind.GroupOpen:
                    tokenStream.ForceMoveNext();
                    var node = ParseExpression(tokenStream, ref state);
                    if (tokenStream.Current.Symbol != TexSymbolKind.GroupClose)
                        throw new ParserException(tokenStream.Current, new[] {
                            TexSymbolKind.GroupClose });
                    tokenStream.ForceMoveNext();
                    return node;
                default:
                    return null;
            }
        }

        private ParseNode ParseBracketedExpressionOptional(TokenStream tokenStream, ref ParserState state)
        {
            var newState = state;
            TexSymbolKind bracketCloseToken;
            switch (tokenStream.Current.Symbol)
            {
                case TexSymbolKind.RoundBracketOpen:
                    bracketCloseToken = TexSymbolKind.RoundBracketClose;
                    break;
                case TexSymbolKind.SquareBracketOpen:
                    bracketCloseToken = TexSymbolKind.SquareBracketClose;
                    break;
                case TexSymbolKind.CurlyBracketOpen:
                    bracketCloseToken = TexSymbolKind.CurlyBracketClose;
                    break;
                case TexSymbolKind.AngleBracketOpen:
                    bracketCloseToken = TexSymbolKind.AngleBracketClose;
                    break;
                case TexSymbolKind.FloorBracketOpen:
                    bracketCloseToken = TexSymbolKind.FloorBracketClose;
                    break;
                case TexSymbolKind.CeilingBracketOpen:
                    bracketCloseToken = TexSymbolKind.CeilingBracketClose;
                    break;
                case TexSymbolKind.ModulusBracket:
                    if (newState.IsModulusBracketOpen)
                        return null;
                    bracketCloseToken = TexSymbolKind.ModulusBracket;
                    newState.IsModulusBracketOpen = true;
                    break;
                case TexSymbolKind.NormBracket:
                    if (newState.IsNormBracketOpen)
                        return null;
                    bracketCloseToken = TexSymbolKind.NormBracket;
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
            if (tokenStream.Current.Symbol != TexSymbolKind.Fraction)
                return null;
            var functionNode = new ParseNode(tokenStream.Current);
            tokenStream.ForceMoveNext();
            var node = new ParseNode(ParseNodeKind.PrefixOperator, new[] {
                functionNode, ParseGroup(tokenStream, ref state), ParseGroup(tokenStream, ref state)});
            return node;
        }

        private ParseNode ParseBinomialOptional(TokenStream tokenStream, ref ParserState state)
        {
            if (tokenStream.Current.Symbol != TexSymbolKind.Binomial)
                return null;
            var functionNode = new ParseNode(tokenStream.Current);
            tokenStream.ForceMoveNext();
            var node = new ParseNode(ParseNodeKind.PrefixOperator, new[] {
                functionNode, ParseGroup(tokenStream, ref state), ParseGroup(tokenStream, ref state)});
            return node;
        }

        private ParseNode ParseRootOptional(TokenStream tokenStream, ref ParserState state)
        {
            if (tokenStream.Current.Symbol != TexSymbolKind.Root)
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
                case TexSymbolKind.Minimum:
                case TexSymbolKind.Maximum:
                case TexSymbolKind.GreatestCommonDenominator:
                case TexSymbolKind.LowestCommonMultiple:
                case TexSymbolKind.Exponent:
                case TexSymbolKind.Log:
                case TexSymbolKind.NaturalLog:
                case TexSymbolKind.Argument:
                case TexSymbolKind.Limit:
                case TexSymbolKind.LimitInferior:
                case TexSymbolKind.LimitSuperior:
                case TexSymbolKind.Sine:
                case TexSymbolKind.Cosine:
                case TexSymbolKind.Tangent:
                case TexSymbolKind.Cosecant:
                case TexSymbolKind.Secant:
                case TexSymbolKind.Cotangent:
                case TexSymbolKind.ArcSine:
                case TexSymbolKind.ArcCosine:
                case TexSymbolKind.ArcTangent:
                case TexSymbolKind.ArcCosecant:
                case TexSymbolKind.ArcSecant:
                case TexSymbolKind.ArcCotangent:
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
                case TexSymbolKind.SquareBracketOpen:
                    tokenStream.ForceMoveNext();
                    var node = ParseExpression(tokenStream, ref state);
                    if (tokenStream.Current.Symbol != TexSymbolKind.SquareBracketClose)
                        throw new ParserException(tokenStream.Current, new[] {
                            TexSymbolKind.SquareBracketClose });
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
                case TexSymbolKind.Sum:
                case TexSymbolKind.Product:
                case TexSymbolKind.Coproduct:
                case TexSymbolKind.Integral:
                case TexSymbolKind.DoubleIntegral:
                case TexSymbolKind.TripleIntegral:
                case TexSymbolKind.QuadrupleIntegral:
                case TexSymbolKind.NtupleIntegral:
                case TexSymbolKind.ClosedIntegral:
                case TexSymbolKind.ClosedDoubleIntegral:
                case TexSymbolKind.ClosedTripleIntegral:
                case TexSymbolKind.ClosedQuadrupleIntegral:
                case TexSymbolKind.ClosedNtupleIntegral:
                case TexSymbolKind.BigOPlus:
                case TexSymbolKind.BigOTimes:
                case TexSymbolKind.BigODot:
                case TexSymbolKind.BigCup:
                case TexSymbolKind.BigCap:
                case TexSymbolKind.BigCupPlus:
                case TexSymbolKind.BigSquareCup:
                case TexSymbolKind.BigSquareCap:
                case TexSymbolKind.BigVee:
                case TexSymbolKind.BigWedge:
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
            if (tokenStream.Current.Symbol != TexSymbolKind.Text)
                return null;
            tokenStream.ForceMoveNext();
            switch (tokenStream.Current.Symbol)
            {
                case TexSymbolKind.GroupOpen:
                    tokenStream.ForceMoveNext();
                    var sb = new StringBuilder();
                    while (tokenStream.Current.Symbol != TexSymbolKind.GroupClose)
                    {
                        sb.Append((char)tokenStream.Current.Value);
                        tokenStream.ForceMoveNext();
                    }
                    if (sb.Length == 0)
                        throw new ParserException(tokenStream.Current,
                            "A text value must contain at least one character.");
                    tokenStream.ForceMoveNext();
                    return new ParseNode(TexToken.FromValue(TexSymbolKind.Text, sb.ToString(),
                        tokenStream.Current.SourcePosition, null));
                default:
                    throw new ParserException(tokenStream.Current, new[] {
                        TexSymbolKind.GroupOpen});
            }
        }

        private struct ParserState
        {
            public bool IsModulusBracketOpen;
            public bool IsNormBracketOpen;
        }
    }
}
