using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.SharedKernels.DataCollection.Scenarios;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.UI.WebTester.Infrastructure
{
    public interface IScenarioSerializer
    {
        string Serialize(Scenario scenario);

        Scenario? Deserialize(string scenario);
    }

    class ScenarioSerializer : IScenarioSerializer
    {
        private readonly JsonSerializerSettings settings;

        public ScenarioSerializer()
        {
            this.settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>
                {
                    new RosterVectorConverter()
                },
                SerializationBinder = new ScenarioCommandSerializationBinder()
            };
        }

        public string Serialize(Scenario scenario)
        {
            return JsonConvert.SerializeObject(scenario, this.settings);
        }

        public Scenario? Deserialize(string scenario)
        {
            return JsonConvert.DeserializeObject<Scenario>(scenario, this.settings);
        }
    }

    internal class ScenarioCommandSerializationBinder : DefaultSerializationBinder
    {
        private readonly Dictionary<string, Type> shortTypeNameToTypeMap = new Dictionary<string, Type>();

        public ScenarioCommandSerializationBinder()
        {
            var scenarioCommand = typeof(IScenarioCommand);
            var assembly = scenarioCommand.Assembly;

            List<Type> commandTypes = assembly.DefinedTypes.Where(x => scenarioCommand.IsAssignableFrom(x) && !x.IsAbstract).Select(x => x.AsType()).ToList();

            foreach (var typeInfo in commandTypes)
            {
                var shortTypeName = typeInfo.Name;
                if (this.shortTypeNameToTypeMap.ContainsKey(shortTypeName))
                    throw new InvalidOperationException("Assembly contains more then one type with same name.");

                this.shortTypeNameToTypeMap[shortTypeName] = typeInfo;
            }
        }

        public override Type BindToType(string? assemblyName, string typeName)
        {
            return shortTypeNameToTypeMap[typeName];
        }

        public override void BindToName(Type serializedType, out string? assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }
}
