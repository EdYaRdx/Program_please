using System.Collections.Generic;

namespace UnitySaveTool
{
    public interface IPathFinder
    {
        string GetFullPath(bool pathMustExist, IEnumerable<string> folders);
    }
}