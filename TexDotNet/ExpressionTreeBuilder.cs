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
                var exprNode = new ExpressionNode(parent, parseNode.Children[1].Token.Symbol);
                exprNode.Children.Add(FromParseNode(exprNode, parseNode.Children[0]));
                exprNode.Children.Add(FromParseNode(exprNode, parseNode.Children[2]));
                return exprNode;
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
            else if (parseNode.Children.Count == 2 && parseNode.Children[0].Token.Symbol == SymbolKind.Dot)
            {
                // Implicit multiplication with only one term.
                return FromParseNode(parent, parseNode.Children[1]);
            }
            else if (parseNode.Children.Count >= 2)
            {
                var exprNode = FromParseNode(parent, parseNode.Children[0]);
                ParseNode childParseNode;
                ExpressionNode childExprNode;
                for (int i = 1; i < parseNode.Children.Count; i++)
                {
                    childParseNode = parseNode.Children[i];
                    if (childParseNode.Kind == ParseNodeKind.Indices)
                    {
                        foreach (var argNode in FromPrefixOperatorIndicesParseNode(exprNode, childParseNode))
                            exprNode.Arguments.Add(argNode);
                        continue;
                    }
                    childExprNode = FromParseNode(exprNode, childParseNode);
                    if (childExprNode != null)
                    {
                        if (childParseNode.IsArgument)
                            exprNode.Arguments.Add(childExprNode);
                        else
                            exprNode.Children.Add(childExprNode);
                    }
                }
                return exprNode;
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
            else if (parseNode.Children.Count == 2 && parseNode.Children[1].Kind == ParseNodeKind.Indices)
            {
                // Indices for value.
                return FromPostfixOperatorIndicesParseNode(parent, parseNode.Children[1], parseNode.Children[0]);
            }
            else if (parseNode.Children.Count >= 2)
            {
                var exprNode = FromParseNode(parent, parseNode.Children[parseNode.Children.Count - 1]);
                ParseNode childParseNode;
                ExpressionNode childExprNode;
                for (int i = 0; i < parseNode.Children.Count - 1; i++)
                {
                    childParseNode = parseNode.Children[i];
                    childExprNode = FromParseNode(exprNode, childParseNode);
                    if (childExprNode != null)
                    {
                        exprNode.Children.Add(childExprNode);
                    }
                }
                return exprNode;
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public static IEnumerable<ExpressionNode> FromPrefixOperatorIndicesParseNode(ExpressionNode parent,
            ParseNode parseNode)
        {
            ExpressionNode indexNode = null;
            if (parseNode.Children.Count == 0)
            {
                yield break;
            }
            else if (parseNode.Children.Count == 2)
            {
                indexNode = new ExpressionNode(parent, parseNode.Children[0].Token.Symbol);
                indexNode.Children.Add(FromParseNode(indexNode, parseNode.Children[1]));
                yield return indexNode;
            }
            else if (parseNode.Children.Count == 4)
            {
                indexNode = new ExpressionNode(parent, parseNode.Children[0].Token.Symbol);
                indexNode.Children.Add(FromParseNode(indexNode, parseNode.Children[1]));
                yield return indexNode;
                indexNode = new ExpressionNode(parent, parseNode.Children[2].Token.Symbol);
                indexNode.Children.Add(FromParseNode(indexNode, parseNode.Children[3]));
                yield return indexNode;
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgUnexpectedNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public static ExpressionNode FromPostfixOperatorIndicesParseNode(ExpressionNode parent, ParseNode parseNode,
            ParseNode childParseNode)
        {
            ExpressionNode firstIndexNode = null;
            ExpressionNode secondIndexNode = null;
            if (parseNode.Children.Count == 0)
            {
                return FromParseNode(parent, childParseNode);
            }
            else if (parseNode.Children.Count == 2)
            {
                firstIndexNode = new ExpressionNode(parent, parseNode.Children[0].Token.Symbol);
            }
            else if (parseNode.Children.Count == 4)
            {
                firstIndexNode = new ExpressionNode(parent, parseNode.Children[0].Token.Symbol);
                secondIndexNode = new ExpressionNode(parent, parseNode.Children[2].Token.Symbol);
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

            ExpressionNode outerIndexNode;
            ExpressionNode innerIndexNode;

            if (secondIndexNode == null)
            {
                outerIndexNode = firstIndexNode;
                innerIndexNode = firstIndexNode;
            }
            else
            {
                // Use raised index as outer exprNode.
                if (firstIndexNode.Symbol == SymbolKind.RaiseToIndex)
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
            innerIndexNode.Children.Add(FromParseNode(innerIndexNode, childParseNode));
            firstIndexNode.Children.Add(FromParseNode(firstIndexNode, parseNode.Children[1]));
            if (secondIndexNode != null)
                secondIndexNode.Children.Add(FromParseNode(secondIndexNode, parseNode.Children[3]));

            return outerIndexNode;
        }
    }
}
