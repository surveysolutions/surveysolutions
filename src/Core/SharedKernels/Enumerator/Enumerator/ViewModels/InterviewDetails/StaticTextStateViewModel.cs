using System;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class StaticTextStateViewModel : MvxNotifyPropertyChanged, IDisposable
    {
        public StaticTextStateViewModel(EnablementViewModel enablement,
            ValidityViewModel validityViewModel, WarningsViewModel warningsViewModel)
        {
            this.Enablement = enablement;
            this.Validity = validityViewModel;
            this.Warnings = warningsViewModel;
        }

        public EnablementViewModel Enablement { get; }

        public ValidityViewModel Validity { get; }
        public WarningsViewModel Warnings { get; }

        public void Init(string interviewId, Identity entityIdentity)
        {
            this.Enablement.Init(interviewId, entityIdentity);
            this.Validity.Init(interviewId, entityIdentity);
            this.Warnings.Init(interviewId, entityIdentity);
        }

        public void Dispose()
        {
            this.Enablement.Dispose();
            this.Validity.Dispose();
            this.Warnings.Dispose();
        }
    }
}