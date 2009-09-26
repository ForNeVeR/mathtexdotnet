using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public class TexComposer
    {
        private const string errMsgUnexpectedNumberOfChildren =
            "An parse node of kind {0} cannot have {1} children.";

        public TexComposer()
        {
            this.PadPlusAndMinusSigns = true;
        }

        public bool PadPlusAndMinusSigns
        {
            get;
            set;
        }

        public TokenStream Write(TexExpressionNode tree)
        {
            using (var tokenStream = new ComposedTokenStream())
            {
                WriteNode(tokenStream, tree, new ComposerState());
                return tokenStream;
            }
        }

        private void WriteNode(ComposedTokenStream tokenStream, TexExpressionNode node, ComposerState state)
        {
            var openBracketSymbol = TexSymbolKind.Null;
            var closeBracketSymbol = TexSymbolKind.Null;
            switch (node.Symbol)
            {
                case TexSymbolKind.Plus:
                case TexSymbolKind.Minus:
                case TexSymbolKind.PlusMinus:
                case TexSymbolKind.MinusPlus:
                case TexSymbolKind.Dot:
                case TexSymbolKind.Cross:
                case TexSymbolKind.Star:
                case TexSymbolKind.Divide:
                case TexSymbolKind.Over:
                case TexSymbolKind.InlineModulo:
                    if (node.Parent != null && node.Symbol != node.Parent.Symbol)
                    {
                        switch (node.Parent.Symbol)
                        {
                            case TexSymbolKind.Plus:
                            case TexSymbolKind.Minus:
                            case TexSymbolKind.PlusMinus:
                            case TexSymbolKind.MinusPlus:
                            case TexSymbolKind.Dot:
                            case TexSymbolKind.Cross:
                            case TexSymbolKind.Star:
                            case TexSymbolKind.Divide:
                            case TexSymbolKind.Over:
                            case TexSymbolKind.InlineModulo:
                                openBracketSymbol = TexSymbolKind.RoundBracketOpen;
                                closeBracketSymbol = TexSymbolKind.RoundBracketClose;
                                break;
                        }
                    }
                    break;
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
                    openBracketSymbol = TexSymbolKind.GroupOpen;
                    closeBracketSymbol = TexSymbolKind.GroupClose;
                    break;
                case TexSymbolKind.Number:
                case TexSymbolKind.Letter:
                case TexSymbolKind.GreekLetter:
                    break;
                default:
                    break;
            }
            if (openBracketSymbol == TexSymbolKind.Null)
            {
                if (node.Parent != null)
                {
                    switch (node.Parent.Symbol)
                    {
                        case TexSymbolKind.LowerToIndex:
                        case TexSymbolKind.RaiseToIndex:
                            // Surround expression with group if node is second operand, or node has more than one
                            // child.
                            if ((node.Parent.Children.Count == 2 && node.Parent.Children.IndexOf(node) == 1) ||
                                node.Children.Count > 1)
                            {
                                openBracketSymbol = TexSymbolKind.GroupOpen;
                                closeBracketSymbol = TexSymbolKind.GroupClose;
                            }
                            break;
                        case TexSymbolKind.Factorial:
                        case TexSymbolKind.IdentityModulo:
                            if (node.Parent.Children.Count == 1)
                            {
                                openBracketSymbol = TexSymbolKind.GroupOpen;
                                closeBracketSymbol = TexSymbolKind.GroupClose;
                            }
                            break;
                    }
                }
            }
            //if (state.IsParentNodeGroupOpen && node.Parent.Children.Count == 1)
            //{
            //    openBracketSymbol = TexSymbolKind.Null;
            //    closeBracketSymbol = TexSymbolKind.Null;
            //    state.IsParentNodeGroupOpen = false;
            //}
            else if (!state.IsParentNodeGroupOpen && openBracketSymbol == TexSymbolKind.GroupOpen)
            {
                state.IsParentNodeGroupOpen = true;
            }

            if (openBracketSymbol != TexSymbolKind.Null)
                tokenStream.Write(TexToken.FromSymbol(openBracketSymbol));

            switch (node.Symbol)
            {
                case TexSymbolKind.Plus:
                case TexSymbolKind.Minus:
                case TexSymbolKind.Dot:
                case TexSymbolKind.Cross:
                case TexSymbolKind.Star:
                case TexSymbolKind.Divide:
                case TexSymbolKind.Over:
                case TexSymbolKind.RaiseToIndex:
                case TexSymbolKind.LowerToIndex:
                case TexSymbolKind.InlineModulo:
                    WriteInfixOperatorNode(tokenStream, node, state);
                    break;
                case TexSymbolKind.Fraction:
                case TexSymbolKind.Binomial:
                case TexSymbolKind.Root:
                    WriteBracketedFunction(tokenStream, node, state);
                    break;
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
                    WritePrefixOperatorNode(tokenStream, node, state);
                    break;
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
                    WritePrefixOperatorNode(tokenStream, node, state);
                    break;
                case TexSymbolKind.Factorial:
                case TexSymbolKind.IdentityModulo:
                    WritePostfixOperatorNode(tokenStream, node, state);
                    break;
                case TexSymbolKind.Number:
                case TexSymbolKind.Letter:
                case TexSymbolKind.GreekLetter:
                case TexSymbolKind.Text:
                    WriteValueNode(tokenStream, node, state);
                    break;
                default:
                    throw new TexComposerException(node,
                        "Unrecognised node symbol.");
            }

            if (closeBracketSymbol != TexSymbolKind.Null)
                tokenStream.Write(TexToken.FromSymbol(closeBracketSymbol));
        }

        private void WriteBracketedFunction(ComposedTokenStream tokenStream, TexExpressionNode node,
            ComposerState state)
        {
            if (node.Children.Count >= 1)
            {
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                foreach (var argNode in node.Arguments)
                {
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.SquareBracketOpen));
                    state.IsParentNodeGroupOpen = true;
                    WriteNode(tokenStream, argNode, state);
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.SquareBracketClose));
                }
                foreach (var childNode in node.Children)
                {
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.GroupOpen));
                    state.IsParentNodeGroupOpen = true;
                    WriteNode(tokenStream, childNode, state);
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.GroupClose));
                }
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errMsgUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WriteInfixOperatorNode(ComposedTokenStream tokenStream, TexExpressionNode node,
            ComposerState state)
        {
            if (node.Children.Count == 1)
            {
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                WriteNode(tokenStream, node.Children[0], state);
            }
            else if (node.Children.Count == 2)
            {
                WriteNode(tokenStream, node.Children[0], state);

                var padSymbol = ((this.PadPlusAndMinusSigns && (node.Symbol == TexSymbolKind.Plus ||
                    node.Symbol == TexSymbolKind.Minus))) || node.Symbol.IsLongOperator();
                if (node.Symbol == TexSymbolKind.Dot)
                {
                    // If terms can be multipled implicitly, do not write operator token.
                    switch (node.Children[1].Symbol)
                    {
                        case TexSymbolKind.Number:
                        case TexSymbolKind.Text:
                            tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (padSymbol)
                        tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.Space));
                    tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                }
                if (padSymbol)
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.Space));

                WriteNode(tokenStream, node.Children[1], state);
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errMsgUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WritePrefixOperatorNode(ComposedTokenStream tokenStream, TexExpressionNode node,
            ComposerState state)
        {
            if (node.Children.Count >= 1)
            {
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                foreach (var argNode in node.Arguments)
                {
                    if (argNode.Children.Count != 1)
                        throw new TexComposerException(argNode, string.Format(
                            errMsgUnexpectedNumberOfChildren, argNode.Symbol, argNode.Children.Count));
                    tokenStream.Write(TexToken.FromSymbol(argNode.Symbol));
                    WriteNode(tokenStream, argNode.Children[0], state);
                }
                tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.Space));
                foreach (var childNode in node.Children)
                    WriteNode(tokenStream, childNode, state);
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errMsgUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WritePostfixOperatorNode(ComposedTokenStream tokenStream, TexExpressionNode node,
            ComposerState state)
        {
            if (node.Children.Count >= 1)
            {
                foreach (var childNode in node.Children)
                    WriteNode(tokenStream, childNode, state);
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errMsgUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WriteValueNode(ComposedTokenStream tokenStream, TexExpressionNode node,
            ComposerState state)
        {
            tokenStream.Write(TexToken.FromValue(node.Symbol, node.Value));
        }

        private class ComposedTokenStream : TokenStream
        {
            private LinkedList<TexToken> tokenList;
            private LinkedListNode<TexToken> curToken;

            public ComposedTokenStream()
            {
                this.tokenList = new LinkedList<TexToken>();
                this.curToken = null;
            }

            internal LinkedList<TexToken> TokenList
            {
                get { return this.tokenList; }
            }

            public TexToken Current
            {
                get { return this.curToken.Value; }
            }

            object IEnumerator.Current
            {
                get { return this.curToken.Value; }
            }

            public void Dispose()
            {
            }

            public void Write(TexToken token)
            {
                this.tokenList.AddLast(token);
            }

            public bool MoveNext()
            {
                LinkedListNode<TexToken> nextTokenNode;
                if (this.curToken == null)
                    nextTokenNode = this.tokenList.First;
                else
                    nextTokenNode = this.curToken.Next;
                if (nextTokenNode != null)
                    this.curToken = nextTokenNode;
                return nextTokenNode != null;
            }

            public void Reset()
            {
                this.curToken = this.tokenList.First;
            }
        }

        private struct ComposerState
        {
            public bool IsParentNodeGroupOpen;
        }
    }
}
