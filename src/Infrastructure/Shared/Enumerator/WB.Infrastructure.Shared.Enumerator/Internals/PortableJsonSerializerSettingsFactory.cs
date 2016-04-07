using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Portable.Services;
using Main.Core.Documents;
using System.Linq;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    public class PortableJsonSerializerSettingsFactory : IJsonSerializerSettingsFactory
    {
        private readonly JsonSerializerSettings AllTypesJsonSerializerSettings;
        private readonly JsonSerializerSettings ObjectsOnlyJsonSerializerSettings;

        public PortableJsonSerializerSettingsFactory()
        {
            PortableOldToNewAssemblyRedirectSerializationBinder oldToNewBinder = new PortableOldToNewAssemblyRedirectSerializationBinder();

            this.AllTypesJsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        NullValueHandling = NullValueHandling.Ignore,
                        FloatParseHandling = FloatParseHandling.Decimal,
                        Binder = oldToNewBinder
                };

            this.ObjectsOnlyJsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Binder = oldToNewBinder
            };
        }

        public JsonSerializerSettings GetAllTypesJsonSerializerSettings()
        {
            return AllTypesJsonSerializerSettings;
        }

        public JsonSerializerSettings GetObjectsJsonSerializerSettings()
        {
            return ObjectsOnlyJsonSerializerSettings;
        }

        private class PortableOldToNewAssemblyRedirectSerializationBinder : DefaultSerializationBinder
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
                
                foreach (var typeInfo in assembly.DefinedTypes)
                {
                    if (typesMap.ContainsKey(typeInfo.Name))
                        throw new InvalidOperationException("Assembly contains more then one type with same name.");

                    typesMap[typeInfo.Name] = typeInfo.FullName;
                    typeToName[typeInfo.AsType()] = typeInfo.Name;
                }
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
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
}