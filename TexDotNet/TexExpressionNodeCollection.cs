using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class TexExpressionNodeCollection : Collection<TexExpressionNode>
    {
        public TexExpressionNodeCollection(TexExpressionNode parentNode)
            : this()
        {
            this.ParentNode = parentNode;
        }

        public TexExpressionNodeCollection()
            : base()
        {
        }

        public TexExpressionNode ParentNode
        {
            get;
            private set;
        }

        protected override void InsertItem(int index, TexExpressionNode item)
        {
            item.Parent = this.ParentNode;
            base.InsertItem(index, item);
        }
    }
}
