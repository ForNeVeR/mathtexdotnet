using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ParseNode
    {
        public ParseNode(Token value, IList<ParseNode> children)
        {
            this.Value = value;
            this.Children = new ParseNodeCollection(children);
        }

        public ParseNode(Token value)
        {
            this.Value = value;
            this.Children = null;
        }

        public Token Value
        {
            get;
            set;
        }

        public ParseNodeCollection Children
        {
            get;
            private set;
        }
    }
}
