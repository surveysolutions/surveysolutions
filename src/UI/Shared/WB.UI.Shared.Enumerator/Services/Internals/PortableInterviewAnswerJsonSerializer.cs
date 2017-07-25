using System;
using System.Collections.Generic;
using System.Reflection;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    public class PortableInterviewAnswerJsonSerializer : IInterviewAnswerSerializer
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public PortableInterviewAnswerJsonSerializer()
        {
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Include,
                FloatParseHandling = FloatParseHandling.Decimal,
                Formatting = Formatting.Indented,
                SerializationBinder = new InterviewAnswerSerializationBinder()
            };
        }

        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, this.jsonSerializerSettings);
        }

        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, this.jsonSerializerSettings);
        }

        public class InterviewAnswerSerializationBinder : DefaultSerializationBinder
        {
            private readonly Dictionary<string, Type> shortTypeNameToTypeMap = new Dictionary<string, Type>();

            public InterviewAnswerSerializationBinder()
            {
                var abstractAnswerType = typeof(AbstractAnswer);
                var assembly = abstractAnswerType.Assembly;

                List<Type> answerTypes = new List<Type>();
                foreach (TypeInfo typeInfo in assembly.DefinedTypes)
                {
                    if (abstractAnswerType.IsAssignableFrom(typeInfo) && !typeInfo.IsAbstract)
                    {
                        answerTypes.Add(typeInfo.AsType());
                    }
                }
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
                return this.shortTypeNameToTypeMap[typeName];
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = serializedType.Name;
            }
        }
    }
}