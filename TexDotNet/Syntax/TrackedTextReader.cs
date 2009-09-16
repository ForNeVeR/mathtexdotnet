using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class TrackedTextReader : TextReader
    {
        public TrackedTextReader(TextReader baseTextReader)
            : base()
        {
            this.BaseTextReader = baseTextReader;
        }

        public int Position
        {
            get;
            private set;
        }

        public TextReader BaseTextReader
        {
            get;
            private set;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this.BaseTextReader.Close();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override object InitializeLifetimeService()
        {
            return this.BaseTextReader.InitializeLifetimeService();
        }

        public override System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType)
        {
            return this.BaseTextReader.CreateObjRef(requestedType);
        }

        public override int Peek()
        {
            return this.BaseTextReader.Peek();
        }

        public override int Read()
        {
            this.Position++;
            return this.BaseTextReader.Read();
        }

        public override bool Equals(object obj)
        {
            return this.BaseTextReader.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.BaseTextReader.GetHashCode();
        }

    }
}
