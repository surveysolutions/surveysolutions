using System;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Main.Core.Documents;
using System.Linq;

namespace WB.Infrastructure.Native.Storage
{
    internal class OldToNewAssemblyRedirectSerializationBinder : DefaultSerializationBinder
    {
        private const string oldAssemblyNameToRedirect = "Main.Core";
        private readonly string targetAssemblyName = "WB.Core.SharedKernels.Questionnaire";

        private readonly Dictionary<string, string> typesMap = new Dictionary<string, string>();

        readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();

        private const string oldAssemblyGenericReplacePattern = ", Main.Core]";
        private const string newAssemblyGenericReplacePattern = ", WB.Core.SharedKernels.Questionnaire]";

        public OldToNewAssemblyRedirectSerializationBinder()
        {
            var assembly = typeof (QuestionnaireDocument).Assembly;

            foreach (var type in assembly.GetTypes().Where(t => t.IsPublic))
            {
                if (typesMap.ContainsKey(type.Name))
                    throw new InvalidOperationException("Assembly contains more then one type with same name.");

                typesMap[type.Name] = type.FullName;
                typeToName[type] = type.Name;
            }

        }

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
