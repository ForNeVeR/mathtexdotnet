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
            return new ExpressionTree(FromParseNode(parseTree.RootNode));
        }

        public static ExpressionNode FromParseNode(ParseNode parseNode)
        {
            switch (parseNode.Kind)
            {
                case ParseNodeKind.PrefixOperator:
                    return FromPrefixCallParseNode(parseNode);
                case ParseNodeKind.InfixOperator:
                    return FromInfixOperatorParseNode(parseNode);
                case ParseNodeKind.PostfixOperator:
                    return FromPostfixOperatorParseNode(parseNode);
                case ParseNodeKind.Token:
                    return new ExpressionNode(parseNode.Token.Symbol, parseNode.Token.Value);
                default:
                    throw new InvalidOperationException(string.Format(
                        "Invalid parse node kind: {0}.", parseNode.Kind));
            }
        }

        public static ExpressionNode FromPrefixCallParseNode(ParseNode parseNode)
        {
            if (parseNode.Children.Count == 1)
            {
                return FromParseNode(parseNode.Children[0]);
            }
            else if (parseNode.Children.Count >= 2)
            {
                var children = new List<ExpressionNode>(parseNode.Children.Count - 1);
                var arguments = new List<ExpressionNode>(parseNode.Children.Count - 1);
                ParseNode childParseNode;
                for (int i = 1; i < parseNode.Children.Count; i++)
                {
                    childParseNode = parseNode.Children[i];
                    if (childParseNode.IsArgument)
                        arguments.Add(FromParseNode(childParseNode));
                    else
                        children.Add(FromParseNode(childParseNode));
                }
                return new ExpressionNode(parseNode.Children[0].Token.Symbol, children, arguments);
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public static ExpressionNode FromInfixOperatorParseNode(ParseNode parseNode)
        {
            if (parseNode.Children.Count == 1)
            {
                return FromParseNode(parseNode.Children[0]);
            }
            else if (parseNode.Children.Count == 3)
            {
                var node = new ExpressionNode(parseNode.Children[1].Token.Symbol, new[] {
                    FromParseNode(parseNode.Children[0]), FromParseNode(parseNode.Children[2])});
                return node;
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public static ExpressionNode FromPostfixOperatorParseNode(ParseNode parseNode)
        {
            if (parseNode.Children.Count == 1)
            {
                return FromParseNode(parseNode.Children[0]);
            }
            else if (parseNode.Children.Count >= 2)
            {
                var children = new List<ExpressionNode>(parseNode.Children.Count - 1);
                ParseNode childParseNode;
                for (int i = 0; i < parseNode.Children.Count - 1; i++)
                {
                    childParseNode = parseNode.Children[i];
                    children.Add(FromParseNode(childParseNode));
                }
                return new ExpressionNode(parseNode.Children[parseNode.Children.Count - 1].Token.Symbol, children);
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errMsgWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }
    }
}
