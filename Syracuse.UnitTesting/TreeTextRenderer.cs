using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TexDotNet;

namespace Syracuse.UnitTesting
{
    public sealed class TreeTextRenderer
    {
        public static string GetText(TexExpressionNode tree)
        {
            using (var stringWriter = new StringWriter())
            {
                var treeTextRenderer = new TreeTextRenderer(stringWriter);
                treeTextRenderer.Render(tree);
                return stringWriter.ToString();
            }
        }

        public TreeTextRenderer(TextWriter writer)
        {
            this.Writer = writer;
        }

        public TextWriter Writer
        {
            get;
            private set;
        }

        public void Render(TexExpressionNode tree)
        {
            Render(tree, false, 0);
        }

        private void Render(TexExpressionNode node, bool isArgument, int level)
        {
            // Prefix argument nodes with '@'.
            var indentation = new string(' ', level * 2);
            this.Writer.WriteLine(indentation + (isArgument ? "@" : string.Empty) + node.ToString());
            if (node.Children != null)
            {
                if (node.Arguments != null)
                {
                    foreach (var childNode in node.Arguments)
                        Render(childNode, true, level + 1);
                }
                foreach (var childNode in node.Children)
                    Render(childNode, false, level + 1);
            }
        }
    }
}
