namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface ISerializer
    {
        string Serialize(object item);

        string SerializeWithoutTypes(object item);

        T DeserializeWithoutTypes<T>(string payload);

        T Deserialize<T>(string payload);
    }
}
