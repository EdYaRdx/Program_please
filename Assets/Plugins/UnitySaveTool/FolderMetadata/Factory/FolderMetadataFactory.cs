using System.Collections.Generic;

namespace UnitySaveTool
{
    public class FolderMetadataFactory : IFolderMetadataFactory
    {
        private readonly IDataConverter _dataConverter;

        private readonly Dictionary<string, FolderMetadata> _cachedMetadata;

        public FolderMetadataFactory(IDataConverter dataConverter)
        {
            _dataConverter = dataConverter;

            _cachedMetadata = new();
        }

        public IFolderFilesCollection GetFilesCollection(string path)
        {
            if (_cachedMetadata.TryGetValue(path, out FolderMetadata metadata) == false)
            {
                metadata = FolderMetadata.GetFilesCollection(path, _dataConverter);

                _cachedMetadata[path] = metadata;
            }

            return metadata;
        }
    }
}
