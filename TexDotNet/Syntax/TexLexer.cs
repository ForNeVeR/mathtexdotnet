using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<Token>;

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
                    yield return Token.FromNumber(ScanReal(reader));
                }
                else if (nextChar == '\\')
                {
                    object value;
                    yield return Token.FromValue(ScanLongSymbol(reader, out value), value);
                }
                else
                {
                    object value;
                    yield return Token.FromValue(ScanShortSymbol(reader, out value), value);
                }
            }
            yield return Token.FromKind(SymbolKind.EndOfStream);
        }

        protected double ScanReal(TrackedTextReader reader)
        {
            var sb = new StringBuilder();
            while (Char.IsDigit((char)reader.Peek()))
                sb.Append((char)reader.Read());
            if (reader.Peek() == '.')
            {
                sb.Append(reader.Read());
                while (Char.IsDigit((char)reader.Peek()))
                    sb.Append((char)reader.Read());
            }
            return double.Parse(sb.ToString(), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture.NumberFormat);
        }

        protected SymbolKind ScanShortSymbol(TrackedTextReader reader, out object value)
        {
            char chr = (char)reader.Read();
            value = null;
            switch (chr)
            {
                case '^':
                    return SymbolKind.RaiseToIndex;
                case '_':
                    return SymbolKind.LowerToIndex;
                case '\'':
                    return SymbolKind.Prime;
                case ':':
                    return SymbolKind.Colon;

                #region Relations
                case '=':
                    return SymbolKind.Equals;
                case '<':
                    return SymbolKind.LessThan;
                case '>':
                    return SymbolKind.GreaterThan;
                #endregion

                #region Operators
                case '+':
                    return SymbolKind.Plus;
                case '-':
                    return SymbolKind.Minus;
                case '*':
                    return SymbolKind.Star;
                case '/':
                    return SymbolKind.Divide;
                case '!':
                    return SymbolKind.Factorial;
                #endregion

                #region Brackets
                case '{':
                    return SymbolKind.GroupOpen;
                case '}':
                    return SymbolKind.GroupClose;
                case '(':
                    return SymbolKind.RoundBracketOpen;
                case ')':
                    return SymbolKind.RoundBracketClose;
                case '[':
                    return SymbolKind.SquareBracketOpen;
                case ']':
                    return SymbolKind.SquareBracketClose;
                case '|':
                    return SymbolKind.ModulusBracket;
                #endregion

                default:
                    value = chr;
                    return SymbolKind.Letter;
                //throw new LexerException(reader.Position, string.Format(
                //    "Illegal character '{0}'.", chr));
            }
        }

        protected SymbolKind ScanLongSymbol(TrackedTextReader reader, out object value)
        {
            var sb = new StringBuilder();
            char nextChr;
            while (reader.Peek() != -1)
            {
                nextChr = (char)reader.Peek();
                if (char.IsWhiteSpace(nextChr))
                    break;
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
                    return SymbolKind.Number;
                case "text":
                    return SymbolKind.Text;

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
                    return SymbolKind.GreekLetter;
                #endregion

                #region Brackets
                case "{":
                    return SymbolKind.CurlyBracketOpen;
                case "}":
                    return SymbolKind.CurlyBracketClose;
                case "|":
                    return SymbolKind.NormBracket;
                case "langle":
                    return SymbolKind.AngleBracketOpen;
                case "rangle":
                    return SymbolKind.AngleBracketClose;
                case "lfloor":
                    return SymbolKind.FloorBracketOpen;
                case "rfloor":
                    return SymbolKind.FloorBracketClose;
                case "lceil":
                    return SymbolKind.CeilingBracketOpen;
                case "rceil":
                    return SymbolKind.CeilingBracketClose;
                #endregion

                #region Relations
                case "neq":
                    return SymbolKind.NotEquals;
                case "doteq":
                    return SymbolKind.DotEquals;
                case "approx":
                    return SymbolKind.Approximates;
                case "equiv":
                    return SymbolKind.Equivalent;
                case "leq":
                    return SymbolKind.LessThanOrEqualTo;
                case "geq":
                    return SymbolKind.GreaterThanOrEqualTo;
                case "ll":
                    return SymbolKind.MuchLessThan;
                case "gg":
                    return SymbolKind.MuchGreaterThan;
                case "propto":
                    return SymbolKind.Proportional;
                case "asymp":
                    return SymbolKind.Asymptotic;
                case "bowtie":
                    return SymbolKind.Bowtie;
                case "models":
                    return SymbolKind.Models;
                case "prec":
                    return SymbolKind.Precedes;
                case "preceq":
                    return SymbolKind.PrecedesOrEquals;
                case "succ":
                    return SymbolKind.Succedes;
                case "succeq":
                    return SymbolKind.SuccedesOrEquals;
                case "cong":
                    return SymbolKind.Congruent;
                case "sim":
                    return SymbolKind.Similar;
                case "simeq":
                    return SymbolKind.SimilarOrEquals;
                case "perp":
                    return SymbolKind.Perpendicular;
                case "mid":
                    return SymbolKind.Middle;
                case "subset":
                    return SymbolKind.Subset;
                case "subseteq":
                    return SymbolKind.SubsetOrEqualTo;
                case "supset":
                    return SymbolKind.Superset;
                case "supseteq":
                    return SymbolKind.SupersetOrEqualTo;
                case "sqsubset":
                    return SymbolKind.SquareSubset;
                case "sqsubseteq":
                    return SymbolKind.SquareSubsetOrEqualTo;
                case "sqsupset":
                    return SymbolKind.SquareSuperset;
                case "sqsupseteq":
                    return SymbolKind.SquareSupersetOrEqualTo;
                case "in":
                    return SymbolKind.Member;
                case "nin":
                    return SymbolKind.NotMember;
                case "ni":
                    return SymbolKind.Contains;
                case "nni":
                    return SymbolKind.NotContains;
                case "smile":
                    return SymbolKind.Smile;
                case "frown":
                    return SymbolKind.Frown;
                case "vdash":
                    return SymbolKind.VLineDash;
                case "dashv":
                    return SymbolKind.DashVLine;
                #endregion

                #region Operators
                case "pm":
                    return SymbolKind.PlusMinus;
                case "mp":
                    return SymbolKind.MinusPlus;
                case "times":
                    return SymbolKind.Cross;
                case "cdot":
                    return SymbolKind.Dot;
                case "div":
                    return SymbolKind.Divide;
                case "frac":
                    return SymbolKind.Fraction;
                case "sqrt":
                    return SymbolKind.Root;
                case "sin":
                    return SymbolKind.Sine;
                case "cos":
                    return SymbolKind.Cosine;
                case "tan":
                    return SymbolKind.Tangent;
                case "sec":
                    return SymbolKind.Secant;
                case "csc":
                    return SymbolKind.Cosecant;
                case "cot":
                    return SymbolKind.Cotangent;
                case "arcsin":
                    return SymbolKind.ArcSine;
                case "arccos":
                    return SymbolKind.ArcCosine;
                case "arctan":
                    return SymbolKind.ArcTangent;
                case "arcsec":
                    return SymbolKind.ArcSecant;
                case "arccsc":
                    return SymbolKind.ArcCosecant;
                case "arccot":
                    return SymbolKind.ArcCotangent;
                case "sinh":
                    return SymbolKind.Sine;
                case "cosh":
                    return SymbolKind.Cosine;
                case "tanh":
                    return SymbolKind.Tangent;
                case "sech":
                    return SymbolKind.Secant;
                case "csch":
                    return SymbolKind.Cosecant;
                case "coth":
                    return SymbolKind.Cotangent;
                case "arcsinh":
                    return SymbolKind.ArcSine;
                case "arccosh":
                    return SymbolKind.ArcCosine;
                case "arctanh":
                    return SymbolKind.ArcTangent;
                case "arcsech":
                    return SymbolKind.ArcSecant;
                case "arccsch":
                    return SymbolKind.ArcCosecant;
                case "arccoth":
                    return SymbolKind.ArcCotangent;
                case "bmod":
                    return SymbolKind.InlineModulo;
                case "pmod":
                    return SymbolKind.IdentityModulo;
                case "sum":
                    return SymbolKind.Sum;
                case "prod":
                    return SymbolKind.Product;
                case "coprod":
                    return SymbolKind.Coproduct;
                case "int":
                    return SymbolKind.Integral;
                case "iint":
                    return SymbolKind.DoubleIntegral;
                case "iiint":
                    return SymbolKind.TripleIntegral;
                case "iiiint":
                    return SymbolKind.QuadrupleIntegral;
                case "idotsint":
                    return SymbolKind.NtupleIntegral;
                case "oint":
                    return SymbolKind.ClosedIntegral;
                case "oiint":
                    return SymbolKind.ClosedDoubleIntegral;
                case "oiiint":
                    return SymbolKind.ClosedTripleIntegral;
                case "oiiiint":
                    return SymbolKind.ClosedQuadrupleIntegral;
                case "oidotsint":
                    return SymbolKind.ClosedNtupleIntegral;
                case "bigoplus":
                    return SymbolKind.BigOPlus;
                case "bigotimes":
                    return SymbolKind.BigOTimes;
                case "bigodot":
                    return SymbolKind.BigODot;
                case "bigcup":
                    return SymbolKind.BigCup;
                case "bigcap":
                    return SymbolKind.BigCap;
                case "bigcupplus":
                    return SymbolKind.BigCupPlus;
                case "bigsqcup":
                    return SymbolKind.BigSquareCup;
                case "bigsqcap":
                    return SymbolKind.BigSquareCap;
                case "bigveee":
                    return SymbolKind.BigVee;
                case "bigwedge":
                    return SymbolKind.BigWedge;
                #endregion

                #region Formatting
                case "left":
                    return SymbolKind.Left;
                case "right":
                    return SymbolKind.Right;
                #endregion

                default:
                    if (this.IgnoreUnknownSymbols)
                        return SymbolKind.Unknown;
                    throw new LexerException(reader.Position, string.Format(
                        "Illegal symbol '{0}'.", symbol));
            }
        }
    }
}
