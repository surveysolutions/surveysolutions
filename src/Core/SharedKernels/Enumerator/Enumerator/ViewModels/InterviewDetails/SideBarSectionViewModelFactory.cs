using Microsoft.Practices.ServiceLocation;
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

        public SideBarSectionViewModel BuildSectionItem(SideBarSectionsViewModel root, 
            SideBarSectionViewModel sectionToAddTo,
            NavigationIdentity enabledSubgroupIdentity, 
            NavigationState navigationState, 
            string interviewId)
        {
            var sideBarItem = this.serviceLocator.GetInstance<SideBarSectionViewModel>();
            var groupStateViewModel = this.serviceLocator.GetInstance<GroupStateViewModel>();
            sideBarItem.Init(interviewId, enabledSubgroupIdentity, root, sectionToAddTo, groupStateViewModel, navigationState);
            return sideBarItem;
        }

        public SideBarSectionViewModel BuildCompleteScreenSectionItem(NavigationState navigationState, string interviewId)
        {
            var sideBarItem = this.serviceLocator.GetInstance<SideBarSectionViewModel>();
            var interviewStateViewModel = this.serviceLocator.GetInstance<InterviewStateViewModel>();
            sideBarItem.InitCompleteScreenItem(interviewId, interviewStateViewModel, navigationState);
            return sideBarItem;
        }

        public SideBarSectionViewModel BuildCoverScreenSectionItem(NavigationState navigationState, string interviewId)
        {
            var sideBarItem = this.serviceLocator.GetInstance<SideBarSectionViewModel>();
            var interviewStateViewModel = this.serviceLocator.GetInstance<CoverStateViewModel>();
            sideBarItem.InitCoverScreenItem(interviewId, interviewStateViewModel, navigationState);
            return sideBarItem;
        }
    }

 
}