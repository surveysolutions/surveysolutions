using System;
using System.Reflection;
using System.Text;

namespace Mono.Android.Crasher.Data.Collectors
{
    /// <summary>
    /// Tools to retrieve key/value pairs from static Properties of any
    /// class. Reflection API usage allows to retrieve data without having to
    /// implement a class for each android version of each intersting class.
    /// </summary>
    static class ReflectionCollector
    {
        /// <summary>
        /// Retrieves key/value pairs from static Properties of a class.
        /// </summary>
        /// <param name="someClass">the <see cref="Type"/> to be inspected</param>
        /// <returns>A human readable string with a key=value pair on each line.</returns>
        public static String CollectStaticProperties(Type someClass)
        {
            var result = new StringBuilder();
            foreach (var property in someClass.GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                result.Append(property.Name).Append('=');
                try
                {
                    result.Append(property.GetValue(null, null)).AppendLine();
                }
                catch (Exception)
                {
                    result.Append("N/A");
                }
            }
            return result.ToString();
        }
    }
}