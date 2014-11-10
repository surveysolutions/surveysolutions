using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ncqrs
{
    internal enum BindingFlags
    {
        
    }
    internal class SerializableAttribute : Attribute
	{
	}

	internal class NonSerializedAttribute : Attribute
	{
	}

    internal static class Utils
    {
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
}
