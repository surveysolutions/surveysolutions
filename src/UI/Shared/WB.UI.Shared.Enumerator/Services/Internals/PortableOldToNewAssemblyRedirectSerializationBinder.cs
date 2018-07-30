using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Newtonsoft.Json.Serialization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    public class PortableOldToNewAssemblyRedirectSerializationBinder : DefaultSerializationBinder
    {
        private const string oldAssemblyNameToRedirect = "Main.Core";
        private const string targetAssemblyName = "WB.Core.SharedKernels.Questionnaire";

        private readonly Dictionary<string, string> typesMap = new Dictionary<string, string>();
        private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();

        private const string oldAssemblyGenericReplacePattern = ", Main.Core";
        private const string newAssemblyGenericReplacePattern = ", WB.Core.SharedKernels.Questionnaire";

        public PortableOldToNewAssemblyRedirectSerializationBinder()
        {
            var assembly = typeof(QuestionnaireDocument).Assembly;

            foreach (var typeInfo in assembly.DefinedTypes.Where(x => !x.Name.StartsWith("<")))
            {
                if (typesMap.ContainsKey(typeInfo.Name))
                    throw new InvalidOperationException($"Assembly contains more then one type with same name. Duplicate name: {typeInfo.Name}");

                typesMap[typeInfo.Name] = typeInfo.FullName;
                typeToName[typeInfo.AsType()] = typeInfo.Name;
            }
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            // Can be removed when all interivewers of version < 18.08 are synchronized with server
            if ("WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.AuditLogEntityView".Equals(typeName, StringComparison.Ordinal))
                return typeof(AuditLogEntityView);

            if (String.Equals(assemblyName, oldAssemblyNameToRedirect, StringComparison.Ordinal) ||
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
