using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface ISideBarSectionItem : ISideBarItem
    {
        Identity SectionIdentity { get; }
        Identity[] ParentsIdentities { get; }
        Identity ParentIdentity { get; }
        
        event SideBarSectionUpdated OnSectionUpdated;
    }
}