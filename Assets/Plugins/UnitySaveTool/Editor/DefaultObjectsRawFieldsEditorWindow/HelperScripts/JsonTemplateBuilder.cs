using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnitySaveTool.EditorTools
{
    public class JsonTemplateBuilder
    {
        public static Dictionary<string, object> BuildTemplateDom(Type type)
        {
            Dictionary<string, object> root = new Dictionary<string, object>();

            if (type == null)
                return root;

            List<FieldInfo> fields = UnitySerializedFieldScanner.GetUnitySerializedFields(type);

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo f = fields[i];
                root[f.Name] = DefaultValueFactory.CreateDefaultDomValue(f.FieldType);
            }

            return root;
        }
    }
}
