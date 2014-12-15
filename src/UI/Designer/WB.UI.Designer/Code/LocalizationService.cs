using Resources;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.Designer.Code
{
    public class LocalizationService : ILocalizationService
    {
        public string GetString(string resourceKey)
        {
            return Strings.ResourceManager.GetString(resourceKey);
        }
    }
}