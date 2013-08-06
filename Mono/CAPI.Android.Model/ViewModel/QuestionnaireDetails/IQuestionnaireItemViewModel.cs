using System;
using System.ComponentModel;
using Cirrious.MvvmCross.ViewModels;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{

    public interface IQuestionnaireItemViewModel : IMvxViewModel, INotifyPropertyChanged
    {
        ItemPublicKey PublicKey { get; }
        string Text { get; }
        IQuestionnaireItemViewModel Clone(Guid propagationKey);
        //   bool Enabled { get; }
    }
}