using System;
using System.Collections.Generic;

namespace UnitySaveTool
{
    public class FileSystem : IFileSystem
    {
        private readonly IPathFinder _pathFinder;
        private readonly IFolderMetadataFactory _metadataFactory;

        private readonly Dictionary<string, Dictionary<Type, object>> _cachedDirectoryes;

        public FileSystem(IFolderMetadataFactory metadataFactory, IPathFinder pathFinder)
        {
            _metadataFactory = metadataFactory;
            _pathFinder = pathFinder;

            _cachedDirectoryes = new();
        }

        public void Save(object objectToSave, params string[] folders)
        {
            SaveInternal(objectToSave, folders);
        }

        public void Save(object objectToSave, IEnumerable<string> folders)
        {
            SaveInternal(objectToSave, folders);
        }

        public void Remove(Type type, params string[] folders)
        {
            RemoveInternal(type, folders);
        }

        public void Remove(Type type, IEnumerable<string> folders)
        {
            RemoveInternal(type, folders);
        }

        public void SaveAll(Dictionary<Type, object> objectsToSave, params string[] folders)
        {
            SaveAllInternal(objectsToSave, folders);
        }

        public void SaveAll(Dictionary<Type, object> objectsToSave, IEnumerable<string> folders)
        {
            SaveAllInternal(objectsToSave, folders);
        }

        public object Load(Type objectType, params string[] folders)
        {
            return LoadInternal(objectType, folders);
        }

        public object Load(Type objectType, IEnumerable<string> folders)
        {
            return LoadInternal(objectType, folders);
        }

        public Dictionary<Type, object> LoadAll(params string[] folders)
        {
            return LoadAllInternal(folders);
        }

        public Dictionary<Type, object> LoadAll(IEnumerable<string> folders)
        {
            return LoadAllInternal(folders);
        }

        public void RemoveInternal(Type type, IEnumerable<string> folders)
        {
            string path = _pathFinder.GetFullPath(false, folders);

            if (path == null)
                return;

            IFolderFilesCollection filesCollection = _metadataFactory.GetFilesCollection(path);

            filesCollection.Remove(type);

            if (_cachedDirectoryes.ContainsKey(path) == false)
                return;

            if (_cachedDirectoryes[path].ContainsKey(type))
                _cachedDirectoryes[path].Remove(type);
        }

        public Dictionary<Type, object> LoadAllInternal(IEnumerable<string> folders)
        {
            string path = _pathFinder.GetFullPath(false, folders);

            if (path == null)
                return new();

            if (_cachedDirectoryes.ContainsKey(path))
                return _cachedDirectoryes[path];

            IFolderFilesCollection filesCollection = _metadataFactory.GetFilesCollection(path);

            var loadedAll = filesCollection.GetAll();

            _cachedDirectoryes[path] = loadedAll;

            return loadedAll;
        }

        private object LoadInternal(Type objectType, IEnumerable<string> folders)
        {
            string path = _pathFinder.GetFullPath(false, folders);

            if (path == null)
                return null;

            if (_cachedDirectoryes.ContainsKey(path) && _cachedDirectoryes[path].ContainsKey(objectType))
                return _cachedDirectoryes[path][objectType];

            IFolderFilesCollection filesCollection = _metadataFactory.GetFilesCollection(path);

            object loaded = filesCollection.Get(objectType);

            if (loaded != null)
                SaveCacheObject(loaded, path);

            return loaded;
        }

        private void SaveAllInternal(Dictionary<Type, object> objectsToSave, IEnumerable<string> folders)
        {
            foreach (object obj in objectsToSave.Values)
            {
                AssertThatReferenceType(obj);
            }

            string path = _pathFinder.GetFullPath(true, folders);

            IFolderFilesCollection filesCollection = _metadataFactory.GetFilesCollection(path);

            filesCollection.ResetAll(objectsToSave);

            SaveCacheObjects(objectsToSave, path);
        }

        private void SaveInternal(object objectToSave, IEnumerable<string> folders)
        {
            AssertThatReferenceType(objectToSave);

            string path = _pathFinder.GetFullPath(true, folders);

            IFolderFilesCollection filesCollection = _metadataFactory.GetFilesCollection(path);

            filesCollection.Reset(objectToSave);

            SaveCacheObject(objectToSave, path);
        }

        private void SaveCacheObjects(Dictionary<Type, object> objectsToSave, string path)
        {
            if (_cachedDirectoryes.ContainsKey(path) == false)
                _cachedDirectoryes.Add(path, new());

            _cachedDirectoryes[path] = new Dictionary<Type, object>(objectsToSave);
        }

        private void SaveCacheObject(object objectToSave, string path)
        {
            if (_cachedDirectoryes.ContainsKey(path) == false)
                _cachedDirectoryes.Add(path, new());

            Type objectType = objectToSave.GetType();

            if (_cachedDirectoryes[path].ContainsKey(objectType) == false)
                _cachedDirectoryes[path].Add(objectType, null);

            _cachedDirectoryes[path][objectType] = objectToSave;
        }

        private void AssertThatReferenceType(object obj)
        {
            if (obj.GetType().IsValueType)
                throw new ArgumentException("Only reference types can be saved");
        }
    }
}
