using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Syracuse.Common;

namespace Syracuse.UnitTesting
{
    public class TestCaseSet<TCase> : KeyedCollection<string, TestCaseGroup<TCase>>
        where TCase : new()
    {
        public static TestCaseSet<TCase> FromFile(string path)
        {
            using (var reader = new TestCaseReader<TCase>(path))
            {
                var set = new TestCaseSet<TCase>();
                set.AddRange(reader.ReadAllTestCaseGroups());
                return set;
            }
        }

        public static TestCaseSet<TCase> FromStream(Stream stream)
        {
            using (var reader = new TestCaseReader<TCase>(stream))
            {
                var set = new TestCaseSet<TCase>();
                set.AddRange(reader.ReadAllTestCaseGroups());
                return set;
            }
        }

        public TestCaseSet()
            : base()
        {
        }

        protected override string GetKeyForItem(TestCaseGroup<TCase> item)
        {
            return item.Name;
        }
    }
}
