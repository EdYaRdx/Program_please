using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine.UIElements;

namespace UnitySaveTool.EditorTools
{
    public class JsonFieldEditorFactory
    {
        public static VisualElement CreateEditorForField(
                FieldInfo field,
                Dictionary<string, object> rootObj,
                Action onChanged)
        {
            Type fieldType = field.FieldType;

            object rawValue;
            rootObj.TryGetValue(field.Name, out rawValue);

            return CreateEditor(field.Name, fieldType, rawValue, rootObj, onChanged, 0);
        }

        private static VisualElement CreateEditor(
            string key,
            Type type,
            object rawValue,
            Dictionary<string, object> parentObj,
            Action onChanged,
            int depth)
        {
            if (type == typeof(string))
                return CreateStringField(key, rawValue, parentObj, onChanged);

            if (type == typeof(bool))
                return CreateBoolField(key, rawValue, parentObj, onChanged);

            if (type.IsEnum)
                return CreateEnumField(key, type, rawValue, parentObj, onChanged);

            if (IsInteger(type))
                return CreateLongField(key, rawValue, parentObj, onChanged);

            if (IsFloat(type))
                return CreateDoubleField(key, rawValue, parentObj, onChanged);

            if (type.IsArray)
                return CreateListEditor(key, type.GetElementType(), rawValue, parentObj, onChanged);

            if (IsList(type))
                return CreateListEditor(key, type.GetGenericArguments()[0], rawValue, parentObj, onChanged);

            return CreateObjectEditor(key, type, rawValue, parentObj, onChanged, depth);
        }

        private static VisualElement CreateStringField(string key, object rawValue, Dictionary<string, object> parent, Action onChanged)
        {
            TextField f = new TextField(key);
            f.value = rawValue != null ? Convert.ToString(rawValue, CultureInfo.InvariantCulture) : string.Empty;

            f.RegisterValueChangedCallback(evt =>
            {
                parent[key] = evt.newValue;
                onChanged();
            });

            return f;
        }

        private static VisualElement CreateBoolField(string key, object rawValue, Dictionary<string, object> parent, Action onChanged)
        {
            Toggle t = new Toggle(key);
            t.value = ToBool(rawValue);

            t.RegisterValueChangedCallback(evt =>
            {
                parent[key] = evt.newValue;
                onChanged();
            });

            return t;
        }

        private static VisualElement CreateEnumField(string key, Type enumType, object rawValue, Dictionary<string, object> parent, Action onChanged)
        {
            long num = ToLong(rawValue);

            Array values = Enum.GetValues(enumType);
            object current = values.Length > 0 ? values.GetValue(0) : null;

            for (int i = 0; i < values.Length; i++)
            {
                object v = values.GetValue(i);
                long vv = Convert.ToInt64(v, CultureInfo.InvariantCulture);
                if (vv == num)
                {
                    current = v;
                    break;
                }
            }

            EnumField ef = new EnumField(key, current as Enum);

            ef.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == null)
                    parent[key] = 0L;
                else
                    parent[key] = Convert.ToInt64(evt.newValue, CultureInfo.InvariantCulture);

                onChanged();
            });

            return ef;
        }

        private static VisualElement CreateLongField(string key, object rawValue, Dictionary<string, object> parent, Action onChanged)
        {
            LongField f = new LongField(key);
            f.value = ToLong(rawValue);

            f.RegisterValueChangedCallback(evt =>
            {
                parent[key] = evt.newValue;
                onChanged();
            });

            return f;
        }

        private static VisualElement CreateDoubleField(string key, object rawValue, Dictionary<string, object> parent, Action onChanged)
        {
            DoubleField f = new DoubleField(key);
            f.value = ToDouble(rawValue);

            f.RegisterValueChangedCallback(evt =>
            {
                parent[key] = evt.newValue;
                onChanged();
            });

            return f;
        }

        private static VisualElement CreateObjectEditor(
                string key,
                Type objectType,
                object rawValue,
                Dictionary<string, object> parent,
                Action onChanged,
                int depth)
        {
            Dictionary<string, object> obj = rawValue as Dictionary<string, object>;
            if (obj == null)
            {
                obj = new Dictionary<string, object>();
                parent[key] = obj;
            }

            Foldout foldout = new Foldout();
            foldout.text = key + " (" + GetNiceTypeName(objectType) + ")";
            foldout.value = depth < 2;

            List<FieldInfo> fields = UnitySerializedFieldScanner.GetUnitySerializedFields(objectType);
            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo f = fields[i];

                object childValue;
                obj.TryGetValue(f.Name, out childValue);

                VisualElement childEditor = CreateEditor(f.Name, f.FieldType, childValue, obj, onChanged, depth + 1);
                foldout.Add(childEditor);
            }

            return foldout;
        }

        private static VisualElement CreateListEditor(
            string key,
            Type elementType,
            object rawValue,
            Dictionary<string, object> parent,
            Action onChanged)
        {
            List<object> list = rawValue as List<object>;
            if (list == null)
            {
                list = new List<object>();
                parent[key] = list;
            }

            Foldout foldout = new Foldout();
            foldout.text = key + " [" + GetNiceTypeName(elementType) + "] (" + list.Count + ")";
            foldout.value = false;

            VisualElement itemsRoot = new VisualElement();
            itemsRoot.style.flexDirection = FlexDirection.Column;

            Action rebuildItems = null;

            rebuildItems = () =>
            {
                itemsRoot.Clear();

                for (int i = 0; i < list.Count; i++)
                {
                    int index = i;

                    VisualElement row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.alignItems = Align.Center;

                    EnsureComplexElementInitializedIfNeeded(elementType, list, index);

                    VisualElement editor = CreateListElementEditor(
                        elementType,
                        list[index],
                        v =>
                        {
                            list[index] = v;
                            onChanged();
                        });

                    editor.style.flexGrow = 1.0f;

                    Button remove = new Button(() =>
                    {
                        list.RemoveAt(index);
                        foldout.text = key + " [" + GetNiceTypeName(elementType) + "] (" + list.Count + ")";
                        rebuildItems();
                        onChanged();
                    });

                    remove.text = "-";
                    remove.style.marginLeft = 6.0f;

                    row.Add(editor);
                    row.Add(remove);

                    itemsRoot.Add(row);
                }
            };

            Button addButton = new Button(() =>
            {
                object newValue = DefaultValueFactory.CreateDefaultDomValue(elementType);
                list.Add(newValue);

                foldout.text = key + " [" + GetNiceTypeName(elementType) + "] (" + list.Count + ")";
                rebuildItems();
                onChanged();
            });

            addButton.text = "Add";
            addButton.style.marginTop = 6.0f;

            rebuildItems();

            foldout.Add(itemsRoot);
            foldout.Add(addButton);

            return foldout;
        }

        private static void EnsureComplexElementInitializedIfNeeded(Type elementType, List<object> list, int index)
        {
            if (elementType == typeof(string) ||
                elementType == typeof(bool) ||
                elementType.IsEnum ||
                IsInteger(elementType) ||
                IsFloat(elementType))
            {
                return;
            }

            if (elementType.IsArray || IsList(elementType))
            {
                if (list[index] is List<object>)
                    return;

                list[index] = new List<object>();
                return;
            }

            if (list[index] is Dictionary<string, object>)
                return;

            list[index] = new Dictionary<string, object>();
        }

        private static VisualElement CreateListElementEditor(Type elementType, object currentValue, Action<object> setValue)
        {
            if (elementType == typeof(string))
            {
                TextField f = new TextField();
                f.value = currentValue != null ? Convert.ToString(currentValue, CultureInfo.InvariantCulture) : string.Empty;
                f.RegisterValueChangedCallback(evt => setValue(evt.newValue));
                return f;
            }

            if (elementType == typeof(bool))
            {
                Toggle t = new Toggle();
                t.value = ToBool(currentValue);
                t.RegisterValueChangedCallback(evt => setValue(evt.newValue));
                return t;
            }

            if (elementType.IsEnum)
            {
                long num = ToLong(currentValue);

                Array values = Enum.GetValues(elementType);
                object current = values.Length > 0 ? values.GetValue(0) : null;

                for (int i = 0; i < values.Length; i++)
                {
                    object v = values.GetValue(i);
                    long vv = Convert.ToInt64(v, CultureInfo.InvariantCulture);
                    if (vv == num)
                    {
                        current = v;
                        break;
                    }
                }

                EnumField ef = new EnumField(current as Enum);
                ef.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue == null)
                        setValue(0L);
                    else
                        setValue(Convert.ToInt64(evt.newValue, CultureInfo.InvariantCulture));
                });

                return ef;
            }

            if (IsInteger(elementType))
            {
                LongField f = new LongField();
                f.value = ToLong(currentValue);
                f.RegisterValueChangedCallback(evt => setValue(evt.newValue));
                return f;
            }

            if (IsFloat(elementType))
            {
                DoubleField f = new DoubleField();
                f.value = ToDouble(currentValue);
                f.RegisterValueChangedCallback(evt => setValue(evt.newValue));
                return f;
            }

            Dictionary<string, object> obj = currentValue as Dictionary<string, object>;
            if (obj == null)
            {
                obj = new Dictionary<string, object>();
            }

            Foldout fold = new Foldout();
            fold.text = GetNiceTypeName(elementType);
            fold.value = false;

            List<FieldInfo> fields = UnitySerializedFieldScanner.GetUnitySerializedFields(elementType);
            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo fi = fields[i];

                object child;
                obj.TryGetValue(fi.Name, out child);

                VisualElement childEditor = CreateEditor(
                    fi.Name,
                    fi.FieldType,
                    child,
                    obj,
                    () =>
                    {
                        setValue(obj);
                    },
                    2);

                fold.Add(childEditor);
            }

            return fold;
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

        private static bool ToBool(object o)
        {
            if (o == null)
                return false;

            if (o is bool b)
                return b;

            if (o is string s)
                return string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);

            if (o is long l)
                return l != 0;

            if (o is double d)
                return Math.Abs(d) > double.Epsilon;

            return false;
        }

        private static long ToLong(object o)
        {
            if (o == null)
                return 0L;

            if (o is long l)
                return l;

            if (o is int i)
                return i;

            if (o is double d)
                return (long)d;

            if (o is string s)
            {
                long parsed;
                if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                    return parsed;
            }

            return 0L;
        }

        private static double ToDouble(object o)
        {
            if (o == null)
                return 0.0;

            if (o is double d)
                return d;

            if (o is float f)
                return f;

            if (o is long l)
                return l;

            if (o is int i)
                return i;

            if (o is string s)
            {
                double parsed;
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                    return parsed;
            }

            return 0.0;
        }

        private static string GetNiceTypeName(Type t)
        {
            if (t == null)
                return "<null>";

            if (t.FullName != null)
                return t.FullName;

            return t.Name;
        }
    }
}
