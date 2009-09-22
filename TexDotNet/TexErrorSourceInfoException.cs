using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class TexErrorSourceInfoException : Exception
    {
        public TexErrorSourceInfoException(ITexErrorSourceInfo sourceInfo, string message)
            : this(sourceInfo.SourcePosition, sourceInfo.SourceText, message)
        {
        }

        public TexErrorSourceInfoException(int sourcePosition, string sourceText, string message)
            : base(message)
        {
            this.SourcePosition = sourcePosition;
            this.SourceText = sourceText;
        }

        public int SourcePosition
        {
            get;
            set;
        }

        public string SourceText
        {
            get;
            set;
        }

        public override string Message
        {
            get
            {
                return base.Message + Environment.NewLine +
                    string.Format("Charcter position: {0}", this.SourcePosition) + Environment.NewLine +
                    string.Format("Source text: {0}", this.SourceText);
            }
        }
    }
}
