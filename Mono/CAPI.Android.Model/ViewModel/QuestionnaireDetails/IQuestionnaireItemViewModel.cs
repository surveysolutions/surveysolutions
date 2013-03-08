using System;
using System.ComponentModel;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{

    public interface IQuestionnaireItemViewModel : Cirrious.MvvmCross.Interfaces.ViewModels.IMvxViewModel, INotifyPropertyChanged
    {
        ItemPublicKey PublicKey { get; }
        string Text { get; }
        IQuestionnaireItemViewModel Clone(Guid propagationKey);
        //   bool Enabled { get; }
    }
}