using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TexDotNet.Tests
{
    public static class CollectionsExtensions
    {
        public static T[] ToArray<T>(this IEnumerator<T> source)
        {
            var list = new List<T>();
            while (source.MoveNext())
                list.Add(source.Current);
            return list.ToArray();
        }
    }
}
