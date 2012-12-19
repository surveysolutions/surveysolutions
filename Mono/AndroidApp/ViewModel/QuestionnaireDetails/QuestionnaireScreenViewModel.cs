using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireScreenViewModel : IQuestionnaireViewModel
    {
        public QuestionnaireScreenViewModel(Guid questionnaireId,string screenName, 
            Guid screenId,
            Guid? propagationKey, IEnumerable<IQuestionnaireItemViewModel> items,
            IList<QuestionnaireNavigationPanelItem> siblings,
            IEnumerable<QuestionnaireNavigationPanelItem> breadcrumbs, IEnumerable<QuestionnaireNavigationPanelItem> chapters)
        {

            QuestionnaireId = questionnaireId;
            Items = items;
            ScreenId = screenId;
            PropagationKey = propagationKey;
            Siblings = siblings;
            Breadcrumbs = breadcrumbs;
            Chapters = chapters;
        }

        public Guid QuestionnaireId { get; private set; }
        public string ScreenName { get; private set; }
        public Guid ScreenId { get; private set; }
        public Guid? PropagationKey { get; private set; }
        public IList<QuestionnaireNavigationPanelItem> Siblings { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Breadcrumbs { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Chapters { get; private set; }

        public IEnumerable<IQuestionnaireItemViewModel> Items { get; private set; }
    }
}