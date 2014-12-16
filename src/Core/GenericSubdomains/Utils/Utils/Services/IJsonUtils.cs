namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IJsonUtils
    {
        string GetItemAsContent(object item);
        byte[] SerializeToByteArray(object item);
        T Deserrialize<T>(string payload);
        T Deserrialize<T>(byte[] payload);
    }
}