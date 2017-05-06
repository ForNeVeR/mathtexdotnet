using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Syracuse.UnitTesting
{
    public class TestCaseGroup<TCase> : Collection<TCase>
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
