using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public class TexWriter
    {
        public TexWriter(TextWriter writer)
        {
            this.TextWriter = writer;

            this.IgnoreUnknownSymbols = true;
            this.StrictMode = true;
        }

        public bool IgnoreUnknownSymbols
        {
            get;
            set;
        }

        public bool StrictMode
        {
            get;
            set;
        }

        public TextWriter TextWriter
        {
            get;
            private set;
        }

        public void Write(TokenStream tokenStream)
        {
            var prevToken = TexToken.Null;
            while (tokenStream.MoveNext())
            {
                var chr = GetShortSymbol(tokenStream.Current);
                if (chr != null)
                {
                    Write(chr.ToString(), prevToken);
                    goto tokenWritten;
                }
                var str = GetLongSymbol(tokenStream.Current);
                if (str != null)
                {
                    Write('\\' + str, prevToken);
                    goto tokenWritten;
                }
                if (tokenStream.Current.Symbol == TexSymbolKind.Number)
                {
                    var value = (double)tokenStream.Current.Value;
                    if (double.IsPositiveInfinity(value))
                        Write("infty", prevToken);
                    else
                        Write(value.ToString(), prevToken);
                    goto tokenWritten;
                }
                if (!this.IgnoreUnknownSymbols)
                    throw new WriterException(tokenStream.Current);

            tokenWritten:
                prevToken = tokenStream.Current;
            }
        }

        private void Write(string value, TexToken prevToken)
        {
            var encloseWithGroup = this.StrictMode &&
                (prevToken.Symbol == TexSymbolKind.RaiseToIndex || prevToken.Symbol == TexSymbolKind.LowerToIndex) &&
                value.Length > 1;

            if (encloseWithGroup)
                this.TextWriter.Write("{");
            this.TextWriter.Write(value);
            if (encloseWithGroup)
                this.TextWriter.Write("}");
        }

        private char? GetShortSymbol(TexToken token)
        {
            switch (token.Symbol)
            {
                case TexSymbolKind.RaiseToIndex:
                    return '^';
                case TexSymbolKind.LowerToIndex:
                    return '_';
                case TexSymbolKind.Prime:
                    return '\'';
                case TexSymbolKind.Colon:
                    return ':';
                case TexSymbolKind.Comma:
                    return ',';
                case TexSymbolKind.Letter:
                    return (char)token.Value;

                #region Relations
                case TexSymbolKind.Equals:
                    return '=';
                case TexSymbolKind.LessThan:
                    return '<';
                case TexSymbolKind.GreaterThan:
                    return '>';
                #endregion

                #region Operators
                case TexSymbolKind.Plus:
                    return '+';
                case TexSymbolKind.Minus:
                    return '-';
                case TexSymbolKind.Star:
                    return '*';
                case TexSymbolKind.Divide:
                    return '/';
                case TexSymbolKind.Factorial:
                    return '!';
                #endregion

                #region Brackets
                case TexSymbolKind.GroupOpen:
                    return '{';
                case TexSymbolKind.GroupClose:
                    return '}';
                case TexSymbolKind.RoundBracketOpen:
                    return '(';
                case TexSymbolKind.RoundBracketClose:
                    return ')';
                case TexSymbolKind.SquareBracketOpen:
                    return '[';
                case TexSymbolKind.SquareBracketClose:
                    return ']';
                case TexSymbolKind.ModulusBracket:
                    return '|';
                #endregion

                #region Formatting
                case TexSymbolKind.Space:
                    return ' ';
                #endregion

                default:
                    return null;
            }
        }

        private string GetLongSymbol(TexToken token)
        {
            switch (token.Symbol)
            {
                case TexSymbolKind.Text:
                    return string.Format("text{{{0}}}", (string)token.Value);
                case TexSymbolKind.GreekLetter:
                    return (string)token.Value;

                #region Brackets
                case TexSymbolKind.CurlyBracketOpen:
                    return "{";
                case TexSymbolKind.CurlyBracketClose:
                    return "}";
                case TexSymbolKind.AngleBracketOpen:
                    return "langle";
                case TexSymbolKind.AngleBracketClose:
                    return "rangle";
                case TexSymbolKind.FloorBracketOpen:
                    return "lfloor";
                case TexSymbolKind.FloorBracketClose:
                    return "rfloor";
                case TexSymbolKind.CeilingBracketOpen:
                    return "lceil";
                case TexSymbolKind.CeilingBracketClose:
                    return "rceil";
                case TexSymbolKind.NormBracket:
                    return "|";
                #endregion

                #region Relations
                case TexSymbolKind.NotEquals:
                    return "neq";
                case TexSymbolKind.DotEquals:
                    return "doteq";
                case TexSymbolKind.Approximates:
                    return "approx";
                case TexSymbolKind.Equivalent:
                    return "equiv";
                case TexSymbolKind.LessThanOrEqualTo:
                    return "leq";
                case TexSymbolKind.GreaterThanOrEqualTo:
                    return "geq";
                case TexSymbolKind.MuchLessThan:
                    return "ll";
                case TexSymbolKind.MuchGreaterThan:
                    return "gg";
                case TexSymbolKind.Proportional:
                    return "propto";
                case TexSymbolKind.Asymptotic:
                    return "asymp";
                case TexSymbolKind.Bowtie:
                    return "bowtie";
                case TexSymbolKind.Models:
                    return "models";
                case TexSymbolKind.Precedes:
                    return "prec";
                case TexSymbolKind.PrecedesOrEquals:
                    return "preceq";
                case TexSymbolKind.Succedes:
                    return "succ";
                case TexSymbolKind.SuccedesOrEquals:
                    return "succeq";
                case TexSymbolKind.Congruent:
                    return "cong";
                case TexSymbolKind.Similar:
                    return "sim";
                case TexSymbolKind.SimilarOrEquals:
                    return "simeq";
                case TexSymbolKind.Perpendicular:
                    return "perp";
                case TexSymbolKind.Middle:
                    return "mid";
                case TexSymbolKind.Subset:
                    return "subset";
                case TexSymbolKind.SubsetOrEqualTo:
                    return "subseteq";
                case TexSymbolKind.Superset:
                    return "supset";
                case TexSymbolKind.SupersetOrEqualTo:
                    return "supseteq";
                case TexSymbolKind.SquareSubset:
                    return "sqsubset";
                case TexSymbolKind.SquareSubsetOrEqualTo:
                    return "sqsubseteq";
                case TexSymbolKind.SquareSuperset:
                    return "sqsupset";
                case TexSymbolKind.SquareSupersetOrEqualTo:
                    return "sqsupseteq";
                case TexSymbolKind.Member:
                    return "in";
                case TexSymbolKind.NotMember:
                    return "nin";
                case TexSymbolKind.Contains:
                    return "ni";
                case TexSymbolKind.NotContains:
                    return "nni";
                case TexSymbolKind.Smile:
                    return "smile";
                case TexSymbolKind.Frown:
                    return "frown";
                case TexSymbolKind.VLineDash:
                    return "vdash";
                case TexSymbolKind.DashVLine:
                    return "dashv";
                #endregion

                #region Operators
                case TexSymbolKind.PlusMinus:
                    return "pm";
                case TexSymbolKind.MinusPlus:
                    return "mp";
                case TexSymbolKind.Cross:
                    return "times";
                case TexSymbolKind.Dot:
                    return "cdot";
                case TexSymbolKind.Divide:
                    return "div";
                case TexSymbolKind.Over:
                    return "over";
                #endregion

                #region Functions
                case TexSymbolKind.Fraction:
                    return "frac";
                case TexSymbolKind.Binomial:
                    return "binom";
                case TexSymbolKind.Root:
                    return "sqrt";
                case TexSymbolKind.Minimum:
                    return "min";
                case TexSymbolKind.Maximum:
                    return "max";
                case TexSymbolKind.GreatestCommonDenominator:
                    return "gcd";
                case TexSymbolKind.LowestCommonMultiple:
                    return "lcm";
                case TexSymbolKind.Exponent:
                    return "exp";
                case TexSymbolKind.Log:
                    return "log";
                case TexSymbolKind.NaturalLog:
                    return "ln";
                case TexSymbolKind.Argument:
                    return "arg";
                case TexSymbolKind.Limit:
                    return "lim";
                case TexSymbolKind.LimitInferior:
                    return "liminf";
                case TexSymbolKind.LimitSuperior:
                    return "limsup";
                case TexSymbolKind.Sine:
                    return "sin";
                case TexSymbolKind.Cosine:
                    return "cos";
                case TexSymbolKind.Tangent:
                    return "tan";
                case TexSymbolKind.Secant:
                    return "sec";
                case TexSymbolKind.Cosecant:
                    return "csc";
                case TexSymbolKind.Cotangent:
                    return "cot";
                case TexSymbolKind.ArcSine:
                    return "arcsin";
                case TexSymbolKind.ArcCosine:
                    return "arccos";
                case TexSymbolKind.ArcTangent:
                    return "arctan";
                case TexSymbolKind.ArcSecant:
                    return "arcsec";
                case TexSymbolKind.ArcCosecant:
                    return "arccsc";
                case TexSymbolKind.ArcCotangent:
                    return "arccot";
                case TexSymbolKind.HypSine:
                    return "sinh";
                case TexSymbolKind.HypCosine:
                    return "cosh";
                case TexSymbolKind.HypTangent:
                    return "tanh";
                case TexSymbolKind.HypSecant:
                    return "sech";
                case TexSymbolKind.HypCosecant:
                    return "csch";
                case TexSymbolKind.HypCotangent:
                    return "coth";
                case TexSymbolKind.ArHypSine:
                    return "arcsinh";
                case TexSymbolKind.ArHypCosine:
                    return "arccosh";
                case TexSymbolKind.ArHypTangent:
                    return "arctanh";
                case TexSymbolKind.ArHypSecant:
                    return "arcsech";
                case TexSymbolKind.ArHypCosecant:
                    return "arccsch";
                case TexSymbolKind.ArHypCotangent:
                    return "arccoth";
                case TexSymbolKind.InlineModulo:
                    return "bmod";
                case TexSymbolKind.IdentityModulo:
                    return "pmod";
                case TexSymbolKind.Sum:
                    return "sum";
                case TexSymbolKind.Product:
                    return "prod";
                case TexSymbolKind.Coproduct:
                    return "coprod";
                case TexSymbolKind.Integral:
                    return "int";
                case TexSymbolKind.DoubleIntegral:
                    return "iint";
                case TexSymbolKind.TripleIntegral:
                    return "iiint";
                case TexSymbolKind.QuadrupleIntegral:
                    return "iiiint";
                case TexSymbolKind.NtupleIntegral:
                    return "idotsint";
                case TexSymbolKind.ClosedIntegral:
                    return "oint";
                case TexSymbolKind.ClosedDoubleIntegral:
                    return "oiint";
                case TexSymbolKind.ClosedTripleIntegral:
                    return "oiiint";
                case TexSymbolKind.ClosedQuadrupleIntegral:
                    return "oiiiint";
                case TexSymbolKind.ClosedNtupleIntegral:
                    return "oidotsint";
                case TexSymbolKind.BigOPlus:
                    return "bigoplus";
                case TexSymbolKind.BigOTimes:
                    return "bigotimes";
                case TexSymbolKind.BigODot:
                    return "bigodot";
                case TexSymbolKind.BigCup:
                    return "bigcup";
                case TexSymbolKind.BigCap:
                    return "bigcap";
                case TexSymbolKind.BigCupPlus:
                    return "bigcupplus";
                case TexSymbolKind.BigSquareCup:
                    return "bigsqcup";
                case TexSymbolKind.BigSquareCap:
                    return "bigsqcap";
                case TexSymbolKind.BigVee:
                    return "bigveee";
                case TexSymbolKind.BigWedge:
                    return "bigwedge";
                #endregion

                #region Formatting
                case TexSymbolKind.Separator:
                    return ",";
                case TexSymbolKind.Left:
                    return "left";
                case TexSymbolKind.Right:
                    return "right";
                #endregion

                default:
                    return null;
            }
        }
    }
}
