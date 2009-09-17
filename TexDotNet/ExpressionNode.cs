using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ExpressionNode
    {
        public ExpressionNode(SymbolKind symbol, object value)
            : this()
        {
            this.Symbol = symbol;
            this.Value = value;
            this.Children = null;
            this.Arguments = null;
        }

        public ExpressionNode(SymbolKind symbol, IList<ExpressionNode> children, IList<ExpressionNode> arguments)
            : this()
        {
            this.Symbol = symbol;
            this.Value = null;
            this.Children = new ExpressionNodeCollection(children);
            this.Arguments = new ExpressionNodeCollection(arguments);
        }

        public ExpressionNode(SymbolKind symbol, IList<ExpressionNode> children)
            : this()
        {
            this.Symbol = symbol;
            this.Value = null;
            this.Children = new ExpressionNodeCollection(children);
            this.Arguments = new ExpressionNodeCollection();
        }

        public ExpressionNode(SymbolKind symbol)
            : this()
        {
            this.Symbol = symbol;
            this.Value = null;
            this.Children = new ExpressionNodeCollection();
            this.Arguments = new ExpressionNodeCollection();
        }

        public ExpressionNode()
        {
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

        public ExpressionNodeCollection Arguments
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
