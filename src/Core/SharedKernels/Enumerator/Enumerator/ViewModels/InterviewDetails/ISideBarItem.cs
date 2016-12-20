using System;
using System.Windows.Input;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface ISideBarItem : IDisposable
    {
        bool IsSelected { get; }
        bool IsCurrent { get; }
        bool Expanded { get; }
        int NodeDepth { get; }
        bool HasChildren { get; }
        GroupStateViewModel SideBarGroupState { get; }
        DynamicTextViewModel Title { get; }
        ICommand NavigateToSectionCommand { get; }
        ICommand ToggleCommand { get; }
    }
}