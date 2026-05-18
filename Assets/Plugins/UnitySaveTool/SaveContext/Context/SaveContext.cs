using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace UnitySaveTool
{
    public class SaveContext : ISaveContext
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnumerable<string> _compositePath;

        [Inject]
        public SaveContext(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _compositePath = new string[0];
        }

        private SaveContext(IFileSystem fileSystem, IEnumerable<string> compositePath) : this(fileSystem)
        {
            _compositePath = compositePath;
        }

        public ISaveContext GetChild(string folderInParent)
        {
            return new SaveContext(_fileSystem, _compositePath.Append(folderInParent));
        }

        public void Save(object objectToSave)
        {
            _fileSystem.Save(objectToSave, _compositePath);
        }

        public void Remove(Type type)
        {
            _fileSystem.Remove(type, _compositePath);
        }

        public void SaveAll(Dictionary<Type, object> objectsToSave)
        {
            _fileSystem.SaveAll(objectsToSave, _compositePath);
        }

        public object Load(Type objectType)
        {
            return _fileSystem.Load(objectType, _compositePath);
        }

        public Dictionary<Type, object> LoadAll()
        {
            return _fileSystem.LoadAll(_compositePath); 
        }

        public void LoadDataToCache()
        {
            LoadAll();
        }
    }
}