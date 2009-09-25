using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet.Tests
{
    using TokenStream = IEnumerable<TexToken>;

    public class TestCaseGroup : Collection<TestCase>
    {
        public TestCaseGroup(string name)
            : base()
        {
            this.Name = name;
        }

        public string Name
        {
            get;
            private set;
        }
    }
}
