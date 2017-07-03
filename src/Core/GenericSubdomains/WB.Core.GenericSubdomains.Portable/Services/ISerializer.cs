namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface ISerializer
    {
        string Serialize(object item);

        T Deserialize<T>(string payload);
    }
}