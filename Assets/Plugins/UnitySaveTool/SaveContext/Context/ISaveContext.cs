using System;
using System.Collections.Generic;

namespace UnitySaveTool
{
    public interface ISaveContext
    {
        ISaveContext GetChild(string folderInParent);

        void LoadDataToCache();

        void Save(object objectToSave);
        void Remove(Type type);

        void SaveAll(Dictionary<Type, object> objectsToSave);

        object Load(Type objectType);
        Dictionary<Type, object> LoadAll();
    }
}
