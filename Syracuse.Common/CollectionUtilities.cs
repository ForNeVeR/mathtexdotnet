using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Syracuse.Common
{
    public static class CollectionsExtensions
    {
        public static void AddRange<T>(this IList<T> source, IEnumerable<T> value)
        {
            foreach (var item in value)
                source.Add(item);
        }

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> source)
        {
            while (source.MoveNext())
                yield return source.Current;
        }
    }
}
