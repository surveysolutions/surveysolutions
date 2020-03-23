namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IPreferencesStorage
    {
        void Store(string key, string value);
        string Get(string key);
    }
}
