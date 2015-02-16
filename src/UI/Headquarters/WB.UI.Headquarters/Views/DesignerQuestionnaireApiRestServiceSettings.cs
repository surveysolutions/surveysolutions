using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Headquarters.Views
{
    public class DesignerQuestionnaireApiRestServiceSettings : IRestServiceSettings
    {
        private readonly IConfigurationManager configurationManager;

        public DesignerQuestionnaireApiRestServiceSettings(IConfigurationManager configurationManager)
        {
            this.configurationManager = configurationManager;
        }

        public string Endpoint()
        {
            return configurationManager.AppSettings["DesignerAddress"];
        }
    }
}