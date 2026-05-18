using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnitySaveTool.EditorTools
{
    public class SerializableReferenceTypeCatalog
    {
        private static List<Type> _cached;

        public static List<Type> GetOrBuild()
        {
            if (_cached != null)
                return _cached;

            _cached = Build();
            return _cached;
        }

        private static List<Type> Build()
        {
            List<Type> result = new List<Type>(2048);

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly asm = assemblies[i];
                if (asm == null || asm.IsDynamic)
                    continue;

                string asmName = asm.GetName().Name;

                if (asmName.StartsWith("UnityEditor", StringComparison.Ordinal) ||
                    asmName.StartsWith("Unity.", StringComparison.Ordinal) ||
                    asmName.StartsWith("UnityEngine", StringComparison.Ordinal) ||
                    asmName.StartsWith("System", StringComparison.Ordinal) ||
                    asmName.StartsWith("mscorlib", StringComparison.Ordinal))
                {
                    continue;
                }

                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    types = rtle.Types;
                }

                if (types == null)
                    continue;

                for (int t = 0; t < types.Length; t++)
                {
                    Type type = types[t];
                    if (IsCandidate(type))
                        result.Add(type);
                }
            }

            result.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
            return result;
        }

        private static bool IsCandidate(Type type)
        {
            if (type == null)
                return false;

            if (type.IsClass == false)
                return false;

            if (type.IsAbstract || type.IsInterface)
                return false;

            if (type.ContainsGenericParameters || type.IsGenericTypeDefinition)
                return false;

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return false;

            if (type.IsSerializable == false)
                return false;

            if (type.FullName == null)
                return false;

            return true;
        }
    }
}
