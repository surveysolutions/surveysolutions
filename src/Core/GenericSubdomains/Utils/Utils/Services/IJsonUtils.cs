namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IJsonUtils
    {
        string GetItemAsContent(object item);
        T Deserrialize<T>(string payload);
    }
}