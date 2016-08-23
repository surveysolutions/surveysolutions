namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface ISideBarSectionViewModelsFactory
    {
        SideBarSectionViewModel BuildSectionItem(SideBarSectionsViewModel root, SideBarSectionViewModel sectionToAddTo, NavigationIdentity enabledSubgroupIdentity, NavigationState navigationState, string interviewId);
        SideBarSectionViewModel BuildCompleteScreenSectionItem(NavigationState navigationState,  string interviewId);
        SideBarSectionViewModel BuildCoverScreenSectionItem(NavigationState navigationState,  string interviewId);
    }
}