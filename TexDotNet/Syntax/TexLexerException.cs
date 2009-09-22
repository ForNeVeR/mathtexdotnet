using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class LexerException : TexErrorSourceInfoException
    {
        public LexerException(int sourcePosition, string sourceText, string message)
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
