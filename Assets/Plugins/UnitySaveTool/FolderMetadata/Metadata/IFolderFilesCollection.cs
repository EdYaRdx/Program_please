using System;
using System.Collections.Generic;

namespace UnitySaveTool
{
    public interface IFolderFilesCollection
    {
        void Set(object obj);
        void Reset(object obj);
        void ResetAll(Dictionary<Type, object> objects);

        void Remove(Type type);

        object Get(Type type);
        Dictionary<Type, object> GetAll();

        bool HasType(Type type);

        void SetWithoutConvertation(Type type, string json);
        void ResetWithoutConvertation(Type type, string json);
        void ResetAllWithoutConvertation(Dictionary<Type, string> jsonObjects);

        string GetWithoutConvertation(Type type);
        Dictionary<Type, string> GetAllWithoutConvertation();
    }
}