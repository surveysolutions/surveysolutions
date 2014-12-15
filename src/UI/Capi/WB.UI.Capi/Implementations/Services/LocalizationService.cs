using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.Capi.Implementations.Services
{
    internal class LocalizationService : ILocalizationService
    {
        public string GetString(string resourceKey)
        {
            return Properties.Resource.ResourceManager.GetString(resourceKey);
        }
    }
}
