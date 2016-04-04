namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface ISynchronizationSerializer
    {
        string Serialize(object item);
        string Serialize(object item, TypeSerializationSettings typeSerializationSettings);
        string Serialize(object item, TypeSerializationSettings typeSerializationSettings, SerializationBinderSettings binderSettings);

        T Deserialize<T>(byte[] payload);
        T Deserialize<T>(string payload);
        T Deserialize<T>(string payload, TypeSerializationSettings typeSerializationSettings);
    }
}