using System.Web.Configuration;
using Ninject.Modules;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Shared.Web.Modules
{
    public class WebConfigurationModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IConfigurationManager>()
                .ToConstant(new ConfigurationManager(appSettings: WebConfigurationManager.AppSettings));
        }
    }
}