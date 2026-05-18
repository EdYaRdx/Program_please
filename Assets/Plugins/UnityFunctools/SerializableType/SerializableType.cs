using System;
using UnityEditor;
using UnityEngine;

namespace UnitySaveTool.Tools
{
    [Serializable]
    public class SerializableType
    {
#if UNITY_EDITOR
        public const string ScriptField = nameof(_script);
        public const string ClassInfoField = nameof(_classInfo);

        [SerializeField] private MonoScript _script;
#endif
        [HideInInspector]
        [SerializeField]
        private string _classInfo;

        public SerializableType() { }

        public SerializableType(Type type)
        {
            SetValue(type);
        }

        public bool TryGetValue(out Type type)
        {
            type = null;

            if (_classInfo == null)
                return false;

            type = Type.GetType(_classInfo);
            return true;
        }

        public Type GetValue()
        {
            if (TryGetValue(out Type type) == false)
                throw new ArgumentException();

            return type;
        }

        public void SetValue(Type type)
        {
            _classInfo = $"{type.FullName}, {type.Assembly.FullName}";
        }
    }
}