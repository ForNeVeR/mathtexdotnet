using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class WriterException : Exception
    {
        public WriterException(TexToken token)
            : base(null)
        {
            this.Token = token;
        }

        public WriterException(TexToken token, string message)
            : base(message)
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
                return base.Message + Environment.NewLine +
                    string.Format("Unknown symbol kind: {0}", this.Token.Symbol);
            }
        }
    }
}
