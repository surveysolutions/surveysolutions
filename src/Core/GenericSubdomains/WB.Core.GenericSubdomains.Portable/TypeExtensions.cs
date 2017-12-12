using System;
using System.Reflection;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class TypeExtensions
    {
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.GetTypeInfo().BaseType;
            }
            return false;
        }

        public static T To<T>(this object obj)
        {
            if (obj is T variable) return variable;

            Type t = typeof(T);
            Type u = Nullable.GetUnderlyingType(t);
            Type o = obj?.GetType();

            if (u != null)
            {
                return (obj == null) ? default(T) : (T)Convert.ChangeType(obj, u);
            }

            if (t == o)
                return (T) obj;

            return (T)Convert.ChangeType(obj, t);
        }
    }
}