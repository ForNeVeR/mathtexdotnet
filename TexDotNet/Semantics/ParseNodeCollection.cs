using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ParseNodeCollection : Collection<ParseNode>
    {
        public ParseNodeCollection(IList<ParseNode> list)
            : base(list)
        {
        }

        public ParseNodeCollection()
            : base()
        {
        }
    }
}
