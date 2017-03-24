namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IRandomValuesSource
    {
        int Next(int maxInterviewKeyValue);
    }
}