
namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public class GoogleApiSettings
    {
        public string ApiKey { get; private set; }

        public GoogleApiSettings(string apiKey)
        {
            this.ApiKey = apiKey;
        }
    }
}