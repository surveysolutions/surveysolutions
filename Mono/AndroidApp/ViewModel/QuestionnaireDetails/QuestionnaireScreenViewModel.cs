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
    public class QuestionnaireScreenViewModel
    {
        public QuestionnaireScreenViewModel(Guid questionnaireId, Guid screenId, Guid? propagationKey, QuestionnaireNavigationPanelItem next, QuestionnaireNavigationPanelItem previous, IEnumerable<QuestionnaireNavigationPanelItem> breadcrumbs)
        {
            QuestionnaireId = questionnaireId;
            ScreenId = screenId;
            PropagationKey = propagationKey;
            Next = next;
            Previous = previous;
            Breadcrumbs = breadcrumbs;
        }

        public Guid QuestionnaireId { get; private set; }
        public Guid ScreenId { get; private set; }
        public Guid? PropagationKey { get; private set; }
        public QuestionnaireNavigationPanelItem Next { get; private set; }
        public QuestionnaireNavigationPanelItem Previous { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Breadcrumbs { get; private set; }
    }
}