using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public class SupervisorSideBarSectionViewModelFactory : SideBarSectionViewModelFactory
    {
        public SupervisorSideBarSectionViewModelFactory(IServiceLocator serviceLocator) : base(serviceLocator)
        {
        }

        public override ISideBarItem BuildCompleteItem(NavigationState navigationState, string interviewId)
        {
            var sideBarItem = this.serviceLocator.GetInstance<SideBarCompleteSectionViewModel>();
            sideBarItem.Init(navigationState, interviewId, InterviewDetails.Resolve);
            return sideBarItem;
        }
    }
}
