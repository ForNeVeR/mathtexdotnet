using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public static class ExpressionTreeBuilder
    {
        private const string errMsgUnexpectedNumberOfChildren =
            "An parse node of kind {0} cannot have {1} children.";

        public static ExpressionTree FromParseTree(ParseTree parseTree)
        {
            return new ExpressionTree(FromParseNode(null, parseTree.RootNode));
        }

        public static ExpressionNode FromParseNode(ExpressionNode parent, ParseNode parseNode)
        {
            switch (parseNode.Kind)
            {
                case ParseNodeKind.InfixOperator:
                    return FromInfixOperatorParseNode(parent, parseNode);
                case ParseNodeKind.PrefixOperator:
                    return FromPrefixCallParseNode(parent, parseNode);
                case ParseNodeKind.PostfixOperator:
                    return FromPostfixOperatorParseNode(parent, parseNode);
                case ParseNodeKind.Indices:
                    return FromIndicesParseNode(parent, parseNode);
                case ParseNodeKind.Token:
                    return new ExpressionNode(parent, parseNode.Token.Symbol, parseNode.Token.Value);
                default:
                    throw new InvalidOperationException(string.Format(
                        "Invalid parse node kind: {0}.", parseNode.Kind));
            }
        }

        public static ExpressionNode FromInfixOperatorParseNode(ExpressionNode parent, ParseNode parseNode)
        {
            if (parseNode.Children.Count == 1)
            {
                return FromParseNode(parent, parseNode.Children[0]);
            }
            else if (parseNode.Children.Count == 3)
            {
                var node = new ExpressionNode(parent, parseNode.Children[1].Token.Symbol);
                node.Children.Add(FromParseNode(node, parseNode.Children[0]));
                node.Children.Add(FromParseNode(node, parseNode.Children[2]));
                return node;
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public static ExpressionNode FromPrefixCallParseNode(ExpressionNode parent, ParseNode parseNode)
        {
            if (parseNode.Children.Count == 1)
            {
                return FromParseNode(parent, parseNode.Children[0]);
            }
            else if (parseNode.Children.Count >= 2)
            {
                var node = FromParseNode(parent, parseNode.Children[0]);
                if (node == null && parseNode.Children.Count == 2)
                    return FromParseNode(parent, parseNode.Children[1]);

                ParseNode childParseNode;
                ExpressionNode childExprNode;
                for (int i = 1; i < parseNode.Children.Count; i++)
                {
                    childParseNode = parseNode.Children[i];
                    childExprNode = FromParseNode(node, childParseNode);
                    if (childExprNode != null)
                    {
                        if (childParseNode.IsArgument)
                            node.Arguments.Add(childExprNode);
                        else
                            node.Children.Add(childExprNode);
                    }
                }
                return node;
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public static ExpressionNode FromPostfixOperatorParseNode(ExpressionNode parent, ParseNode parseNode)
        {
            if (parseNode.Children.Count == 1)
            {
                return FromParseNode(parent, parseNode.Children[0]);
            }
            else if (parseNode.Children.Count == 2 &&
                parseNode.Children[parseNode.Children.Count - 1].Token.Symbol == SymbolKind.Dot)
            {
                return FromParseNode(parent, parseNode.Children[0]);
            }
            else if (parseNode.Children.Count >= 2)
            {
                var node = FromParseNode(parent, parseNode.Children[parseNode.Children.Count - 1]);
                if (node == null && parseNode.Children.Count == 2)
                    return FromParseNode(parent, parseNode.Children[0]);

                // Handle index nodes specially: add children to inner index node.
                var innerNode = node.Symbol == SymbolKind.RaiseToIndex && node.Children.Count == 2 ?
                    node.Children[1] : node;
                ParseNode childParseNode;
                ExpressionNode childExprNode;
                for (int i = 0; i < parseNode.Children.Count - 1; i++)
                {
                    childParseNode = parseNode.Children[i];
                    childExprNode = FromParseNode(node, childParseNode);
                    if (childExprNode != null)
                    {
                        innerNode.Children.Add(childExprNode);
                    }
                }
                return node;
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public static ExpressionNode FromIndicesParseNode(ExpressionNode parent, ParseNode parseNode)
        {
            ExpressionNode firstIndexNode = null;
            ExpressionNode secondIndexNode = null;
            if (parseNode.Children.Count == 0)
            {
                return null;
            }
            else if (parseNode.Children.Count == 2)
            {
                firstIndexNode = new ExpressionNode(parent, parseNode.Children[0].Token.Symbol);
                firstIndexNode.Children.Add(FromParseNode(parent, parseNode.Children[1]));
            }
            else if (parseNode.Children.Count == 4)
            {
                firstIndexNode = new ExpressionNode(parent, parseNode.Children[0].Token.Symbol);
                firstIndexNode.Children.Add(FromParseNode(parent, parseNode.Children[1]));
                secondIndexNode = new ExpressionNode(parent, parseNode.Children[2].Token.Symbol);
                firstIndexNode.Children.Add(FromParseNode(parent, parseNode.Children[3]));
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }

            Debug.Assert(firstIndexNode == null || firstIndexNode.Symbol == SymbolKind.RaiseToIndex ||
                firstIndexNode.Symbol == SymbolKind.LowerToIndex,
                "First index node is not RaiseToIndex or LowerToIndex.");
            Debug.Assert(secondIndexNode == null || secondIndexNode.Symbol == SymbolKind.RaiseToIndex ||
                secondIndexNode.Symbol == SymbolKind.LowerToIndex,
                "Second index node is not RaiseToIndex or LowerToIndex.");

            if (secondIndexNode == null)
            {
                return firstIndexNode;
            }
            else
            {
                // Use raised index as outer node.
                if (firstIndexNode.Symbol == SymbolKind.RaiseToIndex)
                {
                    firstIndexNode.Children.Add(secondIndexNode);
                    return firstIndexNode;
                }
                else
                {
                    secondIndexNode.Children.Add(firstIndexNode);
                    return secondIndexNode;
                }
            }
        }
    }
}
