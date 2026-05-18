namespace UnitySaveTool
{
    public interface IGameProgressDataContextManager
    {
        (ISaveContext global, ISaveContext gameProgress) GetGameProgressContext(int saveCellIndex);
    }
}