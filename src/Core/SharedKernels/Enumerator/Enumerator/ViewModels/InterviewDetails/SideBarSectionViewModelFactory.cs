using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    internal class SideBarSectionViewModelFactory : ISideBarSectionViewModelsFactory
    {
        private readonly IServiceLocator serviceLocator;

        public SideBarSectionViewModelFactory(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public ISideBarSectionItem BuildSectionItem(Identity sectionIdentity, NavigationState navigationState, string interviewId)
        {
            var sideBarItem = this.serviceLocator.GetInstance<SideBarSectionViewModel>();
            var groupStateViewModel = this.serviceLocator.GetInstance<GroupStateViewModel>();
            sideBarItem.Init(interviewId, sectionIdentity, groupStateViewModel, navigationState);
            return sideBarItem;
        }

        public ISideBarItem BuildCompleteItem(NavigationState navigationState, string interviewId)
        {
            var sideBarItem = this.serviceLocator.GetInstance<SideBarCompleteSectionViewModel>();
            sideBarItem.Init(navigationState, interviewId);
            return sideBarItem;
        }

        public ISideBarItem BuildCoverItem(NavigationState navigationState)
        {
            var sideBarItem = this.serviceLocator.GetInstance<SideBarCoverSectionViewModel>();
            sideBarItem.Init(navigationState);
            return sideBarItem;
        }
    }

 
}