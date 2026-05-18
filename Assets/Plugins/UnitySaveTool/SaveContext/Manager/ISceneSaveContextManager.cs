namespace UnitySaveTool
{
    public interface ISceneSaveContextManager
    {
        (ISaveContext global, ISaveContext gameProgress, ISaveContext scene) GetSceneContext(string sceneName);
    }
}
