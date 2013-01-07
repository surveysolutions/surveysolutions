using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public interface IQuestionnaireViewModel : INotifyPropertyChanged
    {
        Guid QuestionnaireId { get; }
        string Title { get; }
        string ScreenName { get; }
        ItemPublicKey ScreenId { get; }
        IList<QuestionnaireNavigationPanelItem> Siblings { get; }
        IEnumerable<QuestionnaireNavigationPanelItem> Breadcrumbs { get; }
        IEnumerable<QuestionnaireNavigationPanelItem> Chapters { get; }
    }
}