using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    internal class SideBarSectionViewModelFactory : ISideBarSectionViewModelsFactory
    {
        readonly IServiceLocator serviceLocator;

        public SideBarSectionViewModelFactory(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public SideBarSectionViewModel BuildSectionItem(SideBarSectionsViewModel root, 
            SideBarSectionViewModel sectionToAddTo,
            Identity enabledSubgroupIdentity, 
            NavigationState navigationState, 
            string interviewId)
        {
            var sideBarItem = this.serviceLocator.GetInstance<SideBarSectionViewModel>();
            var groupStateViewModel = this.serviceLocator.GetInstance<GroupStateViewModel>();
            sideBarItem.Init(interviewId, enabledSubgroupIdentity, root, sectionToAddTo, groupStateViewModel, navigationState);
            return sideBarItem;
        }
    }

 
}