using System.IO;
using System.Reflection;

namespace Tools
{
    public static class SettingsFile
    {
        public static string GetDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:", "");
        }

        public static string LoadJson(string filePath)
        {
            var fullPath = Path.Join(GetDirectory(), filePath);
            return File.ReadAllText(Path.Join(GetDirectory(), filePath));
        }

        public static void Save(string filePath, string content)
        {
            File.WriteAllText(Path.Join(GetDirectory(), filePath), content);
        }
    }
}