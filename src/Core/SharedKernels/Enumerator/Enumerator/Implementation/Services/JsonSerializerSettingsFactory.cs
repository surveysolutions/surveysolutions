using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class JsonSerializerSettingsFactory : IJsonSerializerSettingsFactory
    {
        private readonly Dictionary<TypeSerializationSettings, JsonSerializerSettings> jsonSerializerSettingsByTypeNameHandling;
        private Dictionary<string, string> assemblyRemappings = new Dictionary<string, string>();

        public JsonSerializerSettingsFactory(Dictionary<string, string> assemblyRemappings = null)
        {
            this.assemblyRemappings = assemblyRemappings;

            var binder = assemblyRemappings != null && assemblyRemappings.Count > 0 
                ? new AssemblyRedirectSerializationBinder(assemblyRemappings) 
                : new DefaultSerializationBinder();

            jsonSerializerSettingsByTypeNameHandling =
               new Dictionary<TypeSerializationSettings, JsonSerializerSettings>()
                {
                    {
                        TypeSerializationSettings.AllTypes, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Binder = binder
                        }
                    },
                    {
                        TypeSerializationSettings.ObjectsOnly, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Formatting = Formatting.None,
                            Binder = binder
                        }
                    },
                    {
                        TypeSerializationSettings.None, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.None,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Formatting = Formatting.None,
                            Binder = binder
                        }
                    },

                    {
                        TypeSerializationSettings.Auto, new JsonSerializerSettings()
                        {

                            TypeNameHandling = TypeNameHandling.Auto,
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            Formatting = Formatting.None,
                            Binder = binder
                        }
                   },
                    {
                        TypeSerializationSettings.Event, new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            TypeNameHandling = TypeNameHandling.Auto,
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            Converters = new JsonConverter[] { new StringEnumConverter() }
                        }
                    }
                };
        }

        public JsonSerializerSettings GetJsonSerializerSettings(TypeSerializationSettings typeSerialization)
        {
            return jsonSerializerSettingsByTypeNameHandling[typeSerialization];
        }

        private class AssemblyRedirectSerializationBinder : DefaultSerializationBinder
        {
            private Dictionary<string, string> assemblyNamesMapping = new Dictionary<string, string>();

            public AssemblyRedirectSerializationBinder() { }

            public AssemblyRedirectSerializationBinder(Dictionary<string, string> assemblyNamesMapping)
            {
                this.assemblyNamesMapping = assemblyNamesMapping;
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                if (this.assemblyNamesMapping.ContainsKey(assemblyName))
                    assemblyName = this.assemblyNamesMapping[assemblyName];

                return base.BindToType(assemblyName, typeName);
            }
        }
    }
}