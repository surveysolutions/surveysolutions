using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ncqrs
{
    internal static class Utils
    {
        public static bool IsAssignableFrom(this Type someType, Type otherType)
        {
            return someType.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
        }

        public static PropertyInfo GetProperty(this Type someType, string propertyName)
        {
            return someType.GetTypeInfo().DeclaredProperties.FirstOrDefault(x => x.Name == propertyName);
        }

        public static MethodInfo GetMethod(this Type someType, string metodName)
        {
            return someType.GetTypeInfo().DeclaredMethods.FirstOrDefault(x => x.Name == metodName);
        }

        public static MethodInfo GetMethod(this Type someType, string metodName, Type[] types)
        {
            return someType.GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == metodName)
                .FirstOrDefault(x => x.GetParameters().Select(p => p.ParameterType).SequenceEqual(types));
        }

        public static IEnumerable<MethodInfo> GetMethods(this Type someType)
        {
            var t = someType;
            while (t != null)
            {
                var ti = t.GetTypeInfo();
                foreach (var m in ti.DeclaredMethods)
                    yield return m;
                t = ti.BaseType;
            }
        }
    }

    internal class NonSerializedAttribute : Attribute
    {
    }
}