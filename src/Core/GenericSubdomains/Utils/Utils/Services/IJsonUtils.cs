namespace WB.Core.GenericSubdomains.Utils.Services
{
    public enum TypeSerializationSettings
    {
        ObjectsOnly,
        AllTypes
    }

    public interface IJsonUtils
    {
        string Serialize(object item, TypeSerializationSettings typeSerializationSettings = TypeSerializationSettings.ObjectsOnly);
        byte[] SerializeToByteArray(object item);
        T Deserialize<T>(string payload);
        T Deserialize<T>(byte[] payload);
    }
}