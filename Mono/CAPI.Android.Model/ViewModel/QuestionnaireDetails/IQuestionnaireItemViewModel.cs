using System;
using System.ComponentModel;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{

    public interface IQuestionnaireItemViewModel : IMvxViewModel, INotifyPropertyChanged
    {
        InterviewItemId PublicKey { get; }
        string Text { get; }
        IQuestionnaireItemViewModel Clone(int[] propagationVector);
    }
}