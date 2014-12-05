namespace WB.Core.SharedKernels.SurveySolutions.Services
{
    public interface IJsonUtils
    {
        string GetItemAsContent(object item);
        T Deserrialize<T>(string payload);
    }
}