using Newtonsoft.Json;

namespace WB.Infrastructure.Native.Storage
{
    public class DefaultEventSerializerSettingsFactory : IEventSerializerSettingsFactory
    {
        public JsonSerializerSettings GetJsonSerializerSettings()
        {
            return EventSerializerSettings.JsonSerializerSettings;
        }
    }
}
