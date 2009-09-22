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
        }

        public TokenStream Write(TexExpressionNode tree)
        {
            using (var tokenStream = new ComposedTokenStream())
            {
                WriteNode(tokenStream, tree);
                return tokenStream;
            }
        }

        private void WriteNode(ComposedTokenStream tokenStream, TexExpressionNode node)
        {
            var hasChildren = node.Children.Count > 0;
            if (hasChildren)
                tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.GroupOpen));
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
                    WriteInfixOperatorNode(tokenStream, node);
                    break;
                case TexSymbolKind.Fraction:
                case TexSymbolKind.Binomial:
                case TexSymbolKind.Root:
                    WriteBracketedFunction(tokenStream, node);
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
                    WritePrefixOperatorNode(tokenStream, node);
                    break;
                case TexSymbolKind.Factorial:
                    WritePostfixOperatorNode(tokenStream, node);
                    break;
                case TexSymbolKind.Number:
                case TexSymbolKind.Letter:
                case TexSymbolKind.GreekLetter:
                case TexSymbolKind.Text:
                    WriteValueNode(tokenStream, node);
                    break;
                default:
                    throw new TexComposerException(node,
                        "Unrecognised node symbol.");
            }
            if (hasChildren)
                tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.GroupClose));
        }

        private void WriteBracketedFunction(ComposedTokenStream tokenStream, TexExpressionNode node)
        {
            if (node.Children.Count >= 1)
            {
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                foreach (var argNode in node.Arguments)
                {
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.SquareBracketOpen));
                    WriteNode(tokenStream, argNode);
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.SquareBracketClose));
                }
                foreach (var childNode in node.Children)
                {
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.GroupOpen));
                    WriteNode(tokenStream, childNode);
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.GroupClose));
                }
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errMsgUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WriteInfixOperatorNode(ComposedTokenStream tokenStream, TexExpressionNode node)
        {
            if (node.Children.Count == 1)
            {
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                WriteNode(tokenStream, node.Children[0]);
            }
            else if (node.Children.Count == 2)
            {
                WriteNode(tokenStream, node.Children[0]);
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                WriteNode(tokenStream, node.Children[1]);
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errMsgUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WritePrefixOperatorNode(ComposedTokenStream tokenStream, TexExpressionNode node)
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
                    WriteNode(tokenStream, argNode.Children[0]);
                }
                foreach (var childNode in node.Children)
                    WriteNode(tokenStream, childNode);
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errMsgUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WritePostfixOperatorNode(ComposedTokenStream tokenStream, TexExpressionNode node)
        {
            if (node.Children.Count >= 1)
            {
                foreach (var childNode in node.Children)
                    WriteNode(tokenStream, childNode);
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errMsgUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WriteValueNode(ComposedTokenStream tokenStream, TexExpressionNode node)
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
    }
}
