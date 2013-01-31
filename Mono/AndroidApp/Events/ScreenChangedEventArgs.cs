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
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Events
{
    public class ScreenChangedEventArgs : EventArgs
    {
        public ScreenChangedEventArgs(ItemPublicKey? screenId)
        {
            ScreenId = screenId;
        }
        public ItemPublicKey? ScreenId { get; private set; }
    }
}