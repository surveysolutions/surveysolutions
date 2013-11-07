using System.ComponentModel;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.CAPI.Views.InterviewDetails
{

    public interface IQuestionnaireItemViewModel : IMvxViewModel, INotifyPropertyChanged
    {
        InterviewItemId PublicKey { get; }
        string Text { get; }
        IQuestionnaireItemViewModel Clone(int[] propagationVector);
    }
}