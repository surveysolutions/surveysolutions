using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface ISideBarSectionViewModelsFactory
    {
        SideBarSectionViewModel BuildSectionItem(SideBarSectionsViewModel root, SideBarSectionViewModel sectionToAddTo, Identity enabledSubgroupIdentity, NavigationState navigationState, string interviewId);
    }
}