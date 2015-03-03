namespace WB.Core.GenericSubdomains.Utils.Services
{
    public enum TypeSerializationSettings
    {
        ObjectsOnly,
        AllTypes
    }

    public interface IJsonUtils
    {
        string Serialize(object item);
        string Serialize(object item, TypeSerializationSettings typeSerializationSettings);
        byte[] SerializeToByteArray(object item);
        T Deserialize<T>(string payload);
        T Deserialize<T>(byte[] payload);
    }
}