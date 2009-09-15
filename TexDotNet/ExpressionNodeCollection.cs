using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ExpressionNodeCollection : Collection<ExpressionNode>
    {
        public ExpressionNodeCollection(IList<ExpressionNode> list)
            : base(list)
        {
        }

        public ExpressionNodeCollection()
            : base()
        {
        }
    }
}
