using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ncqrs
{
    internal static class Utils
    {
        public static PropertyInfo GetProperty(this Type someType, string propertyName)
        {
            return someType.GetTypeInfo().DeclaredProperties.FirstOrDefault(x => x.Name == propertyName);
        }
    }

    internal class NonSerializedAttribute : Attribute
    {
    }
}