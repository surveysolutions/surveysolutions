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
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class RosterRow
    {
        public RosterRow(ItemPublicKey id, string screenName, IList<IQuestionnaireItemViewModel> items, bool enabled)
        {
            Id = id;
            ScreenName = screenName;
            Items = items;
            Enabled = enabled;
        }

        public ItemPublicKey Id { get; private set; }
        public string ScreenName { get; private set; }
        public IList<IQuestionnaireItemViewModel> Items { get; private set; }
        public bool Enabled { get; private set; }
    }
}