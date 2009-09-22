using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public interface ITexErrorSourceInfo
    {
        int SourcePosition { get; }
        string SourceText { get; }
    }
}
