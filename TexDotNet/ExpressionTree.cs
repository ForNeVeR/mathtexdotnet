using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ExpressionTree
    {

        public ExpressionTree(ExpressionNode rootNode)
        {
            this.RootNode = rootNode;
        }

        public ExpressionNode RootNode
        {
            get;
            private set;
        }
    }
}
