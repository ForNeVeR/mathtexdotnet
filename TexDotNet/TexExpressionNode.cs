using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TexDotNet
{
    [DebuggerDisplay("{ToString() + string.Format(\" [\\{0\\} children]\", this.Children.Count),nq}")]
    public class TexExpressionNode
    {
        public TexExpressionNode(TexSymbolKind symbol, IEnumerable<TexExpressionNode> children)
            : this(symbol)
        {
            foreach (var childNode in children)
                this.Children.Add(childNode);
        }

        public TexExpressionNode(TexSymbolKind symbol, object value)
            : this()
        {
            this.Symbol = symbol;
            this.Value = value;
            this.Children = new TexExpressionNodeCollection(this);
            this.Arguments = new TexExpressionNodeCollection(this);
        }

        public TexExpressionNode(TexSymbolKind symbol)
            : this()
        {
            this.Symbol = symbol;
            this.Value = null;
        }

        public TexExpressionNode()
        {
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
            return this.Symbol + (this.Value == null ? string.Empty : "(" + this.Value.ToString() + ")");
        }

        public override bool Equals(object obj)
        {
            return Equals((TexExpressionNode)obj);
        }

        public bool Equals(TexExpressionNode tree)
        {
            if (!this.Symbol.Equals(tree.Symbol))
                return false;
            if (this.Value != null && !this.Value.Equals(tree.Value))
                return false;
            if (!this.Children.Count.Equals(tree.Children.Count))
                return false;
            for (int i = 0; i < tree.Children.Count; i++)
            {
                if (!this.Children[i].Equals(tree.Children[i]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            if (this.Children.Count == 0)
                return unchecked((int)this.Symbol * 17 + this.Value.GetHashCode());
            int hashCode = 0;
            for (int i = 0; i < this.Children.Count; i++)
                hashCode = unchecked(hashCode * 31 + this.Children[i].GetHashCode());
            return hashCode;
        }
    }
}
