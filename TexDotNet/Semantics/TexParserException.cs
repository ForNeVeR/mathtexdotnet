using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ParserException : TexErrorSourceInfoException
    {
        private const string errorMessageInvalidSymbol =
            "Expected a token with one of the following symbol kinds: {0}.";

        public ParserException(TexToken token, ICollection<TexSymbolKind> expectedSymbolKinds)
            : this(token, string.Format(errorMessageInvalidSymbol,
            string.Join(", ", expectedSymbolKinds.Select(tokenKind => tokenKind.ToString()).ToArray())))
        {
        }

        public ParserException(TexToken token, string message)
            : base(token, message)
        {
            this.Token = token;
        }

        public TexToken Token
        {
            get;
            private set;
        }

        public override string Message
        {
            get
            {
                return
                    base.Message + Environment.NewLine +
                    string.Format("Token kind: {0}", this.Token.Symbol);
            }
        }
    }
}
