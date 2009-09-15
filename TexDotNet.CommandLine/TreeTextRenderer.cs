using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet.CommandLine
{
    public sealed class TreeTextRenderer
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

        public void Render(ExpressionTree tree)
        {
            Render(tree.RootNode, node => node.Children, 0);
        }

        public void Render(ParseTree tree)
        {
            Render(tree.RootNode, node => node.Children, 0);
        }

        private void Render<TNode>(TNode node, Func<TNode, IEnumerable<TNode>> getChildren, int level)
        {
            var indentation = new string(' ', level * 2);
            this.Writer.WriteLine(indentation + node.ToString());
            var children = getChildren(node);
            if (children != null)
            {
                foreach (var childNode in children)
                    Render(childNode, getChildren, level + 1);
            }
        }
    }
}
