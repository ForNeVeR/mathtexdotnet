﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ExpressionNode
    {
        private const string errorMesssageWrongNumberOfChildren =
            "An parse node of kind {0} cannot have {1} children.";

        public static ExpressionNode FromParseNode(ParseNode parseNode)
        {
            switch (parseNode.Kind)
            {
                case ParseNodeKind.Expression:
                    return FromExpressionParseNode(parseNode);
                case ParseNodeKind.Term:
                    return FromTermParseNode(parseNode);
                case ParseNodeKind.IndexedValue:
                    return FromIndexedValueParseNode(parseNode);
                case ParseNodeKind.Modifier:
                    return FromModifierParseNode(parseNode);
                case ParseNodeKind.Token:
                    return new ExpressionNode(parseNode.Token.Symbol, parseNode.Token.Value);
                default:
                    throw new InvalidOperationException(string.Format(
                        "Invalid parse node kind: {0}.", parseNode.Kind));
            }
        }

        public static ExpressionNode FromExpressionParseNode(ParseNode parseNode)
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
                    errorMesssageWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public static ExpressionNode FromTermParseNode(ParseNode parseNode)
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
                    errorMesssageWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public static ExpressionNode FromIndexedValueParseNode(ParseNode parseNode)
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
                    errorMesssageWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public static ExpressionNode FromModifierParseNode(ParseNode parseNode)
        {
            if (parseNode.Children.Count >= 2)
            {
                var children = new ExpressionNode[parseNode.Children.Count - 1];
                for (int i = 0; i < children.Length; i++)
                    children[i] = new ExpressionNode(parseNode.Children[i + 1].Token.Symbol,
                        parseNode.Children[i + 1].Token.Value);
                return new ExpressionNode(parseNode.Children[0].Token.Symbol, children);
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    errorMesssageWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        public ExpressionNode(SymbolKind symbol, object value)
        {
            this.Symbol = symbol;
            this.Value = value;
            this.Children = null;
        }

        public ExpressionNode(SymbolKind symbol, IList<ExpressionNode> children)
        {
            this.Symbol = symbol;
            this.Value = null;
            this.Children = new ExpressionNodeCollection(children);
        }

        public ExpressionNode(SymbolKind symbol)
        {
            this.Symbol = symbol;
            this.Value = null;
            this.Children = new ExpressionNodeCollection();
        }

        public SymbolKind Symbol
        {
            get;
            set;
        }

        public object Value
        {
            get;
            set;
        }

        public ExpressionNodeCollection Children
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return this.Symbol + (this.Value == null ? string.Empty :
                "(" + this.Value.ToString() + ")");
        }
    }
}