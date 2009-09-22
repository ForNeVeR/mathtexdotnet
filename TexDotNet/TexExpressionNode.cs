using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class TexExpressionNode
    {
        public TexExpressionNode(TexSymbolKind symbol, IEnumerable<TexExpressionNode> children)
            : this(symbol)
        {
            foreach (var childNode in children)
                this.Children.Add(childNode);
        }

        public TexExpressionNode(TexSymbolKind symbol, object value)
        {
            this.Symbol = symbol;
            this.Value = value;
            this.Children = new TexExpressionNodeCollection(this);
            this.Arguments = new TexExpressionNodeCollection(this);
        }

        public TexExpressionNode(TexSymbolKind symbol)
        {
            this.Symbol = symbol;
            this.Value = null;
            this.Children = new TexExpressionNodeCollection(this);
            this.Arguments = new TexExpressionNodeCollection(this);
        }

        public TexSymbolKind Symbol
        {
            get;
            set;
        }

        public object Value
        {
            get;
            set;
        }

        public TexExpressionNodeCollection Children
        {
            get;
            private set;
        }

        public TexExpressionNodeCollection Arguments
        {
            get;
            private set;
        }

        public TexExpressionNode Parent
        {
            get;
            internal set;
        }

        public override string ToString()
        {
            return this.Symbol + (this.Value == null ? string.Empty :
                "(" + this.Value.ToString() + ")");
        }
    }
}
