using System;
using System.IO;
using System.Reflection;

namespace Tools
{
    public static class SettingsFile
    {
        public static string Base64(this string input)
        {
            if (input == null) {
                input = "";
            }
            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(input);
            return System.Convert.ToBase64String(data);
        }

        public static string GetLocation(string filename)
        {
            return Path.Join(GetDirectory(), filename);
        }

        public static string GetDirectory()
        {
            var appDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var appConfigPath = Path.Join(appDir, ".config", "NoIPUpdateTool");
            if (!Directory.Exists(appConfigPath)) {
                Directory.CreateDirectory(appConfigPath);
            }
            return appConfigPath;
        }

        public static bool Exists(string filePath)
        {
            var fullPath = Path.Join(GetDirectory(), filePath);
            return File.Exists(fullPath);
        }

        public static string LoadJson(string filePath)
        {
            return File.ReadAllText(Path.Join(GetDirectory(), filePath));
        }

        public static void Save(string filePath, string content)
        {
            File.WriteAllText(Path.Join(GetDirectory(), filePath), content);
        }
    }
}
