using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel(ILogger logger, IPrincipal principal) : base(logger, principal: principal) { }
    }
}