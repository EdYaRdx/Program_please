using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnitySaveTool.Tools;

namespace UnitySaveTool
{
    [Serializable]
    public sealed class FolderMetadata : ISerializationCallbackReceiver, IFolderFilesCollection
    {
        [SerializeField] private SerializableType[] _serializableTypes;
        [SerializeField] private string _folderPath;

        private HashSet<Type> _types;
        private IDataConverter _dataConverter;

        public static FolderMetadata GetFilesCollection(string folderPath, IDataConverter dataConverter)
        {
            FolderMetadata folderMetadata;

            string path = GetFullPath(folderPath, typeof(FolderMetadata));

            SafeFile.RecoverIfMainMissing(path);

            if (File.Exists(path) == false)
            {
                folderMetadata = new(folderPath);
            }
            else
            {
                string metadataString = File.ReadAllText(path);

                object metadataObj = dataConverter.ConvertToObject(metadataString, typeof(FolderMetadata));

                if (metadataObj is not FolderMetadata folderMetadataExample)
                    throw new Exception();

                folderMetadata = folderMetadataExample;
            }

            folderMetadata._dataConverter = dataConverter;

            return folderMetadata;
        }

        private FolderMetadata(string folderPath)
        {
            _folderPath = folderPath;
            _types = new();
        }

        public void OnAfterDeserialize()
        {
            _types = new();

            foreach (SerializableType serializableType in _serializableTypes)
                _types.Add(serializableType.GetValue());
        }

        public void OnBeforeSerialize()
        {
            _serializableTypes = _types.Select(t => new SerializableType(t)).ToArray();
        }

        public void Set(object obj)
        {
            SetWithoutConvertation(obj.GetType(), _dataConverter.ConvertFromObject(obj));
        }

        public void SetWithoutConvertation(Type type, string json)
        {
            SetInternal(type, json);

            Save();
        }

        private void SetInternal(Type type, string json)
        {
            if (_types.Contains(type))
                throw new Exception();

            UpsertInternal(type, json);
        }

        private void UpsertInternal(object obj)
        {
            UpsertInternal(obj.GetType(), _dataConverter.ConvertFromObject(obj));
        }

        private void UpsertInternal(Type type, string json)
        {
            if (type == typeof(FolderMetadata))
                throw new Exception();

            string path = GetFullPath(type);

            SafeFile.WriteAllTextWithBackup(path, json);
            _types.Add(type);
        }

        public void Reset(object obj)
        {
            ResetWithoutConvertation(obj.GetType(), _dataConverter.ConvertFromObject(obj));
        }

        public void ResetWithoutConvertation(Type type, string json)
        {
            UpsertInternal(type, json);
            Save();
        }

        public void ResetAll(Dictionary<Type, object> objects)
        {
            ResetAllWithoutConvertation(new(objects.Select(p => new KeyValuePair<Type, string>(p.Key, _dataConverter.ConvertFromObject(p.Value)))));
        }

        public void ResetAllWithoutConvertation(Dictionary<Type, string> jsonObjects)
        {
            foreach (Type key in jsonObjects.Keys)
            {
                UpsertInternal(key, jsonObjects[key]);
            }

            Save();
        }

        public void Remove(Type type)
        {
            if (RemoveInternal(type))
                Save();
        }

        private bool RemoveInternal(Type type)
        {
            if (_types.Contains(type) == false)
                return false;

            string path = GetFullPath(type);

            SafeFile.Delete(path);

            _types.Remove(type);
            return true;
        }

        public object Get(Type type)
        {
            return _dataConverter.ConvertToObject(GetWithoutConvertation(type), type);
        }

        public string GetWithoutConvertation(Type type)
        {
            if (_types.Contains(type) == false)
                return null;

            string path = GetFullPath(type);

            SafeFile.RecoverIfMainMissing(path);
            string objectString = File.ReadAllText(path);

            return objectString;
        }

        public Dictionary<Type, object> GetAll()
        {
            return new(GetAllWithoutConvertation().Select(p => new KeyValuePair<Type, object>(p.Key, _dataConverter.ConvertToObject(p.Value, p.Key))));
        }

        public Dictionary<Type, string> GetAllWithoutConvertation()
        {
            Dictionary<Type, string> deserializedJsons = new();

            foreach (Type type in _types)
                deserializedJsons.Add(type, GetWithoutConvertation(type));

            return deserializedJsons;
        }

        public bool HasType(Type type)
        {
            return _types.Contains(type);
        }

        private void Save()
        {
            string path = GetFullPath(typeof(FolderMetadata));
            SafeFile.WriteAllTextWithBackup(path, _dataConverter.ConvertFromObject(this));
        }

        private string GetFullPath(Type type)
        {
            return GetFullPath(_folderPath, type);
        }

        private static string GetFullPath(string folderPath, Type type)
        {
            return $"{folderPath}/{type.FullName}.json";
        }
    }
}
