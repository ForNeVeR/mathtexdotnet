using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class TexExpressionTreeBuilderException : Exception
    {
        private const string errorMessageInvalidSymbol =
            "Expected a parse node with one of the following symbol kinds: {0}.";

        public TexExpressionTreeBuilderException(ParseNode node, ICollection<TexSymbolKind> expectedSymbolKinds)
            : this(node, string.Format(errorMessageInvalidSymbol,
            string.Join(", ", expectedSymbolKinds.Select(tokenKind => tokenKind.ToString()).ToArray())))
        {
        }

        public TexExpressionTreeBuilderException(ParseNode node, string message)
            : base(message)
        {
            this.Node = node;
        }

        public ParseNode Node
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
                    string.Format("Parse node: {0}", this.Node);
	        }
        }
    }
}
