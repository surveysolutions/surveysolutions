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
    public class GroupViewModel :Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireItemViewModel
    {
        #region Implementation of IQuestionnaireItemView

        public ItemPublicKey PublicKey { get; private set; }
        public string Text { get; private set; }
        public bool Enabled { get; private set; }

        #endregion

        public GroupViewModel(ItemPublicKey publicKey, string text, bool enabled)
        {
            PublicKey = publicKey;
            Text = text;
            Enabled = enabled;
        }
    }
}