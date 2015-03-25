using Chance.MvvmCross.Plugins.UserInteraction;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class InterviewViewModel : BaseViewModel
    {
        public InterviewViewModel(ILogger logger, IPrincipal principal, IUserInteraction uiDialogs = null) : base(logger, principal, uiDialogs)
        {
        }
    }
}
