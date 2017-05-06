using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Syracuse.UnitTesting
{
    public class TestCaseReader<TCase> : IDisposable
        where TCase : new()
    {
        private FieldTypeInfo[] caseFieldTypeInfos;

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

            this.caseFieldTypeInfos = typeof(TCase).GetFieldTypes();
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
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.stream.Dispose();
                    this.streamReader.Dispose();
                }
            }

            this.disposed = true;
        }

        public IEnumerable<TestCaseGroup<TCase>> ReadAllTestCaseGroups()
        {
            TestCaseGroup<TCase> group;
            while ((group = ReadTestCaseGroup()) != null)
                yield return group;
        }

        public TestCaseGroup<TCase> ReadTestCaseGroup()
        {
            string line;

            // Reade name of group.
            string groupName = null;
            while (groupName == null)
            {
                line = this.streamReader.ReadLine();
                if (line == null)
                    return null;
                if (line.Length > 0)
                    groupName = line.Substring(1).TrimStart();
            }

            // Read sequence of test cases within group.
            var group = new TestCaseGroup<TCase>(groupName);
            TCase testCase;
            bool endOfStream = false;
            bool ignoreTestCase;
            while (!endOfStream)
            {
                testCase = ReadTestCase(out endOfStream, out ignoreTestCase);
                if (!ignoreTestCase)
                    group.Add(testCase);
            }
            return group;
        }

        public TCase ReadTestCase(out bool endOfCase, out bool ignore)
        {
            endOfCase = false;
            ignore = true;

            // Read values of current test case, using one line per field.
            var testCase = new TCase();
            int fieldIndex = 0;
            while(true)
            {
                var line = this.streamReader.ReadLine();
                if (line == null)
                {
                    endOfCase = true;
                    break;
                }
                line = line.Trim();

                // Check if line represents end-of-group.
                if (line == "---")
                {
                    endOfCase = true;
                    break;
                }
                // Ignore blank line.
                if (line.Length == 0)
                    continue;
                // Ignore comment.
                if (line.StartsWith("//"))
                    continue;

                // Set value of current field of test case to converted line text.
                var fieldTypeInfo = this.caseFieldTypeInfos[fieldIndex++];
                var fieldValue = fieldTypeInfo.TypeConverter.ConvertFromString(line);
                fieldTypeInfo.FieldInfo.SetValueDirect(__makeref(testCase), fieldValue);
                ignore = false;
                if (fieldIndex == this.caseFieldTypeInfos.Length)
                    break;
            }
            return testCase;
        }
    }
}
