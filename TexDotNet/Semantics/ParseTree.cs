using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ParseTree
    {
        public ParseTree(ParseNode rootNode)
        {
            this.RootNode = rootNode;
        }

        public ParseNode RootNode
        {
            get;
            private set;
        }
    }
}
