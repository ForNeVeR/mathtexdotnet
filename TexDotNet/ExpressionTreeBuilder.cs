using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public static class ExpressionTreeBuilder
    {
        private const string errMsgWrongNumberOfChildren =
            "An parse node of kind {0} cannot have {1} children.";

        public static ExpressionTree FromParseTree(ParseTree parseTree)
        {
            return new ExpressionTree(FromParseNode(null, parseTree.RootNode));
        }

        public static ExpressionNode FromParseNode(ExpressionNode parent, ParseNode parseNode)
        {
            switch (parseNode.Kind)
            {
                case ParseNodeKind.PrefixOperator:
                    return FromPrefixCallParseNode(parent, parseNode);
                case ParseNodeKind.InfixOperator:
                    return FromInfixOperatorParseNode(parent, parseNode);
                case ParseNodeKind.PostfixOperator:
                    return FromPostfixOperatorParseNode(parent, parseNode);
                case ParseNodeKind.Token:
                    return new ExpressionNode(parent, parseNode.Token.Symbol, parseNode.Token.Value);
                default:
                    throw new InvalidOperationException(string.Format(
                        "Invalid parse node kind: {0}.", parseNode.Kind));
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
                var node = new ExpressionNode(parent, parseNode.Children[0].Token.Symbol);
                ParseNode childParseNode;
                for (int i = 1; i < parseNode.Children.Count; i++)
                {
                    childParseNode = parseNode.Children[i];
                    if (childParseNode.IsArgument)
                        node.Arguments.Add(FromParseNode(node, childParseNode));
                    else
                        node.Children.Add(FromParseNode(node, childParseNode));
                }
                return node;
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
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
                    errMsgWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
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
                var node = new ExpressionNode(parent, parseNode.Children[parseNode.Children.Count - 1].Token.Symbol);
                ParseNode childParseNode;
                for (int i = 0; i < parseNode.Children.Count - 1; i++)
                {
                    childParseNode = parseNode.Children[i];
                    node.Children.Add(FromParseNode(node, childParseNode));
                }
                return node;
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }
    }
}
