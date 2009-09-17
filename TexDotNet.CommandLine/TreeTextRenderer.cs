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
            Render(tree.RootNode, false, node => node.Arguments, node => node.Children, 0);
        }

        public void Render(ParseTree tree)
        {
            Render(tree.RootNode, false, null, node => node.Children, 0);
        }

        private void Render<TNode>(TNode node, bool isAttribute, Func<TNode, IEnumerable<TNode>> getAttributes,
            Func<TNode, IEnumerable<TNode>> getChildren, int level)
        {
            // Prefix attribute nodes with '@'.
            var indentation = new string(' ', level * 2);
            this.Writer.WriteLine(indentation + (isAttribute ? "@" : string.Empty) + node.ToString());
            var attributes = getAttributes == null ? null : getAttributes(node);
            var children = getChildren(node);
            if (children != null)
            {
                if (attributes != null)
                {
                    foreach (var childNode in attributes)
                        Render(childNode, true, getAttributes, getChildren, level + 1);
                }
                foreach (var childNode in children)
                    Render(childNode, false, getAttributes, getChildren, level + 1);
            }
        }
    }
}
