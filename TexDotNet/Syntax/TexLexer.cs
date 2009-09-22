using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public class TexLexer : ILexer
    {
        public TexLexer()
        {
            this.IgnoreUnknownSymbols = true;
        }

        public bool IgnoreUnknownSymbols
        {
            get;
            set;
        }

        public TokenStream Tokenise(string input)
        {
            return Tokenise(new StringReader(input));
        }

        public TokenStream Tokenise(Stream stream)
        {
            return Tokenise(new StreamReader(stream));
        }

        public TokenStream Tokenise(TextReader reader)
        {
            return Tokenise(new TrackedTextReader(reader));
        }

        public TokenStream Tokenise(TrackedTextReader reader)
        {
            char nextChar;
            while (reader.Peek() != -1)
            {
                nextChar = (char)reader.Peek();

                if (char.IsWhiteSpace(nextChar))
                {
                    reader.Read();
                }
                else if (char.IsDigit(nextChar))
                {
                    yield return TexToken.FromNumber(ScanReal(reader), reader.Position, nextChar.ToString());
                }
                else if (nextChar == '\\')
                {
                    object value;
                    var token = TexToken.FromValue(ScanLongSymbol(reader, out value), value, reader.Position,
                        nextChar.ToString());
                    if (token.Symbol != TexSymbolKind.Unknown)
                        yield return token;
                }
                else
                {
                    object value;
                    yield return TexToken.FromValue(ScanShortSymbol(reader, out value), value, reader.Position,
                        nextChar.ToString());
                }
            }
            yield return TexToken.FromKind(TexSymbolKind.EndOfStream, reader.Position, null);
        }

        protected double ScanReal(TrackedTextReader reader)
        {
            var sb = new StringBuilder();
            while (Char.IsDigit((char)reader.Peek()))
                sb.Append((char)reader.Read());
            if (reader.Peek() == '.')
            {
                sb.Append((char)reader.Read());
                while (Char.IsDigit((char)reader.Peek()))
                    sb.Append((char)reader.Read());
            }
            return double.Parse(sb.ToString(), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture.NumberFormat);
        }

        protected TexSymbolKind ScanShortSymbol(TrackedTextReader reader, out object value)
        {
            char chr = (char)reader.Read();
            value = null;
            switch (chr)
            {
                case '^':
                    return TexSymbolKind.RaiseToIndex;
                case '_':
                    return TexSymbolKind.LowerToIndex;
                case '\'':
                    return TexSymbolKind.Prime;
                case ':':
                    return TexSymbolKind.Colon;
                case ',':
                    return TexSymbolKind.Comma;

                #region Relations
                case '=':
                    return TexSymbolKind.Equals;
                case '<':
                    return TexSymbolKind.LessThan;
                case '>':
                    return TexSymbolKind.GreaterThan;
                #endregion

                #region Operators
                case '+':
                    return TexSymbolKind.Plus;
                case '-':
                    return TexSymbolKind.Minus;
                case '*':
                    return TexSymbolKind.Star;
                case '/':
                    return TexSymbolKind.Divide;
                case '!':
                    return TexSymbolKind.Factorial;
                #endregion

                #region Brackets
                case '{':
                    return TexSymbolKind.GroupOpen;
                case '}':
                    return TexSymbolKind.GroupClose;
                case '(':
                    return TexSymbolKind.RoundBracketOpen;
                case ')':
                    return TexSymbolKind.RoundBracketClose;
                case '[':
                    return TexSymbolKind.SquareBracketOpen;
                case ']':
                    return TexSymbolKind.SquareBracketClose;
                case '|':
                    return TexSymbolKind.ModulusBracket;
                #endregion

                default:
                    if (char.IsLetter(chr))
                    {
                        value = chr;
                        return TexSymbolKind.Letter;
                    }
                    throw new LexerException(reader.Position, chr.ToString(), string.Format(
                        "Illegal character '{0}'.", chr));
            }
        }

        protected TexSymbolKind ScanLongSymbol(TrackedTextReader reader, out object value)
        {
            var sb = new StringBuilder();
            char nextChr;
            while (reader.Peek() != -1)
            {
                nextChr = (char)reader.Peek();
                if (char.IsWhiteSpace(nextChr))
                    break;
                if (nextChr == ',')
                {
                    sb.Append((char)reader.Read());
                    break;
                }
                if (sb.Length > 0 && !char.IsLetter(nextChr))
                    break;

                sb.Append((char)reader.Read());
            }

            var symbol = sb.ToString();
            value = null;
            switch (symbol.Substring(1))
            {
                case "infty":
                    value = double.PositiveInfinity;
                    return TexSymbolKind.Number;
                case "text":
                    return TexSymbolKind.Text;

                #region Greek letters
                case "alpha":
                case "Alpha":
                case "beta":
                case "Beta":
                case "gamma":
                case "Gamma":
                case "delta":
                case "Delta":
                case "epsilon":
                case "Epsilon":
                case "varepsilon":
                case "zeta":
                case "Zeta":
                case "eta":
                case "Eta":
                case "theta":
                case "Theta":
                case "vartheta":
                case "iota":
                case "Iota":
                case "kappa":
                case "Kappa":
                case "lambda":
                case "Lambda":
                case "mu":
                case "Mu":
                case "nu":
                case "Nu":
                case "xi":
                case "Xi":
                case "omicron":
                case "Omicron":
                case "pi":
                case "Pi":
                case "rho":
                case "Rho":
                case "varrho":
                case "sigma":
                case "Sigma":
                case "varsigma":
                case "tau":
                case "Tau":
                case "upsilon":
                case "Upsilon":
                case "phi":
                case "Phi":
                case "varphi":
                case "chi":
                case "Chi":
                case "psi":
                case "Psi":
                case "omega":
                case "Omega":
                    value = symbol.Substring(1);
                    return TexSymbolKind.GreekLetter;
                #endregion

                #region Brackets
                case "{":
                    return TexSymbolKind.CurlyBracketOpen;
                case "}":
                    return TexSymbolKind.CurlyBracketClose;
                case "langle":
                    return TexSymbolKind.AngleBracketOpen;
                case "rangle":
                    return TexSymbolKind.AngleBracketClose;
                case "lfloor":
                    return TexSymbolKind.FloorBracketOpen;
                case "rfloor":
                    return TexSymbolKind.FloorBracketClose;
                case "lceil":
                    return TexSymbolKind.CeilingBracketOpen;
                case "rceil":
                    return TexSymbolKind.CeilingBracketClose;
                case "|":
                    return TexSymbolKind.NormBracket;
                #endregion

                #region Relations
                case "neq":
                    return TexSymbolKind.NotEquals;
                case "doteq":
                    return TexSymbolKind.DotEquals;
                case "approx":
                    return TexSymbolKind.Approximates;
                case "equiv":
                    return TexSymbolKind.Equivalent;
                case "leq":
                    return TexSymbolKind.LessThanOrEqualTo;
                case "geq":
                    return TexSymbolKind.GreaterThanOrEqualTo;
                case "ll":
                    return TexSymbolKind.MuchLessThan;
                case "gg":
                    return TexSymbolKind.MuchGreaterThan;
                case "propto":
                    return TexSymbolKind.Proportional;
                case "asymp":
                    return TexSymbolKind.Asymptotic;
                case "bowtie":
                    return TexSymbolKind.Bowtie;
                case "models":
                    return TexSymbolKind.Models;
                case "prec":
                    return TexSymbolKind.Precedes;
                case "preceq":
                    return TexSymbolKind.PrecedesOrEquals;
                case "succ":
                    return TexSymbolKind.Succedes;
                case "succeq":
                    return TexSymbolKind.SuccedesOrEquals;
                case "cong":
                    return TexSymbolKind.Congruent;
                case "sim":
                    return TexSymbolKind.Similar;
                case "simeq":
                    return TexSymbolKind.SimilarOrEquals;
                case "perp":
                    return TexSymbolKind.Perpendicular;
                case "mid":
                    return TexSymbolKind.Middle;
                case "subset":
                    return TexSymbolKind.Subset;
                case "subseteq":
                    return TexSymbolKind.SubsetOrEqualTo;
                case "supset":
                    return TexSymbolKind.Superset;
                case "supseteq":
                    return TexSymbolKind.SupersetOrEqualTo;
                case "sqsubset":
                    return TexSymbolKind.SquareSubset;
                case "sqsubseteq":
                    return TexSymbolKind.SquareSubsetOrEqualTo;
                case "sqsupset":
                    return TexSymbolKind.SquareSuperset;
                case "sqsupseteq":
                    return TexSymbolKind.SquareSupersetOrEqualTo;
                case "in":
                    return TexSymbolKind.Member;
                case "nin":
                    return TexSymbolKind.NotMember;
                case "ni":
                    return TexSymbolKind.Contains;
                case "nni":
                    return TexSymbolKind.NotContains;
                case "smile":
                    return TexSymbolKind.Smile;
                case "frown":
                    return TexSymbolKind.Frown;
                case "vdash":
                    return TexSymbolKind.VLineDash;
                case "dashv":
                    return TexSymbolKind.DashVLine;
                #endregion

                #region Operators
                case "pm":
                    return TexSymbolKind.PlusMinus;
                case "mp":
                    return TexSymbolKind.MinusPlus;
                case "times":
                    return TexSymbolKind.Cross;
                case "cdot":
                    return TexSymbolKind.Dot;
                case "div":
                    return TexSymbolKind.Divide;
                case "over":
                    return TexSymbolKind.Over;
                #endregion

                #region Functions
                case "frac":
                    return TexSymbolKind.Fraction;
                case "binom":
                    return TexSymbolKind.Binomial;
                case "sqrt":
                    return TexSymbolKind.Root;
                case "min":
                    return TexSymbolKind.Minimum;
                case "max":
                    return TexSymbolKind.Maximum;
                case "gcd":
                    return TexSymbolKind.GreatestCommonDenominator;
                case "lcm":
                    return TexSymbolKind.LowestCommonMultiple;
                case "exp":
                    return TexSymbolKind.Exponent;
                case "log":
                    return TexSymbolKind.Log;
                case "ln":
                    return TexSymbolKind.NaturalLog;
                case "arg":
                    return TexSymbolKind.Argument;
                case "lim":
                    return TexSymbolKind.Limit;
                case "liminf":
                    return TexSymbolKind.LimitInferior;
                case "limsup":
                    return TexSymbolKind.LimitSuperior;
                case "sin":
                    return TexSymbolKind.Sine;
                case "cos":
                    return TexSymbolKind.Cosine;
                case "tan":
                    return TexSymbolKind.Tangent;
                case "sec":
                    return TexSymbolKind.Secant;
                case "csc":
                    return TexSymbolKind.Cosecant;
                case "cot":
                    return TexSymbolKind.Cotangent;
                case "arcsin":
                    return TexSymbolKind.ArcSine;
                case "arccos":
                    return TexSymbolKind.ArcCosine;
                case "arctan":
                    return TexSymbolKind.ArcTangent;
                case "arcsec":
                    return TexSymbolKind.ArcSecant;
                case "arccsc":
                    return TexSymbolKind.ArcCosecant;
                case "arccot":
                    return TexSymbolKind.ArcCotangent;
                case "sinh":
                    return TexSymbolKind.HypSine;
                case "cosh":
                    return TexSymbolKind.HypCosine;
                case "tanh":
                    return TexSymbolKind.HypTangent;
                case "sech":
                    return TexSymbolKind.HypSecant;
                case "csch":
                    return TexSymbolKind.HypCosecant;
                case "coth":
                    return TexSymbolKind.HypCotangent;
                case "arcsinh":
                    return TexSymbolKind.ArHypSine;
                case "arccosh":
                    return TexSymbolKind.ArHypCosine;
                case "arctanh":
                    return TexSymbolKind.ArHypTangent;
                case "arcsech":
                    return TexSymbolKind.ArHypSecant;
                case "arccsch":
                    return TexSymbolKind.ArHypCosecant;
                case "arccoth":
                    return TexSymbolKind.ArHypCotangent;
                case "bmod":
                    return TexSymbolKind.InlineModulo;
                case "pmod":
                    return TexSymbolKind.IdentityModulo;
                case "sum":
                    return TexSymbolKind.Sum;
                case "prod":
                    return TexSymbolKind.Product;
                case "coprod":
                    return TexSymbolKind.Coproduct;
                case "int":
                    return TexSymbolKind.Integral;
                case "iint":
                    return TexSymbolKind.DoubleIntegral;
                case "iiint":
                    return TexSymbolKind.TripleIntegral;
                case "iiiint":
                    return TexSymbolKind.QuadrupleIntegral;
                case "idotsint":
                    return TexSymbolKind.NtupleIntegral;
                case "oint":
                    return TexSymbolKind.ClosedIntegral;
                case "oiint":
                    return TexSymbolKind.ClosedDoubleIntegral;
                case "oiiint":
                    return TexSymbolKind.ClosedTripleIntegral;
                case "oiiiint":
                    return TexSymbolKind.ClosedQuadrupleIntegral;
                case "oidotsint":
                    return TexSymbolKind.ClosedNtupleIntegral;
                case "bigoplus":
                    return TexSymbolKind.BigOPlus;
                case "bigotimes":
                    return TexSymbolKind.BigOTimes;
                case "bigodot":
                    return TexSymbolKind.BigODot;
                case "bigcup":
                    return TexSymbolKind.BigCup;
                case "bigcap":
                    return TexSymbolKind.BigCap;
                case "bigcupplus":
                    return TexSymbolKind.BigCupPlus;
                case "bigsqcup":
                    return TexSymbolKind.BigSquareCup;
                case "bigsqcap":
                    return TexSymbolKind.BigSquareCap;
                case "bigveee":
                    return TexSymbolKind.BigVee;
                case "bigwedge":
                    return TexSymbolKind.BigWedge;
                #endregion

                #region Formatting
                case ",":
                    return TexSymbolKind.Separator;
                case "left":
                    return TexSymbolKind.Left;
                case "right":
                    return TexSymbolKind.Right;
                #endregion

                default:
                    if (this.IgnoreUnknownSymbols)
                        return TexSymbolKind.Unknown;
                    throw new LexerException(reader.Position, symbol, string.Format(
                        "Illegal symbol '{0}'.", symbol));
            }
        }
    }
}
