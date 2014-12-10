using System.IO;
using System.Reflection;

namespace WB.Tests.Unit.BoundedContexts.Supervisor
{
    public static class ResourceHelper
    {
        public static string ReadResourceFile(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = name;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
    }
}