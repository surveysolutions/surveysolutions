using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Portable.Services;
using System.Reflection;
using Main.Core.Documents;
using System.Linq;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    public class JsonSerializerSettingsFactory : IJsonSerializerSettingsFactory
    {
        private readonly Dictionary<TypeSerializationSettings, JsonSerializerSettings> jsonSerializerSettingsByTypeNameHandling;
        private Dictionary<string, string> assemblyRemappings = new Dictionary<string, string>();
        
        private OldToNewAssemblyRedirectSerializationBinder oldToNewBinder = new OldToNewAssemblyRedirectSerializationBinder();

        public JsonSerializerSettingsFactory()
        {
            jsonSerializerSettingsByTypeNameHandling =
               new Dictionary<TypeSerializationSettings, JsonSerializerSettings>()
                {
                    {
                        TypeSerializationSettings.AllTypes, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal
                        }
                    },
                    {
                        TypeSerializationSettings.ObjectsOnly, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Formatting = Formatting.None
                        }
                    },
                    {
                        TypeSerializationSettings.None, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.None,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Formatting = Formatting.None
                        }
                    },
                    {
                        TypeSerializationSettings.Auto, new JsonSerializerSettings()
                        {

                            TypeNameHandling = TypeNameHandling.Auto,
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            Formatting = Formatting.None
                        }
                   }
                };
        }

        public JsonSerializerSettings GetJsonSerializerSettings(TypeSerializationSettings typeSerialization)
        {
            return this.GetJsonSerializerSettings(typeSerialization, SerializationBinderSettings.OldToNew);
        }

        public JsonSerializerSettings GetJsonSerializerSettings(TypeSerializationSettings typeSerialization, SerializationBinderSettings binderSettings)
        {
            var settings = jsonSerializerSettingsByTypeNameHandling[typeSerialization];
            switch (binderSettings)
            {
                case SerializationBinderSettings.OldToNew:
                    settings.Binder = this.oldToNewBinder;
                    break;
            }

            return settings;
        }

        private class OldToNewAssemblyRedirectSerializationBinder : DefaultSerializationBinder
        {
            private const string oldAssemblyNameToRedirect = "Main.Core";
            private readonly string targetAssemblyName = "WB.Core.SharedKernels.Questionnaire";
            private readonly Dictionary<string, string> typesMap = new Dictionary<string, string>();
            private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();

            private const string oldAssemblyGenericReplacePattern = ", Main.Core]";
            private const string newAssemblyGenericReplacePattern = ", WB.Core.SharedKernels.Questionnaire]";

            public OldToNewAssemblyRedirectSerializationBinder()
            {
                typesMap = new Dictionary<string, string>();
                var assembly = typeof(QuestionnaireDocument).GetTypeInfo().Assembly;
                
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
                    typeName = serializedType.Name;
                }
                else
                {
                    assemblyName = serializedType.GetTypeInfo().Assembly.FullName;
                    typeName = serializedType.FullName;
                }
            }
        }
    }
}