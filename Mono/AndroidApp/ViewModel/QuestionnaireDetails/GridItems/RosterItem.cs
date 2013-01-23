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
        private readonly Func<string> titleUpdater;
        public RosterItem(ItemPublicKey publicKey, IList<IQuestionnaireItemViewModel> rowItems,Func<string> titleUpdater)
        {
            PublicKey = publicKey;
            RowItems = rowItems;
            this.titleUpdater = titleUpdater;
        }

        public ItemPublicKey PublicKey { get; private set; }

        public string Title
        {
            get { return titleUpdater(); }
        }

        public IList<IQuestionnaireItemViewModel> RowItems { get; private set; }
    }
}