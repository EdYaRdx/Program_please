using System;
using System.Collections.Generic;

namespace UnitySaveTool
{
    public interface IFileSystem
    {
        void Save(object objectToSave, params string[] folders);
        void Save(object objectToSave, IEnumerable<string> folders);

        void SaveAll(Dictionary<Type, object> objectsToSave, params string[] folders);
        void SaveAll(Dictionary<Type, object> objectsToSave, IEnumerable<string> folders);

        void Remove(Type type, params string[] folders);
        void Remove(Type type, IEnumerable<string> folders);

        object Load(Type objectType, params string[] folders);
        object Load(Type objectType, IEnumerable<string> folders);

        Dictionary<Type, object> LoadAll(params string[] folders);
        Dictionary<Type, object> LoadAll(IEnumerable<string> folders);
    }
}