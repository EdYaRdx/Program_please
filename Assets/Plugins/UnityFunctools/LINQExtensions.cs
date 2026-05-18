using System;
using System.Collections.Generic;

namespace UnityFunctools
{
    public static class LINQExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T> callback)
        {
            foreach (T value in values)
                callback.Invoke(value);

            return values;
        }
    }
}