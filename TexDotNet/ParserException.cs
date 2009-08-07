using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ParserException : Exception
    {
        public ParserException(string message)
            : base(message)
        {
        }

        public ParserException()
            : base()
        {
        }
    }
}
