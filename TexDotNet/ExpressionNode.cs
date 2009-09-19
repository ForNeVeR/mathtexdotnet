using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ExpressionNode
    {
        public ExpressionNode(ExpressionNode parent, SymbolKind symbol, object value)
            : this(parent)
        {
            this.Symbol = symbol;
            this.Value = value;
            this.Children = null;
            this.Arguments = null;
        }

        public ExpressionNode(ExpressionNode parent, SymbolKind symbol, IList<ExpressionNode> children,
            IList<ExpressionNode> arguments)
            : this(parent)
        {
            this.Symbol = symbol;
            this.Value = null;
            this.Children = new ExpressionNodeCollection(children);
            this.Arguments = new ExpressionNodeCollection(arguments);
        }

        public ExpressionNode(ExpressionNode parent, SymbolKind symbol, IList<ExpressionNode> children)
            : this(parent)
        {
            this.Symbol = symbol;
            this.Value = null;
            this.Children = new ExpressionNodeCollection(children);
            this.Arguments = new ExpressionNodeCollection();
        }

        public ExpressionNode(ExpressionNode parent, SymbolKind symbol)
            : this(parent)
        {
            this.Symbol = symbol;
            this.Value = null;
            this.Children = new ExpressionNodeCollection();
            this.Arguments = new ExpressionNodeCollection();
        }

        public ExpressionNode(ExpressionNode parent)
        {
            this.Parent = parent;
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

        public ExpressionNode Parent
        {
            get;
            internal set;
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
