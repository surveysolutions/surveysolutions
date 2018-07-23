using System;
using System.Linq;
using System.Reflection;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class ReflectionUtils
    {
        public static Version GetAssemblyVersion(Type typeInAssembly)
        {
            Assembly executingAssembly = Assembly.GetAssembly(typeInAssembly);
            var customAttributes = executingAssembly.GetCustomAttributes();
            var attribute = customAttributes.OfType<AssemblyFileVersionAttribute>().First();
            return new Version(attribute.Version);
        }
    }
}
