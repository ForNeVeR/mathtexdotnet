using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ParseNodeCollection : Collection<ParseNode>
    {
        public ParseNodeCollection(ParseNode parentNode)
            : this()
        {
            this.ParentNode = parentNode;
        }

        public ParseNodeCollection()
            : base()
        {
        }

        public ParseNode ParentNode
        {
            get;
            private set;
        }

        protected override void ClearItems()
        {
            foreach (var item in this.Items)
                item.Parent = null;
            base.ClearItems();
        }

        protected override void InsertItem(int index, ParseNode item)
        {
            item.Parent = this.ParentNode;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this.Items[index].Parent = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, ParseNode item)
        {
            item.Parent = this.ParentNode;
            base.SetItem(index, item);
        }
    }
}
