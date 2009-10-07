using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public static class TexUtilities
    {
        public static string CreateText(this TexExpressionNode tree)
        {
            return CreateText(CreateTokenStream(tree));
        }

        public static string CreateText(this TokenStream tokenStream)
        {
            using (var stringWriter = new StringWriter())
            {
                var texWriter = new TexWriter(stringWriter);
                texWriter.Write(tokenStream);
                return stringWriter.ToString();
            }
        }

        public static TokenStream CreateTokenStream(string expression)
        {
            var lexer = new TexLexer();
            return lexer.Tokenise(expression);
        }

        public static TokenStream CreateTokenStream(this TexExpressionNode tree)
        {
            var texComposer = new TexComposer();
            return texComposer.Write(tree);
        }

        public static TexExpressionNode CreateExpressionTree(string expression)
        {
            return TexExpressionTreeBuilder.FromParseTree(CreateParseTree(expression));
        }

        public static TexExpressionNode CreateExpressionTree(this TokenStream tokenStream)
        {
            return TexExpressionTreeBuilder.FromParseTree(CreateParseTree(tokenStream));
        }

        public static ParseNode CreateParseTree(string expression)
        {
            return CreateParseTree(CreateTokenStream(expression));
        }

        public static ParseNode CreateParseTree(this TokenStream tokenStream)
        {
            var parser = new TexParser();
            return parser.Parse(tokenStream);
        }

        internal static void ForceMoveNext(this TokenStream tokenStream)
        {
            do
            {
                ForceMoveNextIncludeFormatting(tokenStream);
            } while (tokenStream.Current.Symbol.IsFormatting());
        }

        internal static void ForceMoveNextIncludeFormatting(this TokenStream tokenStream)
        {
            if (!tokenStream.MoveNext())
                throw new ParserException(TexToken.Null, "Unexpected end of token stream.");
        }

        #region Symbols

        public static bool IsLeftAssociativeOperator(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.Minus:
                case TexSymbolKind.PlusMinus:
                case TexSymbolKind.MinusPlus:
                case TexSymbolKind.Divide:
                case TexSymbolKind.Over:
                case TexSymbolKind.Fraction:
                case TexSymbolKind.InlineModulo:
                case TexSymbolKind.RaiseToIndex:
                case TexSymbolKind.LowerToIndex:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsLtrInfixOperator(this TexSymbolKind symbol)
        {
            return IsBinaryOperator(symbol);
        }

        public static bool IsLongOperator(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.Dot:
                case TexSymbolKind.InlineModulo:
                case TexSymbolKind.Over:
                    return true;
                default:
                    if (IsRelationOperator(symbol))
                        return true;
                    return false;
            }
        }

        public static bool IsPlusOrMinusOperator(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.Plus:
                case TexSymbolKind.Minus:
                case TexSymbolKind.PlusMinus:
                case TexSymbolKind.MinusPlus:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsRaiseOrLowerOperator(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.RaiseToIndex:
                case TexSymbolKind.LowerToIndex:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsValue(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.Number:
                case TexSymbolKind.Letter:
                case TexSymbolKind.GreekLetter:
                case TexSymbolKind.Text:
                    return true;
                default:
                    return false;
            }
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

        public static bool IsRelationOperator(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
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
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsBracketedFunction(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.Fraction:
                case TexSymbolKind.Binomial:
                case TexSymbolKind.Root:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFunctionOperator(this TexSymbolKind symbol)
        {
            switch (symbol)
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
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsBigOperator(this TexSymbolKind symbol)
        {
            switch (symbol)
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
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsBinaryOperator(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.Plus:
                case TexSymbolKind.Minus:
                case TexSymbolKind.PlusMinus:
                case TexSymbolKind.MinusPlus:
                case TexSymbolKind.Cross:
                case TexSymbolKind.Dot:
                case TexSymbolKind.Star:
                case TexSymbolKind.Divide:
                case TexSymbolKind.Over:
                case TexSymbolKind.InlineModulo:
                case TexSymbolKind.RaiseToIndex:
                case TexSymbolKind.LowerToIndex:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsPostfixOperator(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.Factorial:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFormatting(this TexSymbolKind symbol)
        {
            switch (symbol)
            {
                case TexSymbolKind.Space:
                case TexSymbolKind.Separator:
                case TexSymbolKind.Left:
                case TexSymbolKind.Right:
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}
