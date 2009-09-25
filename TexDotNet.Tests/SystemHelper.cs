using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TexDotNet.Tests
{
    public static class SystemHelper
    {
        public static Stream GetResourceStream(string name)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(SystemHelper), name);
            if (stream == null)
                throw new FileNotFoundException("Cannot find resource stream.", name);
            return stream;
        }
    }
}
