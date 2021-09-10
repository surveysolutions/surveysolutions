using System;
using System.Diagnostics.CodeAnalysis;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    [ExcludeFromCodeCoverage] // no reason to cover it yet. Doesn't have any logic 
    public class FlatRosterTitleViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IDisposable
    {
        public DynamicTextViewModel Title { get; }
        public EnablementViewModel Enablement { get; }

        public FlatRosterTitleViewModel(DynamicTextViewModel title, EnablementViewModel enablement)
        {
            this.Title = title;
            this.Enablement = enablement;
        }
        public Identity Identity { get; private set; }
        public virtual void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Identity = entityIdentity;
            Title.Init(interviewId, entityIdentity);
            Enablement.Init(interviewId, entityIdentity);
        }

        public void Dispose()
        {
            this.Title.Dispose();
            this.Enablement.Dispose();
        }
    }
}
