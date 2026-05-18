using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace UnitySaveTool
{
    public class PathFinder : IPathFinder
    {
        public string GlobalPath => _globalPath;

        private readonly string _globalPath;

        public PathFinder(string globalPath)
        {
            _globalPath = globalPath;
        }

        public string GetFullPath(bool pathMustExist, IEnumerable<string> folders)
        {
            if (Directory.Exists(_globalPath) == false)
                Directory.CreateDirectory(_globalPath);

            StringBuilder checkedPath = new(_globalPath);

            foreach (string folder in folders)
            {
                checkedPath.Append("/");
                checkedPath.Append(folder);

                string checkedPathString = checkedPath.ToString();

                if (pathMustExist && (Directory.Exists(checkedPathString) == false))
                    Directory.CreateDirectory(checkedPathString);
            }

            if (Directory.Exists(checkedPath.ToString()) == false)
                return null;

            return checkedPath.ToString();
        }
    }
}
