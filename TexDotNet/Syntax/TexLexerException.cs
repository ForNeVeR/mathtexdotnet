using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class TexLexerException : TexErrorSourceInfoException
    {
        public TexLexerException(int sourcePosition, string sourceText, string message)
            : base(sourcePosition, sourceText, message)
        {
        }

        public override string Message
        {
            get
            {
                return base.Message;
            }
        }
    }
}
