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
    public class AnswerViewModel
    {
        public AnswerViewModel(Guid publicKey, string title, bool selected)
        {
            PublicKey = publicKey;
            Title = title;
            Selected = selected;
        }

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }
        public bool Selected { get; private set; }
    }
}