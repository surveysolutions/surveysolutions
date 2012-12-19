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
    public class RosterItem
    {
        public RosterItem(ItemPublicKey publicKey, string title, IEnumerable<RowItem> rowItems)
        {
            PublicKey = publicKey;
            Title = title;
            RowItems = rowItems;
        }

        public ItemPublicKey PublicKey { get; private set; }
        public string Title { get; private set; }
        public IEnumerable<RowItem> RowItems { get; private set; }
    }
}