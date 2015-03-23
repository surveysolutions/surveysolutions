using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private readonly IApplicationSettings applicationSettings;

        public AboutViewModel(ILogger logger, IPrincipal principal, IApplicationSettings applicationSettings)
            : base(logger, principal: principal)
        {
            this.applicationSettings = applicationSettings;
        }

        public string EngineVersion
        {
            get { return applicationSettings.EngineVersion; }
        }

        public string ApplicationVersion
        {
            get { return applicationSettings.ApplicationVersion; }
        }

        public string OSVersion
        {
            get { return applicationSettings.OSVersion; }
        }

        public string ApplicationName
        {
            get { return applicationSettings.ApplicationName; }
        }

        public override void GoBack()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}