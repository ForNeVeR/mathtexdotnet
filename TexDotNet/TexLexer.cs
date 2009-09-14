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
            yield return Token.FromKind(TokenKind.EndOfStream);
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

        protected TokenKind ScanShortSymbol(TrackedTextReader reader, out object value)
        {
            char chr = (char)reader.Read();
            value = null;
            switch (chr)
            {
                case '^':
                    return TokenKind.RaiseToIndex;
                case '_':
                    return TokenKind.LowerToIndex;
                case '\'':
                    return TokenKind.Prime;
                case ':':
                    return TokenKind.Colon;

                #region Relations
                case '=':
                    return TokenKind.Equals;
                case '<':
                    return TokenKind.LessThan;
                case '>':
                    return TokenKind.GreaterThan;
                #endregion

                #region Operators
                case '+':
                    return TokenKind.Plus;
                case '-':
                    return TokenKind.Minus;
                case '*':
                    return TokenKind.Dot;
                case '/':
                    return TokenKind.Divide;
                case '!':
                    return TokenKind.Factorial;
                #endregion

                #region Brackets
                case '{':
                    return TokenKind.GroupOpen;
                case '}':
                    return TokenKind.GroupClose;
                case '(':
                    return TokenKind.RoundBracketOpen;
                case ')':
                    return TokenKind.RoundBracketClose;
                case '[':
                    return TokenKind.SquareBracketOpen;
                case ']':
                    return TokenKind.SquareBracketClose;
                case '|':
                    return TokenKind.ModulusBracket;
                #endregion

                default:
                    value = chr;
                    return TokenKind.Letter;
                //throw new LexerException(reader.Position, string.Format(
                //    "Illegal character '{0}'.", chr));
            }
        }

        protected TokenKind ScanLongSymbol(TrackedTextReader reader, out object value)
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
                    return TokenKind.Number;
                case "text":
                    return TokenKind.Text;

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
                    return TokenKind.GreekLetter;
                #endregion

                #region Brackets
                case "{":
                    return TokenKind.CurlyBracketOpen;
                case "}":
                    return TokenKind.CurlyBracketClose;
                case "|":
                    return TokenKind.NormBracket;
                case "langle":
                    return TokenKind.AngleBracketOpen;
                case "rangle":
                    return TokenKind.AngleBracketClose;
                case "lfloor":
                    return TokenKind.FloorBracketOpen;
                case "rfloor":
                    return TokenKind.FloorBracketClose;
                case "lceil":
                    return TokenKind.CeilingBracketOpen;
                case "rceil":
                    return TokenKind.CeilingBracketClose;
                #endregion

                #region Relations
                case "neq":
                    return TokenKind.NotEquals;
                case "doteq":
                    return TokenKind.DotEquals;
                case "approx":
                    return TokenKind.Approximates;
                case "equiv":
                    return TokenKind.Equivalent;
                case "leq":
                    return TokenKind.LessThanOrEqualTo;
                case "geq":
                    return TokenKind.GreaterThanOrEqualTo;
                case "ll":
                    return TokenKind.MuchLessThan;
                case "gg":
                    return TokenKind.MuchGreaterThan;
                case "propto":
                    return TokenKind.Proportional;
                case "asymp":
                    return TokenKind.Asymptotic;
                case "bowtie":
                    return TokenKind.Bowtie;
                case "models":
                    return TokenKind.Models;
                case "prec":
                    return TokenKind.Precedes;
                case "preceq":
                    return TokenKind.PrecedesOrEquals;
                case "succ":
                    return TokenKind.Succedes;
                case "succeq":
                    return TokenKind.SuccedesOrEquals;
                case "cong":
                    return TokenKind.Congruent;
                case "sim":
                    return TokenKind.Similar;
                case "simeq":
                    return TokenKind.SimilarOrEquals;
                case "perp":
                    return TokenKind.Perpendicular;
                case "mid":
                    return TokenKind.Middle;
                case "subset":
                    return TokenKind.Subset;
                case "subseteq":
                    return TokenKind.SubsetOrEqualTo;
                case "supset":
                    return TokenKind.Superset;
                case "supseteq":
                    return TokenKind.SupersetOrEqualTo;
                case "sqsubset":
                    return TokenKind.SquareSubset;
                case "sqsubseteq":
                    return TokenKind.SquareSubsetOrEqualTo;
                case "sqsupset":
                    return TokenKind.SquareSuperset;
                case "sqsupseteq":
                    return TokenKind.SquareSupersetOrEqualTo;
                case "in":
                    return TokenKind.Member;
                case "nin":
                    return TokenKind.NotMember;
                case "ni":
                    return TokenKind.Contains;
                case "nni":
                    return TokenKind.NotContains;
                case "smile":
                    return TokenKind.Smile;
                case "frown":
                    return TokenKind.Frown;
                case "vdash":
                    return TokenKind.VLineDash;
                case "dashv":
                    return TokenKind.DashVLine;
                #endregion

                #region Operators
                case "pm":
                    return TokenKind.PlusMinus;
                case "mp":
                    return TokenKind.MinusPlus;
                case "times":
                    return TokenKind.Cross;
                case "cdot":
                    return TokenKind.Dot;
                case "div":
                    return TokenKind.Divide;
                case "frac":
                    return TokenKind.Fraction;
                case "sqrt":
                    return TokenKind.Root;
                case "sin":
                    return TokenKind.Sine;
                case "cos":
                    return TokenKind.Cosine;
                case "tan":
                    return TokenKind.Tangent;
                case "sec":
                    return TokenKind.Secant;
                case "csc":
                    return TokenKind.Cosecant;
                case "cot":
                    return TokenKind.Cotangent;
                case "arcsin":
                    return TokenKind.ArcSine;
                case "arccos":
                    return TokenKind.ArcCosine;
                case "arctan":
                    return TokenKind.ArcTangent;
                case "arcsec":
                    return TokenKind.ArcSecant;
                case "arccsc":
                    return TokenKind.ArcCosecant;
                case "arccot":
                    return TokenKind.ArcCotangent;
                case "sinh":
                    return TokenKind.Sine;
                case "cosh":
                    return TokenKind.Cosine;
                case "tanh":
                    return TokenKind.Tangent;
                case "sech":
                    return TokenKind.Secant;
                case "csch":
                    return TokenKind.Cosecant;
                case "coth":
                    return TokenKind.Cotangent;
                case "arcsinh":
                    return TokenKind.ArcSine;
                case "arccosh":
                    return TokenKind.ArcCosine;
                case "arctanh":
                    return TokenKind.ArcTangent;
                case "arcsech":
                    return TokenKind.ArcSecant;
                case "arccsch":
                    return TokenKind.ArcCosecant;
                case "arccoth":
                    return TokenKind.ArcCotangent;
                case "bmod":
                    return TokenKind.InlineModulo;
                case "pmod":
                    return TokenKind.IdentityModulo;
                case "sum":
                    return TokenKind.Sum;
                case "prod":
                    return TokenKind.Product;
                case "coprod":
                    return TokenKind.Coproduct;
                case "int":
                    return TokenKind.Integral;
                case "iint":
                    return TokenKind.DoubleIntegral;
                case "iiint":
                    return TokenKind.TripleIntegral;
                case "iiiint":
                    return TokenKind.QuadrupleIntegral;
                case "idotsint":
                    return TokenKind.NtupleIntegral;
                case "oint":
                    return TokenKind.ClosedIntegral;
                case "oiint":
                    return TokenKind.ClosedDoubleIntegral;
                case "oiiint":
                    return TokenKind.ClosedTripleIntegral;
                case "oiiiint":
                    return TokenKind.ClosedQuadrupleIntegral;
                case "oidotsint":
                    return TokenKind.ClosedNtupleIntegral;
                case "bigoplus":
                    return TokenKind.BigOPlus;
                case "bigotimes":
                    return TokenKind.BigOTimes;
                case "bigodot":
                    return TokenKind.BigODot;
                case "bigcup":
                    return TokenKind.BigCup;
                case "bigcap":
                    return TokenKind.BigCap;
                case "bigcupplus":
                    return TokenKind.BigCupPlus;
                case "bigsqcup":
                    return TokenKind.BigSquareCup;
                case "bigsqcap":
                    return TokenKind.BigSquareCap;
                case "bigveee":
                    return TokenKind.BigVee;
                case "bigwedge":
                    return TokenKind.BigWedge;
                #endregion

                #region Formatting
                case "left":
                    return TokenKind.Left;
                case "right":
                    return TokenKind.Right;
                #endregion

                default:
                    if (this.IgnoreUnknownSymbols)
                        return TokenKind.UnknownSymbol;
                    throw new LexerException(reader.Position, string.Format(
                        "Illegal symbol '{0}'.", symbol));
            }
        }
    }
}
