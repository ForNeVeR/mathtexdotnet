﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public class TexParser : IParser
    {
        private const string errorMessageExpectedEndOfStream =
            "Expected end of token stream.";
        private const string errorMessageExpectedValue =
            "Expected one of the following: number, letter, open bracket, fraction, binomial, root, function.";
        private const string errorMessageExpectedValueOrGroup =
            "Expected a single value or group expression.";
        private const string errorMessageTextValueEmpty =
            "A text value must contain at least one character.";

        public TexParser()
        {
        }

        public ParseNode Parse(TokenStream tokenStream)
        {
            var state = CreateDefaultState();
            tokenStream.ForceMoveNext();
            var node = ParseRelationalExpression(tokenStream, ref state);
            if (tokenStream.Current.Symbol != TexSymbolKind.EndOfStream)
                throw new TexParserException(tokenStream.Current, errorMessageExpectedEndOfStream);
            return node;
        }

        private ParserState CreateDefaultState()
        {
            var state = new ParserState();
            state.IsModulusBracketOpen = false;
            state.IsNormBracketOpen = false;
            return state;
        }

        private ParseNode ParseRelationalExpression(TokenStream tokenStream, ref ParserState state)
        {
            return ParseRelationalExpression(tokenStream, ref state, false);
        }

        private ParseNode ParseRelationalExpression(TokenStream tokenStream, ref ParserState state, bool isSubTree)
        {
            var node = new ParseNode(ParseNodeKind.InfixOperator);
            node.Children.Add(ParseFractionalExpression(tokenStream, ref state));
            switch (tokenStream.Current.Symbol)
            {
                case TexSymbolKind.EndOfStream:
                    return node;
                case TexSymbolKind.Equals:
                case TexSymbolKind.NotEquals:
                case TexSymbolKind.DotEquals:
                case TexSymbolKind.Approximates:
                case TexSymbolKind.Equivalent:
                case TexSymbolKind.LessThan:
                case TexSymbolKind.LessThanOrEqualTo:
                case TexSymbolKind.GreaterThan:
                case TexSymbolKind.GreaterThanOrEqualTo:
                case TexSymbolKind.MuchLessThan:
                case TexSymbolKind.MuchGreaterThan:
                case TexSymbolKind.Proportional:
                case TexSymbolKind.Asymptotic:
                case TexSymbolKind.Bowtie:
                case TexSymbolKind.Models:
                case TexSymbolKind.Precedes:
                case TexSymbolKind.PrecedesOrEquals:
                case TexSymbolKind.Succedes:
                case TexSymbolKind.SuccedesOrEquals:
                case TexSymbolKind.Congruent:
                case TexSymbolKind.Similar:
                case TexSymbolKind.SimilarOrEquals:
                case TexSymbolKind.Perpendicular:
                case TexSymbolKind.Parallel:
                case TexSymbolKind.Middle:
                case TexSymbolKind.Subset:
                case TexSymbolKind.SubsetOrEqualTo:
                case TexSymbolKind.Superset:
                case TexSymbolKind.SupersetOrEqualTo:
                case TexSymbolKind.SquareSubset:
                case TexSymbolKind.SquareSubsetOrEqualTo:
                case TexSymbolKind.SquareSuperset:
                case TexSymbolKind.SquareSupersetOrEqualTo:
                case TexSymbolKind.Member:
                case TexSymbolKind.NotMember:
                case TexSymbolKind.Contains:
                case TexSymbolKind.NotContains:
                case TexSymbolKind.Smile:
                case TexSymbolKind.Frown:
                case TexSymbolKind.VLineDash:
                case TexSymbolKind.DashVLine:
                    node.IsSubExpression = isSubTree;
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    break;
                default:
                    return node;
            }
            node.Children.Add(ParseRelationalExpression(tokenStream, ref state, true));
            return node;
        }

        private ParseNode ParseFractionalExpression(TokenStream tokenStream, ref ParserState state)
        {
            var node = new ParseNode(ParseNodeKind.InfixOperator);
            node.Children.Add(ParseExpression(tokenStream, ref state));
            switch (tokenStream.Current.Symbol)
            {
                case TexSymbolKind.EndOfStream:
                    return node;
                case TexSymbolKind.Over:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    break;
                default:
                    return node;
            }
            node.Children.Add(ParseExpression(tokenStream, ref state));
            return node;
        }

        private ParseNode ParseExpression(TokenStream tokenStream, ref ParserState state)
        {
            return ParseExpression(tokenStream, ref state, false);
        }

        private ParseNode ParseExpression(TokenStream tokenStream, ref ParserState state, bool isSubTree)
        {
            var node = new ParseNode(ParseNodeKind.InfixOperator);
            node.Children.Add(ParseSignedTerm(tokenStream, ref state));
            switch (tokenStream.Current.Symbol)
            {
                case TexSymbolKind.EndOfStream:
                    return node;
                case TexSymbolKind.Plus:
                case TexSymbolKind.Minus:
                case TexSymbolKind.PlusMinus:
                case TexSymbolKind.MinusPlus:
                    node.IsSubExpression = isSubTree;
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    break;
                default:
                    return node;
            }
            node.Children.Add(ParseExpression(tokenStream, ref state, true));
            return node;
        }

        private ParseNode ParseSignedTerm(TokenStream tokenStream, ref ParserState state)
        {
            var node = new ParseNode(ParseNodeKind.PrefixOperator);
            switch (tokenStream.Current.Symbol)
            {
                case TexSymbolKind.Plus:
                case TexSymbolKind.Minus:
                case TexSymbolKind.PlusMinus:
                case TexSymbolKind.MinusPlus:
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    break;
            }
            node.Children.Add(ParseTerm(tokenStream, ref state));
            return node;
        }

        private ParseNode ParseTerm(TokenStream tokenStream, ref ParserState state)
        {
            var node = ParseTermOptional(tokenStream, ref state);
            if (node == null)
                throw new TexParserException(tokenStream.Current, errorMessageExpectedValue);
            return node;
        }

        private ParseNode ParseTermOptional(TokenStream tokenStream, ref ParserState state)
        {
            return ParseTermOptional(tokenStream, ref state, false);
        }

        private ParseNode ParseTermOptional(TokenStream tokenStream, ref ParserState state, bool isSubTree)
        {
            var valueNode = ParseFactorialValueOptional(tokenStream, ref state);
            if (valueNode == null)
                return null;
            var node = new ParseNode(ParseNodeKind.InfixOperator);
            node.Children.Add(valueNode);
            var opNode = ParseTermOperatorOptional(tokenStream, ref state);
            var termNode = ParseTermOptional(tokenStream, ref state, true);
            if (termNode == null)
                return node;
            if (opNode == null)
                node.Children.Add(new ParseNode(TexToken.FromSymbol(TexSymbolKind.Dot,
                    tokenStream.Current.SourcePosition, null)));
            else
                node.Children.Add(opNode);
            node.Children.Add(termNode);
            node.IsSubExpression = isSubTree;
            return node;
        }

        private ParseNode ParseTermOperatorOptional(TokenStream tokenStream, ref ParserState state)
        {
            switch (tokenStream.Current.Symbol)
            {
                case TexSymbolKind.Cross:
                case TexSymbolKind.Dot:
                case TexSymbolKind.Star:
                case TexSymbolKind.Divide:
                case TexSymbolKind.InlineModulo:
                case TexSymbolKind.Over:
                    var node = new ParseNode(tokenStream.Current);
                    tokenStream.ForceMoveNext();
                    return node;
                default:
                    return null;
            }
        }

        private ParseNode ParseFactorialValue(TokenStream tokenStream, ref ParserState state)
        {
            var node = ParseFactorialValueOptional(tokenStream, ref state);
            if (node == null)
                throw new TexParserException(tokenStream.Current, errorMessageExpectedValue);
            return node;
        }

        private ParseNode ParseFactorialValueOptional(TokenStream tokenStream, ref ParserState state)
        {
            var valueNode = ParseIndexedValueOptional(tokenStream, ref state);
            if (valueNode == null)
                return null;
            var node = new ParseNode(ParseNodeKind.PostfixOperator);
            node.Children.Add(valueNode);
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
            var node = ParseIndexedValueOptional(tokenStream, ref state);
            if (node == null)
                throw new TexParserException(tokenStream.Current, errorMessageExpectedValue);
            return node;
        }

        private ParseNode ParseIndexedValueOptional(TokenStream tokenStream, ref ParserState state)
        {
            var valueNode = ParseValueOptional(tokenStream, ref state);
            if (valueNode == null)
                return null;
            var node = new ParseNode(ParseNodeKind.PostfixOperator);
            node.Children.Add(valueNode);
            node.Children.Add(ParseIndicesPairOptional(tokenStream, ref state));
            return node;
        }

        private ParseNode ParseValue(TokenStream tokenStream, ref ParserState state)
        {
            var node = ParseValueOptional(tokenStream, ref state);
            if (node == null)
                throw new TexParserException(tokenStream.Current, errorMessageExpectedValue);
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
                        throw new TexParserException(tokenStream.Current, new[] {
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
                throw new TexParserException(tokenStream.Current, errorMessageExpectedValueOrGroup);
            return node;
        }

        private ParseNode ParseGroup(TokenStream tokenStream, ref ParserState state)
        {
            var node = ParseGroupOptional(tokenStream, ref state);
            if (node == null)
                throw new TexParserException(tokenStream.Current, new[] { 
                    TexSymbolKind.GroupOpen });
            return node;
        }

        private ParseNode ParseGroupOptional(TokenStream tokenStream, ref ParserState state)
        {
            switch (tokenStream.Current.Symbol)
            {
                case TexSymbolKind.GroupOpen:
                    tokenStream.ForceMoveNext();
                    var node = ParseRelationalExpression(tokenStream, ref state);
                    if (tokenStream.Current.Symbol != TexSymbolKind.GroupClose)
                        throw new TexParserException(tokenStream.Current, new[] {
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
            var node = ParseRelationalExpression(tokenStream, ref newState);
            if (tokenStream.Current.Symbol != bracketCloseToken)
                throw new TexParserException(tokenStream.Current, new[] {
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
                case TexSymbolKind.HypSine:
                case TexSymbolKind.HypCosine:
                case TexSymbolKind.HypTangent:
                case TexSymbolKind.HypCosecant:
                case TexSymbolKind.HypSecant:
                case TexSymbolKind.HypCotangent:
                case TexSymbolKind.ArHypSine:
                case TexSymbolKind.ArHypCosine:
                case TexSymbolKind.ArHypTangent:
                case TexSymbolKind.ArHypCosecant:
                case TexSymbolKind.ArHypSecant:
                case TexSymbolKind.ArHypCotangent:
                    var node = new ParseNode(ParseNodeKind.PrefixOperator);
                    node.Children.Add(new ParseNode(tokenStream.Current));
                    tokenStream.ForceMoveNext();
                    var indicesNode = ParseIndicesPairOptional(tokenStream, ref state);
                    indicesNode.IsArgument = true;
                    node.Children.Add(indicesNode);
                    node.Children.Add(ParseRelationalExpression(tokenStream, ref state));
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
                        throw new TexParserException(tokenStream.Current, new[] {
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
                    node.Children.Add(ParseRelationalExpression(tokenStream, ref state));
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
                        throw new TexParserException(tokenStream.Current, errorMessageTextValueEmpty);
                    tokenStream.ForceMoveNext();
                    return new ParseNode(TexToken.FromValue(TexSymbolKind.Text, sb.ToString(),
                        tokenStream.Current.SourcePosition, null));
                default:
                    throw new TexParserException(tokenStream.Current, new[] {
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
