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
    public class HeaderItem
    {
        public HeaderItem(Guid publicKey, string title, string instructions)
        {
            PublicKey = publicKey;
            Title = title;
            Instructions = instructions;
        }

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }
        public string Instructions { get; private set; }
    }
}