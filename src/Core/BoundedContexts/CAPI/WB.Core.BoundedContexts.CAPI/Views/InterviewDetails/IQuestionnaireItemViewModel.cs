using System.ComponentModel;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{

    public interface IQuestionnaireItemViewModel : IMvxViewModel, INotifyPropertyChanged
    {
        InterviewItemId PublicKey { get; }
        string Text { get; }
        IQuestionnaireItemViewModel Clone(decimal[] propagationVector);
    }
}