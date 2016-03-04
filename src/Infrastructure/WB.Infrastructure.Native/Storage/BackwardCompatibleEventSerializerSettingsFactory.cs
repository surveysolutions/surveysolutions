using Newtonsoft.Json;

namespace WB.Infrastructure.Native.Storage
{
    public class BackwardCompatibleEventSerializerSettingsFactory : IEventSerializerSettingsFactory
    {
        public JsonSerializerSettings GetJsonSerializerSettings()
        {
            return EventSerializerSettings.BackwardCompatibleJsonSerializerSettings;
        }
    }
}
