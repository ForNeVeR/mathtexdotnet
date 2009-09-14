using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet.CommandLine
{
    public class TreeTextRenderer
    {
        public TreeTextRenderer(TextWriter writer)
        {
            this.Writer = writer;
        }

        public TextWriter Writer
        {
            get;
            private set;
        }

        public void Render(ParseTree tree)
        {
            Render(tree.RootNode, 0);
        }

        public void Render(ParseNode node, int level)
        {
            var indentation = new string(' ', level * 2);
            this.Writer.WriteLine(indentation + node.ToString());
            if (node.Children != null)
            {
                foreach (var childNode in node.Children)
                    Render(childNode, level + 1);
            }
        }
    }
}
