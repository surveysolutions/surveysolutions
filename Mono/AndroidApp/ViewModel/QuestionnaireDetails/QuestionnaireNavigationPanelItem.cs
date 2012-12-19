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
    public class QuestionnaireNavigationPanelItem
    {
        public QuestionnaireNavigationPanelItem(ItemPublicKey screenPublicKey, string title, int total, int answered)
        {
            ScreenPublicKey = screenPublicKey;
            Title = title;
            Total = total;
            Answered = answered;
        }

        public ItemPublicKey ScreenPublicKey { get; private set; }
        public string Title { get; private set; }
        public int Total { get; private set; }
        public int Answered { get; private set; }
    }
}