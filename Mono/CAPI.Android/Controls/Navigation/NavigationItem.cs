using System;

namespace CAPI.Android.Controls.Navigation
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