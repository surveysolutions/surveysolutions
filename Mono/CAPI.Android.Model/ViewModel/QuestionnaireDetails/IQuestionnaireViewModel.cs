using System;
using System.Collections.Generic;
using System.ComponentModel;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public interface IQuestionnaireViewModel : IMvxViewModel,
                                               INotifyPropertyChanged
    {
        Guid QuestionnaireId { get; }
        string Title { get; }
        string ScreenName { get; }
        InterviewItemId ScreenId { get; }
        int Total { get; }
        int Answered { get; }
        IEnumerable<InterviewItemId> Siblings { get; }
        IEnumerable<InterviewItemId> Breadcrumbs { get; }

        bool Enabled { get; }
        void SetEnabled(bool enabled);

    }
}