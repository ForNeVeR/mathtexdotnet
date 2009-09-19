using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class LexerException : Exception
    {
        public LexerException(int position, string message)
            : base(message)
        {
            this.Position = position;
        }

        public int Position
        {
            get;
            private set;
        }

        public override string Message
        {
            get
            {
                return base.Message + Environment.NewLine +
                    string.Format("Charcter position: {0}", this.Position);
            }
        }
    }
}
