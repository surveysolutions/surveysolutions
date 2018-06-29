using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface ISideBarSectionViewModelsFactory
    {
        ISideBarSectionItem BuildSectionItem(Identity sectionIdentity, NavigationState navigationState, string interviewId);
        ISideBarItem BuildCompleteItem(NavigationState navigationState, string interviewId);
        ISideBarItem BuildCoverItem(NavigationState navigationState);
        ISideBarItem BuildOverviewItem(NavigationState navigationState, string interviewId);
    }
}
