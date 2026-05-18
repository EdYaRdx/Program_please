using System.IO;

namespace UnitySaveTool
{
    public static class SafeFile
    {
        public static void WriteAllTextWithBackup(string path, string text)
        {
            SetPaths(path, out string tmp, out string bak);

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(tmp, text);

            if (File.Exists(path))
            {
                SafeDelete(bak);

                File.Move(path, bak);
            }

            File.Move(tmp, path);
        }

        public static void Delete(string path)
        {
            SetPaths(path, out string tmp, out string bak);

            SafeDelete(path);
            SafeDelete(bak);
            SafeDelete(tmp);
        }

        public static void RecoverIfMainMissing(string path)
        {
            SetPaths(path, out string tmp, out string bak);

            if (File.Exists(path))
            {
                SafeDelete(tmp);

                return;
            }

            if (File.Exists(bak))
                File.Move(bak, path);

            SafeDelete(tmp);
        }

        private static void SafeDelete(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void SetPaths(string mainPath, out string tmp, out string bak)
        {
            tmp = mainPath + ".tmp";
            bak = mainPath + ".bak";
        }
    }
}