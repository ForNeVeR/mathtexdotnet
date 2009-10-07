using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public enum TexSymbolKind
    {
        Null,
        Unknown,
        EndOfStream,

        #region General
        Prime,
        Colon,
        Comma,
        #endregion

        #region Values
        Number,
        Letter,
        GreekLetter,
        Text,
        #endregion

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

        #region Relation Operators
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

        #region Bracketed Functions
        Fraction,
        Binomial,
        Root,
        #endregion

        #region Functions
        Minimum,
        Maximum,
        GreatestCommonDenominator,
        LowestCommonMultiple,
        Exponent,
        Log,
        NaturalLog,
        Argument,
        Limit,
        LimitInferior,
        LimitSuperior,
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
        HypSine,
        HypCosine,
        HypTangent,
        HypSecant,
        HypCosecant,
        HypCotangent,
        ArHypSine,
        ArHypCosine,
        ArHypTangent,
        ArHypSecant,
        ArHypCosecant,
        ArHypCotangent,
        InlineModulo,
        IdentityModulo,
        #endregion

        #region Big Operators
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

        #region Binary Operators
        Plus,
        Minus,
        PlusMinus,
        MinusPlus,
        Cross,
        Dot,
        Star,
        Divide,
        Over,
        RaiseToIndex,
        LowerToIndex,
        #endregion

        #region Postfix Operators
        Factorial,
        #endregion

        // Not used in expression trees.
        #region Formatting
        // Only used by writer
        Space,

        Separator,
        Left,
        Right,
        #endregion
    }
}
