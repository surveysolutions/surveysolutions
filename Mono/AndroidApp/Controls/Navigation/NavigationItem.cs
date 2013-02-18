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

namespace AndroidApp.Controls.Navigation
{
    public class NavigationItem
    {
        public string Title { get; private set; }
        public bool Handle(object sender)
        {
            var tmpHdl = handler;
            if (tmpHdl != null)
              return  handler(sender, EventArgs.Empty);
            return false;
        }

        private readonly Func<object,EventArgs,bool> handler;

        public NavigationItem(Func<object, EventArgs, bool> handler, string title)
        {
            this.handler = handler;
            Title = title;
        }
    }
}