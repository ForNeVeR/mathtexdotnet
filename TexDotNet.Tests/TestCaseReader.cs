using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet.Tests
{
    using TokenStream = IEnumerable<TexToken>;

    public class TestCaseReader : IDisposable
    {
        private Stream stream;
        private StreamReader streamReader;

        private bool disposed = false;

        public TestCaseReader(string path)
            : this(new FileStream(path, FileMode.Open, FileAccess.Read))
        {
        }

        public TestCaseReader(Stream stream)
        {
            this.stream = stream;
            this.streamReader = new StreamReader(this.BaseStream);
        }

        ~TestCaseReader()
        {
            GC.SuppressFinalize(true);
            Dispose(false);
        }

        public Stream BaseStream
        {
            get { return this.stream; }
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

        public IEnumerable<TestCaseGroup> ReadAllTestCaseGroups()
        {
            TestCaseGroup group;
            while ((group = ReadTestCaseGroup()) != null)
                yield return group;
        }

        public TestCaseGroup ReadTestCaseGroup()
        {
            string line;

            string groupName = null;
            while (groupName == null)
            {
                line = this.streamReader.ReadLine();
                if (line == null)
                    return null;
                if (line.Length > 0)
                    groupName = line.Substring(1).TrimStart();
            }

            var group = new TestCaseGroup(groupName);
            TestCase testCase;
            while (true)
            {
                testCase = ReadTestCase();
                if (testCase == null)
                    break;
                group.Add(testCase);
            }
            return group;
        }

        public TestCase ReadTestCase()
        {
            var line = streamReader.ReadLine();
            if (string.IsNullOrEmpty(line))
                return null;
            return new TestCase(line);
        }
    }
}
