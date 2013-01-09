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

namespace AndroidApp.ViewModel.QuestionnaireDetails.GridItems
{
    public class GroupRowItem:AbstractRowItem
    {
        public GroupRowItem(Guid publicKey, Guid propagationKey, string text, bool enabled, string comments) : base(publicKey, propagationKey, text, enabled, comments)
        {
        }
    }
}