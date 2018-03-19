using System;
using System.Linq;

namespace WB.Infrastructure.Native.Storage
{
    [Obsolete]
    public class OldToNewAssemblyRedirectSerializationBinder : MainCoreAssemblyRedirectSerializationBaseBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (String.Equals(assemblyName, oldAssemblyNameToRedirect, StringComparison.Ordinal) ||
                String.Equals(assemblyName, targetAssemblyName, StringComparison.Ordinal) ||
                String.IsNullOrEmpty(assemblyName))
            {
                assemblyName = targetAssemblyName;
                string fullTypeName;

                if (typesMap.TryGetValue(typeName.Split('.').Last(), out fullTypeName))
                    typeName = fullTypeName;
            }
            else
            {
                //generic replace
                typeName = typeName.Replace(oldAssemblyGenericReplacePattern, newAssemblyGenericReplacePattern);
            }

            return base.BindToType(assemblyName, typeName);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            string name;
            if (typeToName.TryGetValue(serializedType, out name))
            {
                assemblyName = null;
                typeName = name;
            }
            else
            {
                assemblyName = serializedType.Assembly.FullName;
                typeName = serializedType.FullName;
            }
        }
    }
}
