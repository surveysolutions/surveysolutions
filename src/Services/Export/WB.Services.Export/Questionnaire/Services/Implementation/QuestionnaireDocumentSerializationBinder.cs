using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace WB.Services.Export.Questionnaire.Services.Implementation
{
    public class QuestionnaireDocumentSerializationBinder : DefaultSerializationBinder
    {
        private static readonly Assembly Assembly = typeof(QuestionnaireDocument).Assembly;
        private static readonly ConcurrentDictionary<string, Type> ResolvedTypes = new();

        public override Type BindToType(string? assemblyName, string typeName)
        {
            return ResolvedTypes.GetOrAdd(typeName, tn =>
            {
                var type = Assembly.GetTypes().FirstOrDefault(x => x.IsPublic && x.Name.Equals(tn, StringComparison.Ordinal));

                if (type == null)
                    throw new Exception($"Type {tn} was not found in {Assembly.FullName}");
                return type;
            });
        }

        public override void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }
}
