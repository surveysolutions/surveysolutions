using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AndroidApp.Core.Model.ViewModel.QuestionnaireDetails
{
    public interface IQuestionnaireViewModel : Cirrious.MvvmCross.Interfaces.ViewModels.IMvxViewModel, INotifyPropertyChanged
    {
        Guid QuestionnaireId { get; }
        string Title { get; }
        string ScreenName { get; }
        ItemPublicKey ScreenId { get; }
        int Total { get; }
        int Answered { get; }
        IEnumerable<ItemPublicKey> Siblings { get; }
        IEnumerable<IQuestionnaireViewModel> Breadcrumbs { get; }
        IEnumerable<QuestionnaireScreenViewModel> Chapters { get; }
        bool Enabled { get; }
        void SetEnabled(bool enabled);
    }
}