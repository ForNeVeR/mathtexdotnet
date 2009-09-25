using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet.Tests
{
    public class TestCaseSet : KeyedCollection<string, TestCaseGroup>
    {
        public static TestCaseSet FromFile(string path)
        {
            using (var reader = new TestCaseReader(path))
            {
                var set = new TestCaseSet();
                set.AddRange(reader.ReadAllTestCaseGroups());
                return set;
            }
        }

        public static TestCaseSet FromStream(Stream stream)
        {
            using (var reader = new TestCaseReader(stream))
            {
                var set = new TestCaseSet();
                set.AddRange(reader.ReadAllTestCaseGroups());
                return set;
            }
        }

        public TestCaseSet()
            : base()
        {
        }

        protected override string GetKeyForItem(TestCaseGroup item)
        {
            return item.Name;
        }
    }
}
