using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet.Tests
{
    using TokenStream = IEnumerable<TexToken>;

    public class TestExamplesReader : IDisposable
    {
        private StreamReader streamReader;

        private bool disposed = false;

        public TestExamplesReader(string path)
            : this(new FileStream(path, FileMode.Open, FileAccess.Read))
        {
        }

        public TestExamplesReader(Stream stream)
        {
            this.BaseStream = stream;
            this.streamReader = new StreamReader(this.BaseStream);
        }

        ~TestExamplesReader()
        {
            GC.SuppressFinalize(true);
            Dispose(false);
        }

        public Stream BaseStream
        {
            get;
            private set;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.streamReader.Dispose();
                    this.BaseStream.Dispose();
                }
            }

            disposed = true;
        }

        public IEnumerable<TestExample> ReadAllExamples()
        {
            TestExample curItem;
            while ((curItem = ReadExample()) != null)
                yield return curItem;
        }

        public TestExample ReadExample()
        {
            var text = streamReader.ReadLine();
            if (text == null)
                return null;

            var expectedTokenString = streamReader.ReadLine();
            var expectedTokens = expectedTokenString.Split(' ').Select(tokenString =>
                {
                    return TexToken.FromKind(TexSymbolKind.Unknown, -1, null);
                }).ToArray();

            streamReader.ReadLine();

            return new TestExample(text, expectedTokens);
        }
    }
}
