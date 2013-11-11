using System;
using System.Collections.Generic;
using System.ComponentModel;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
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