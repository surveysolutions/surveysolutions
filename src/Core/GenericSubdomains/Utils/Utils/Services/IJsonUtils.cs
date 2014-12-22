namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IJsonUtils
    {
        string Serialize(object item);
        byte[] SerializeToByteArray(object item);
        T Deserialize<T>(string payload);
        T Deserialize<T>(byte[] payload);
    }
}