using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<Token>;

    public class TexWriter
    {
        public TexWriter(TextWriter writer)
        {
            this.TextWriter = writer;
        }

        public TextWriter TextWriter
        {
            get;
            private set;
        }

        public void Write(ExpressionTree tree)
        {
            Write(tree.RootNode);
        }

        public void Write(ExpressionNode node)
        {
            // TODO: Write text for given node, and repeat for children recursively.
            throw new NotImplementedException();
        }
    }
}
