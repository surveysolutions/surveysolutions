using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Implementation.Services
{
    public class LocalizationService : ILocalizationService
    {
        public string GetString(string resourceKey)
        {
            return Strings.ResourceManager.GetString(resourceKey);
        }
    }
}