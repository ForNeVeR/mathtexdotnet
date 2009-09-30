using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public static class TexExpressionTreeBuilder
    {
        private const string errorMessageUnexpectedNumberOfChildren =
            "An parse node of kind {0} cannot have {1} children.";

        public static TexExpressionNode FromParseTree(ParseNode parseNode)
        {
            switch (parseNode.Kind)
            {
                case ParseNodeKind.InfixOperator:
                    return FromInfixOperatorParseNode(parseNode);
                case ParseNodeKind.PrefixOperator:
                    return FromPrefixCallParseNode(parseNode);
                case ParseNodeKind.PostfixOperator:
                    return FromPostfixOperatorParseNode(parseNode);
                case ParseNodeKind.Token:
                    return new TexExpressionNode(parseNode.Token.Symbol, parseNode.Token.Value);
                default:
                    throw new TexExpressionTreeBuilderException(parseNode, string.Format(
                        "Invalid parse node kind: {0}.", parseNode.Kind));
            }
        }

        public static TexExpressionNode FromInfixOperatorParseNode(ParseNode parseNode)
        {
            if (parseNode.Children.Count == 1)
            {
                return FromParseTree(parseNode.Children[0]);
            }
            else if (parseNode.Children.Count == 3)
            {
                if (parseNode.Children[1].Token.Symbol.IsLtrInfixOperator())
                    return FromLtrInfixOperatorParseNode(parseNode);
                else
                    return FromRtlInfixOperatorParseNode(parseNode);
            }

            throw new TexExpressionTreeBuilderException(parseNode, string.Format(
                errorMessageUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
        }

        public static TexExpressionNode FromLtrInfixOperatorParseNode(ParseNode parseNode)
        {
            var node = new TexExpressionNode(parseNode.Children[1].Token.Symbol);
            node.Children.Add(FromParseTree(parseNode.Children[0]));
            var secondOperandNode = FromParseTree(parseNode.Children[2]);
            if (parseNode.Children[2].IsSubExpression && secondOperandNode.Children.Count == 2)
            {
                node.Children.Add(secondOperandNode.Children[0]);
                secondOperandNode.Children[0] = node;
                return secondOperandNode;
            }
            else
            {
                node.Children.Add(secondOperandNode);
                return node;
            }
        }

        public static TexExpressionNode FromRtlInfixOperatorParseNode(ParseNode parseNode)
        {
            var node = new TexExpressionNode(parseNode.Children[1].Token.Symbol);
            node.Children.Add(FromParseTree(parseNode.Children[0]));
            node.Children.Add(FromParseTree(parseNode.Children[2]));
            return node;
        }

        public static TexExpressionNode FromPrefixCallParseNode(ParseNode parseNode)
        {
            if (parseNode.Children.Count == 1)
            {
                return FromParseTree(parseNode.Children[0]);
            }
            else if (parseNode.Children.Count == 2 && parseNode.Children[0].Token.Symbol == TexSymbolKind.Dot)
            {
                // Implicit multiplication with only one term.
                return FromParseTree(parseNode.Children[1]);
            }
            else if (parseNode.Children.Count >= 2)
            {
                var node = FromParseTree(parseNode.Children[0]);
                ParseNode childParseNode;
                TexExpressionNode childNode;
                for (int i = 1; i < parseNode.Children.Count; i++)
                {
                    childParseNode = parseNode.Children[i];
                    if (childParseNode.Kind == ParseNodeKind.Indices)
                    {
                        foreach (var argNode in FromPrefixOperatorIndicesParseNode(childParseNode))
                            node.Arguments.Add(argNode);
                        continue;
                    }
                    childNode = FromParseTree(childParseNode);
                    if (childNode != null)
                    {
                        if (childParseNode.IsArgument)
                            node.Arguments.Add(childNode);
                        else
                            node.Children.Add(childNode);
                    }
                }
                return node;
            }

            throw new TexExpressionTreeBuilderException(parseNode, string.Format(
                errorMessageUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));

        }

        public static TexExpressionNode FromPostfixOperatorParseNode(ParseNode parseNode)
        {
            if (parseNode.Children.Count == 1)
            {
                return FromParseTree(parseNode.Children[0]);
            }
            else if (parseNode.Children.Count == 2 && parseNode.Children[1].Kind == ParseNodeKind.Indices)
            {
                // Indices for value.
                return FromPostfixOperatorIndicesParseNode(parseNode.Children[1], parseNode.Children[0]);
            }
            else if (parseNode.Children.Count >= 2)
            {
                var node = FromParseTree(parseNode.Children[parseNode.Children.Count - 1]);
                ParseNode childParseNode;
                TexExpressionNode childNode;
                for (int i = 0; i < parseNode.Children.Count - 1; i++)
                {
                    childParseNode = parseNode.Children[i];
                    childNode = FromParseTree(childParseNode);
                    if (childNode != null)
                    {
                        node.Children.Add(childNode);
                    }
                }
                return node;
            }

            throw new TexExpressionTreeBuilderException(parseNode, string.Format(
                errorMessageUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
        }

        public static IEnumerable<TexExpressionNode> FromPrefixOperatorIndicesParseNode(ParseNode parseNode)
        {
            TexExpressionNode indexNode = null;
            if (parseNode.Children.Count == 0)
            {
                yield break;
            }
            else if (parseNode.Children.Count == 2)
            {
                indexNode = new TexExpressionNode(parseNode.Children[0].Token.Symbol);
                indexNode.Children.Add(FromParseTree(parseNode.Children[1]));
                yield return indexNode;
                yield break;
            }
            else if (parseNode.Children.Count == 4)
            {
                indexNode = new TexExpressionNode(parseNode.Children[0].Token.Symbol);
                indexNode.Children.Add(FromParseTree(parseNode.Children[1]));
                yield return indexNode;
                indexNode = new TexExpressionNode(parseNode.Children[2].Token.Symbol);
                indexNode.Children.Add(FromParseTree(parseNode.Children[3]));
                yield return indexNode;
                yield break;
            }

            throw new TexExpressionTreeBuilderException(parseNode, string.Format(
                errorMessageUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
        }

        public static TexExpressionNode FromPostfixOperatorIndicesParseNode(ParseNode parseNode, ParseNode childParseNode)
        {
            TexExpressionNode firstIndexNode = null;
            TexExpressionNode secondIndexNode = null;
            if (parseNode.Children.Count == 0)
            {
                return FromParseTree(childParseNode);
            }
            else if (parseNode.Children.Count == 2)
            {
                firstIndexNode = new TexExpressionNode(parseNode.Children[0].Token.Symbol);
            }
            else if (parseNode.Children.Count == 4)
            {
                firstIndexNode = new TexExpressionNode(parseNode.Children[0].Token.Symbol);
                secondIndexNode = new TexExpressionNode(parseNode.Children[2].Token.Symbol);
            }
            else
            {
                throw new TexExpressionTreeBuilderException(parseNode, string.Format(
                    errorMessageUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }

            Debug.Assert(firstIndexNode == null || firstIndexNode.Symbol == TexSymbolKind.RaiseToIndex ||
                firstIndexNode.Symbol == TexSymbolKind.LowerToIndex,
                "First index node is not RaiseToIndex or LowerToIndex.");
            Debug.Assert(secondIndexNode == null || secondIndexNode.Symbol == TexSymbolKind.RaiseToIndex ||
                secondIndexNode.Symbol == TexSymbolKind.LowerToIndex,
                "Second index node is not RaiseToIndex or LowerToIndex.");

            TexExpressionNode outerIndexNode;
            TexExpressionNode innerIndexNode;

            if (secondIndexNode == null)
            {
                outerIndexNode = firstIndexNode;
                innerIndexNode = firstIndexNode;
            }
            else
            {
                // Use raised index as outer exprNode.
                if (firstIndexNode.Symbol == TexSymbolKind.RaiseToIndex)
                {
                    outerIndexNode = firstIndexNode;
                    innerIndexNode = secondIndexNode;
                }
                else
                {
                    outerIndexNode = secondIndexNode;
                    innerIndexNode = firstIndexNode;
                }
                outerIndexNode.Children.Add(innerIndexNode);
            }

            // Add child nodes, and then nodes for indices.
            innerIndexNode.Children.Add(FromParseTree(childParseNode));
            firstIndexNode.Children.Add(FromParseTree(parseNode.Children[1]));
            if (secondIndexNode != null)
                secondIndexNode.Children.Add(FromParseTree(parseNode.Children[3]));

            return outerIndexNode;
        }
    }
}
