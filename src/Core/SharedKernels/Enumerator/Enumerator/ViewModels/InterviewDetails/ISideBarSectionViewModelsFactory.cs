using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public interface ISideBarSectionViewModelsFactory
    {
        SideBarSectionViewModel BuildSectionItem(SideBarSectionsViewModel root, SideBarSectionViewModel sectionToAddTo, Identity enabledSubgroupIdentity, NavigationState navigationState, string interviewId);
    }
}