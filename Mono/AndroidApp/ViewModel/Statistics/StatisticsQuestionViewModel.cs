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

namespace AndroidApp.ViewModel.Statistics
{
    public class StatisticsQuestionViewModel
    {
        public StatisticsQuestionViewModel(ItemPublicKey publicKey, ItemPublicKey parentPublicKey, string text, string answerString, string errorMessage)
        {
            PublicKey = publicKey;
            ParentKey = parentPublicKey;
            Text = text;
            AnswerString = answerString;
            ErrorMessage = errorMessage;
        }

        public ItemPublicKey PublicKey { get; private set; }
        public ItemPublicKey ParentKey { get; private set; }
        public string Text { get; private set; }
        public string AnswerString { get; protected set; }
        public string ErrorMessage { get; private set; }
    }
}