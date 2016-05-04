using Newtonsoft.Json;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IJsonSerializerSettingsFactory
    {
        JsonSerializerSettings GetJsonSerializerSettings(TypeSerializationSettings typeSerialization, SerializationBinderSettings binderSettings = SerializationBinderSettings.OldToNew);
    }
}
