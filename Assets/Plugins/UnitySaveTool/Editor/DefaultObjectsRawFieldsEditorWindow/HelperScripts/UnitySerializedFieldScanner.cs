using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnitySaveTool.EditorTools
{
    public class UnitySerializedFieldScanner
    {
        public static List<FieldInfo> GetUnitySerializedFields(Type type)
        {
            List<FieldInfo> result = new List<FieldInfo>();

            Type t = type;
            while (t != null && t != typeof(object))
            {
                FieldInfo[] fields = t.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];

                    if (f.IsStatic)
                        continue;

                    if (f.IsNotSerialized)
                        continue;

                    bool hasSerializeField = f.GetCustomAttributes(typeof(SerializeField), true).Length > 0;
                    bool hasSerializeReference = f.GetCustomAttributes(typeof(SerializeReference), true).Length > 0;

                    bool unityWouldSerialize = f.IsPublic || hasSerializeField || hasSerializeReference;
                    if (unityWouldSerialize == false)
                        continue;

                    result.Add(f);
                }

                t = t.BaseType;
            }

            result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return result;
        }
    }
}