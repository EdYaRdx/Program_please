using System;
using System.Collections.Generic;

namespace UnitySaveTool.EditorTools
{
    public class DefaultValueFactory
    {
        public static object CreateDefaultDomValue(Type t)
        {
            if (t == typeof(string))
                return string.Empty;

            if (t == typeof(bool))
                return false;

            if (t.IsEnum)
                return 0L;

            if (IsInteger(t))
                return 0L;

            if (IsFloat(t))
                return 0.0;

            if (t.IsArray)
                return new List<object>();

            if (IsList(t))
                return new List<object>();

            if (t.IsValueType)
                return new Dictionary<string, object>();

            return null;
        }

        private static bool IsList(Type t)
        {
            if (t == null || t.IsGenericType == false)
                return false;

            return t.GetGenericTypeDefinition() == typeof(List<>);
        }

        private static bool IsInteger(Type t)
        {
            return t == typeof(int) || t == typeof(long) || t == typeof(short) ||
                   t == typeof(uint) || t == typeof(ulong) || t == typeof(ushort) ||
                   t == typeof(byte) || t == typeof(sbyte);
        }

        private static bool IsFloat(Type t)
        {
            return t == typeof(float) || t == typeof(double) || t == typeof(decimal);
        }
    }
}
