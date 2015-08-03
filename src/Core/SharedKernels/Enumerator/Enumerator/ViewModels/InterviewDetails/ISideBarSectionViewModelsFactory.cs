using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public interface ISideBarSectionViewModelsFactory
    {
        SideBarSectionViewModel BuildSectionItem(SideBarSectionViewModel sectionToAddTo, Identity groupIdentity, NavigationState navigationState, string interviewId);
    }
}