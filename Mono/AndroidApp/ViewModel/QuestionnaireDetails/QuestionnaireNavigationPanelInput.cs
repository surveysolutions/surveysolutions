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
    public class QuestionnaireNavigationPanelInput
    {
        public Guid QuestionnairePublicKey { get; private set; }

        public QuestionnaireNavigationPanelInput(Guid questionnairePublicKey)
        {
            QuestionnairePublicKey = questionnairePublicKey;
        }
    }
}