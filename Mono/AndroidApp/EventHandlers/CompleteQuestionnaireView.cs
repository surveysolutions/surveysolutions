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
using AndroidApp.ViewModel.QuestionnaireDetails;

namespace AndroidApp.EventHandlers
{
    public class CompleteQuestionnaireView
    {
        public CompleteQuestionnaireView(IDictionary<ItemPublicKey, IQuestionnaireViewModel> screens)
        {
            Screens = screens;
        }

        public IDictionary<ItemPublicKey, IQuestionnaireViewModel> Screens { get; private set; }
    }
}