using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    internal class SideBarSectionViewModelFactory : ISideBarSectionViewModelsFactory
    {
        readonly IServiceLocator serviceLocator;

        public SideBarSectionViewModelFactory(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public SideBarSectionViewModel BuildSectionItem(SideBarSectionViewModel sectionToAddTo, 
            Identity groupIdentity, 
            NavigationState navigationState, 
            string interviewId)
        {
            var sideBarItem = serviceLocator.GetInstance<SideBarSectionViewModel>();
            var groupStateViewModel = serviceLocator.GetInstance<GroupStateViewModel>();
            sideBarItem.Init(interviewId, groupIdentity, sectionToAddTo, groupStateViewModel, navigationState);
            return sideBarItem;
        }
    }
}