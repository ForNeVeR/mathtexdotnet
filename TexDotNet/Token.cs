using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public struct Token
    {
        public static readonly Token Null = new Token(TokenKind.Null);

        public static Token FromNumber(double value)
        {
            return new Token(TokenKind.Number, value);
        }

        public static Token FromKind(TokenKind kind)
        {
            return new Token(kind);
        }

        public static Token FromValue(TokenKind kind, object value)
        {
            return new Token(kind, value);
        }

        public readonly TokenKind Kind;
        public readonly object Value;

        private Token(TokenKind kind)
            : this(kind, null)
        {
        }

        private Token(TokenKind kind, object value)
        {
            this.Kind = kind;
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Kind + (this.Value == null ? string.Empty :
                "(" + this.Value.ToString() + ")");
        }
    }

    public enum TokenKind
    {
        Null,
        EndOfStream,
        UnknownSymbol,

        Number,
        Letter,
        GreekLetter,
        Text,
        Prime,
        Colon,
        RaiseToIndex,
        LowerToIndex,

        #region Brackets
        GroupOpen,
        GroupClose,
        RoundBracketOpen,
        RoundBracketClose,
        SquareBracketOpen,
        SquareBracketClose,
        CurlyBracketOpen,
        CurlyBracketClose,
        AngleBracketOpen,
        AngleBracketClose,
        FloorBracketOpen,
        FloorBracketClose,
        CeilingBracketOpen,
        CeilingBracketClose,
        ModulusBracket,
        NormBracket,
        #endregion

        #region Relations
        Equals,
        NotEquals,
        DotEquals,
        Approximates,
        Equivalent,
        LessThan,
        LessThanOrEqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
        MuchLessThan,
        MuchGreaterThan,
        Proportional,
        Asymptotic,
        Bowtie,
        Models,
        Precedes,
        PrecedesOrEquals,
        Succedes,
        SuccedesOrEquals,
        Congruent,
        Similar,
        SimilarOrEquals,
        Perpendicular,
        Parallel,
        Middle,
        Subset,
        SubsetOrEqualTo,
        Superset,
        SupersetOrEqualTo,
        SquareSubset,
        SquareSubsetOrEqualTo,
        SquareSuperset,
        SquareSupersetOrEqualTo,
        Member,
        NotMember,
        Contains,
        NotContains,
        Smile,
        Frown,
        VLineDash,
        DashVLine,
        #endregion

        #region Operators
        Plus,
        Minus,
        PlusMinus,
        MinusPlus,
        Cross,
        Dot,
        Divide,
        Factorial,
        Fraction,
        Root,
        Sine,
        Cosine,
        Tangent,
        Secant,
        Cosecant,
        Cotangent,
        ArcSine,
        ArcCosine,
        ArcTangent,
        ArcSecant,
        ArcCosecant,
        ArcCotangent,
        HSine,
        HCosine,
        HTangent,
        HSecant,
        HCosecant,
        HCotangent,
        ArHSine,
        ArHCosine,
        ArHTangent,
        ArHSecant,
        ArHCosecant,
        ArHCotangent,
        InlineModulo,
        IdentityModulo,
        Sum,
        Product,
        Coproduct,
        Integral,
        DoubleIntegral,
        TripleIntegral,
        QuadrupleIntegral,
        NtupleIntegral,
        ClosedIntegral,
        ClosedDoubleIntegral,
        ClosedTripleIntegral,
        ClosedQuadrupleIntegral,
        ClosedNtupleIntegral,
        BigOPlus,
        BigOTimes,
        BigODot,
        BigCup,
        BigCap,
        BigCupPlus,
        BigSquareCup,
        BigSquareCap,
        BigVee,
        BigWedge,
        #endregion

        #region Formatting
        Left,
        Right,
        #endregion
    }
}
