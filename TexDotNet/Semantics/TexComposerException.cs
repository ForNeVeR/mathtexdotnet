using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ComposerException : Exception
    {
        private const string errorMessageInvalidSymbol =
            "Expected a node with one of the following symbol kinds: {0}.";

        public ComposerException(TexExpressionNode node, ICollection<TexSymbolKind> expectedSymbolKinds)
            : this(node, string.Format(errorMessageInvalidSymbol,
            string.Join(", ", expectedSymbolKinds.Select(tokenKind => tokenKind.ToString()).ToArray())))
        {
        }

        public ComposerException(TexExpressionNode node, string message)
            : base(message)
        {
            this.Node = node;
        }

        public TexExpressionNode Node
        {
            get;
            private set;
        }

        public override string Message
        {
            get 
	        { 
		        return
                    base.Message +  Environment.NewLine +
                    string.Format("Expression node: {0}", this.Node);
	        }
        }
    }
}
