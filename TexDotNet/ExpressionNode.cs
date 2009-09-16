using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ExpressionNode
    {
        private const string errMsgWrongNumberOfChildren =
            "An parse node of kind {0} cannot have {1} children.";

        #region Conversion from Parse Tree

        public static ExpressionNode FromParseNode(ParseNode parseNode)
        {
            switch (parseNode.Kind)
            {
                case ParseNodeKind.Operation:
                    return FromOperationParseNode(parseNode);
                case ParseNodeKind.FunctionCall:
                    return FromFunctionCallParseNode(parseNode);
                case ParseNodeKind.Token:
                    return new ExpressionNode(parseNode.Token.Symbol, parseNode.Token.Value);
                default:
                    throw new InvalidOperationException(string.Format(
                        "Invalid parse node kind: {0}.", parseNode.Kind));
            }
        }

        public static ExpressionNode FromOperationParseNode(ParseNode parseNode)
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

        public static ExpressionNode FromFunctionCallParseNode(ParseNode parseNode)
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
                    errMsgWrongNumberOfChildren, parseNode.Kind, parseNode.Children.Count));
            }
        }

        #endregion

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

        #region Conversion to Parse Tree

        public ParseNode ToParseNode()
        {
            // TODO
            throw new NotImplementedException();
        }

        #endregion

        public override string ToString()
        {
            return this.Symbol + (this.Value == null ? string.Empty :
                "(" + this.Value.ToString() + ")");
        }
    }
}
