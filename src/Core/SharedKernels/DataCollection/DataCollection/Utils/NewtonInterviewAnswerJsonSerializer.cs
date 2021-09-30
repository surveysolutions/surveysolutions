using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public class NewtonInterviewAnswerJsonSerializer : IInterviewAnswerSerializer
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            NullValueHandling = NullValueHandling.Include,
            FloatParseHandling = FloatParseHandling.Decimal,
            Formatting = Formatting.Indented,
            SerializationBinder = new InterviewAnswerSerializationBinder(),
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            Converters = new List<JsonConverter>
            {
                new IdentityJsonConverter(),
                new RosterVectorConverter()
            },
            ContractResolver = new NewtonInterviewAnswerContractResolver()
        };

        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, jsonSerializerSettings);
        }

        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, jsonSerializerSettings);
        }
        
        public class InterviewAnswerSerializationBinder : DefaultSerializationBinder
        {
            private readonly Dictionary<string, Type> shortTypeNameToTypeMap = new Dictionary<string, Type>();

            public InterviewAnswerSerializationBinder()
            {
                var abstractAnswerType = typeof(AbstractAnswer);
                var assembly = abstractAnswerType.Assembly;

                List<Type> answerTypes = assembly.DefinedTypes.Where(x => abstractAnswerType.IsAssignableFrom(x) && !x.IsAbstract).Select(x => x.AsType()).ToList();
                answerTypes.Add(typeof(Identity));
                answerTypes.Add(typeof(InterviewAnswer));
                answerTypes.Add(typeof(TextListAnswerRow));
                answerTypes.Add(typeof(GeoPosition));
                answerTypes.Add(typeof(CheckedYesNoAnswerOption));

                foreach (var typeInfo in answerTypes)
                {
                    if (this.shortTypeNameToTypeMap.ContainsKey(typeInfo.Name))
                        throw new InvalidOperationException("Assembly contains more then one type with same name.");

                    this.shortTypeNameToTypeMap[typeInfo.Name] = typeInfo;
                }
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                return shortTypeNameToTypeMap[typeName];
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = serializedType.Name;
            }
        }
    }

    public class NewtonInterviewAnswerContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasNonPublicSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasNonPublicSetter;
                }
            }

            return prop;
        }
    }
}
