using System;
using System.Collections.Generic;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public interface IQuestionnaireViewModel
    {
        Guid QuestionnaireId { get; }
        string ScreenName { get; }
        ItemPublicKey ScreenId { get; }
        IList<QuestionnaireNavigationPanelItem> Siblings { get; }
        IEnumerable<QuestionnaireNavigationPanelItem> Breadcrumbs { get; }
        IEnumerable<QuestionnaireNavigationPanelItem> Chapters { get; }
    }
}