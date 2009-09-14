using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ParserException : Exception
    {
        public ParserException(Token tokenRead, ICollection<TokenKind> expectedTokenKinds)
            : this(tokenRead, string.Format("Invalid token; expected one of the following token kinds: {0}.",
            string.Join(", ", expectedTokenKinds.Select(tokenKind => tokenKind.ToString()).ToArray())))
        {
        }

        public ParserException(Token tokenRead, string message)
            : base(
                message + Environment.NewLine +
                string.Format("Token read: {0}", tokenRead))
        {
            this.TokenRead = tokenRead;
        }

        public Token TokenRead
        {
            get;
            private set;
        }
    }
}
