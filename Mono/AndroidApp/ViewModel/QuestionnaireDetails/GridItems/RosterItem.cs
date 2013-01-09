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
        public RosterItem(ItemPublicKey publicKey, string title, IList<AbstractRowItem> rowItems)
        {
            PublicKey = publicKey;
            Title = title;
            RowItems = rowItems;
        }

        public ItemPublicKey PublicKey { get; private set; }
        public string Title { get; private set; }
        public IList<AbstractRowItem> RowItems { get; private set; }
    }
}