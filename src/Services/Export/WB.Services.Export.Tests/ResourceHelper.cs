using System.IO;
using System.Reflection;

namespace WB.Services.Export.Tests
{
    internal static class ResourceHelper
    {
        public static string ReadResourceFile(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = name;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
    }
}
