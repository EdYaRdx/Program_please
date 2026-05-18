namespace UnitySaveTool
{
    public interface IFolderMetadataFactory
    {
        IFolderFilesCollection GetFilesCollection(string path);
    }
}
