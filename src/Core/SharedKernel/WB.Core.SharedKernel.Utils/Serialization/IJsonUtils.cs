namespace WB.Core.SharedKernel.Utils.Serialization
{
    public interface IJsonUtils
    {
        string GetItemAsContent(object item);
        T Deserrialize<T>(string payload);
    }
}