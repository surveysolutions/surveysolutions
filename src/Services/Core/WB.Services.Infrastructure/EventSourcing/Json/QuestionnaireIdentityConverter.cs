using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WB.ServicesIntegration.Export;

namespace WB.Services.Infrastructure.EventSourcing.Json
{
    public class QuestionnaireIdentityConverter : JsonConverter<QuestionnaireIdentity>
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, QuestionnaireIdentity? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override QuestionnaireIdentity ReadJson(JsonReader reader, 
            Type objectType, 
            QuestionnaireIdentity? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jObject = serializer.Deserialize<JObject>(reader);
            if (jObject == null) 
                return new QuestionnaireIdentity();
            if (jObject["Id"]?.Value<string>()?.Contains("$") == true)
            {
                return new QuestionnaireIdentity(jObject["Id"]!.Value<string>());
            }
            
            QuestionnaireIdentity? result = jObject.ToObject(objectType, serializer) as QuestionnaireIdentity;
            if (result == null) throw new SerializationException("Failed to deserialize questionnaire identity");
            return result;
        }
    }
}
