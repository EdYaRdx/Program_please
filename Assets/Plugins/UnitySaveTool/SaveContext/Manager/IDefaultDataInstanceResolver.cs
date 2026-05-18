using System;

namespace UnitySaveTool
{
    public interface IDefaultDataInstanceResolver
    {
        bool TryGetDefaultDataInstance(Type dataType, out object dataInstacne);
    }
}
