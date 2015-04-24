using System;

namespace WB.UI.QuestionnaireTester.Utils
{
    public static class TypeUtils
    {
        public static bool IsImplementationOfGenericType(this Type type, Type genericType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
        }
    }
}