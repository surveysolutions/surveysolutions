using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace WB.Services.Export.Questionnaire.Services.Implementation
{
    internal class QuestionnaireDocumentSerializationBinder : DefaultSerializationBinder
    {
        private static readonly Assembly Assembly = typeof(QuestionnaireDocument).Assembly;
        private static readonly ConcurrentDictionary<string, Type> ResolvedTypes = new ConcurrentDictionary<string, Type>();

        public override Type BindToType(string assemblyName, string typeName)
        {
            return ResolvedTypes.GetOrAdd(typeName, tn =>
            {
                return Assembly.GetTypes().FirstOrDefault(x => x.IsPublic && x.Name.Equals(tn, StringComparison.Ordinal));
            });
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }
}
