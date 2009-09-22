using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    // TODO: Create associated TexExpressionTreeBuilderException class?
    public static class TexExpressionTreeBuilder
    {
        private const string errMsgUnexpectedNumberOfChildren =
            "An parse node of kind {0} cannot have {1} children.";

        public static TexExpressionNode FromParseTree(ParseNode parseTree)
        {
            switch (parseTree.Symbol)
            {
                case ParseNodeKind.InfixOperator:
                    return FromInfixOperatorParseNode(parseTree);
                case ParseNodeKind.PrefixOperator:
                    return FromPrefixCallParseNode(parseTree);
                case ParseNodeKind.PostfixOperator:
                    return FromPostfixOperatorParseNode(parseTree);
                case ParseNodeKind.Token:
                    return new TexExpressionNode(parseTree.Token.Symbol, parseTree.Token.Value);
                default:
                    throw new InvalidOperationException(string.Format(
                        "Invalid parse node kind: {0}.", parseTree.Symbol));
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
                var exprNode = new TexExpressionNode(parseNode.Children[1].Token.Symbol);
                exprNode.Children.Add(FromParseTree(parseNode.Children[0]));
                exprNode.Children.Add(FromParseTree(parseNode.Children[2]));
                return exprNode;
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgUnexpectedNumberOfChildren, parseNode.Symbol, parseNode.Children.Count));
            }
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
                var exprNode = FromParseTree(parseNode.Children[0]);
                ParseNode childParseNode;
                TexExpressionNode childExprNode;
                for (int i = 1; i < parseNode.Children.Count; i++)
                {
                    childParseNode = parseNode.Children[i];
                    if (childParseNode.Symbol == ParseNodeKind.Indices)
                    {
                        foreach (var argNode in FromPrefixOperatorIndicesParseNode(childParseNode))
                            exprNode.Arguments.Add(argNode);
                        continue;
                    }
                    childExprNode = FromParseTree(childParseNode);
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
                    errMsgUnexpectedNumberOfChildren, parseNode.Symbol, parseNode.Children.Count));
            }
        }

        public static TexExpressionNode FromPostfixOperatorParseNode(ParseNode parseNode)
        {
            if (parseNode.Children.Count == 1)
            {
                return FromParseTree(parseNode.Children[0]);
            }
            else if (parseNode.Children.Count == 2 && parseNode.Children[1].Symbol == ParseNodeKind.Indices)
            {
                // Indices for value.
                return FromPostfixOperatorIndicesParseNode(parseNode.Children[1], parseNode.Children[0]);
            }
            else if (parseNode.Children.Count >= 2)
            {
                var exprNode = FromParseTree(parseNode.Children[parseNode.Children.Count - 1]);
                ParseNode childParseNode;
                TexExpressionNode childExprNode;
                for (int i = 0; i < parseNode.Children.Count - 1; i++)
                {
                    childParseNode = parseNode.Children[i];
                    childExprNode = FromParseTree(childParseNode);
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
                    errMsgUnexpectedNumberOfChildren, parseNode.Symbol, parseNode.Children.Count));
            }
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
            }
            else if (parseNode.Children.Count == 4)
            {
                indexNode = new TexExpressionNode(parseNode.Children[0].Token.Symbol);
                indexNode.Children.Add(FromParseTree(parseNode.Children[1]));
                yield return indexNode;
                indexNode = new TexExpressionNode(parseNode.Children[2].Token.Symbol);
                indexNode.Children.Add(FromParseTree(parseNode.Children[3]));
                yield return indexNode;
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgUnexpectedNumberOfChildren, parseNode.Symbol, parseNode.Children.Count));
            }
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
                throw new InvalidOperationException(string.Format(
                    errMsgUnexpectedNumberOfChildren, parseNode.Symbol, parseNode.Children.Count));
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
