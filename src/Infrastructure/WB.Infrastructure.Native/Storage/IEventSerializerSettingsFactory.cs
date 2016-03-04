using Newtonsoft.Json;

namespace WB.Infrastructure.Native.Storage
{
    public interface IEventSerializerSettingsFactory
    {
        JsonSerializerSettings GetJsonSerializerSettings();
    }
}
