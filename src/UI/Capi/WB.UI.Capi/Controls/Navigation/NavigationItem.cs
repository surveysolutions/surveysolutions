using System;

namespace WB.UI.Capi.Controls.Navigation
{
    public class NavigationItem
    {
        public string Title { get; private set; }
        public bool Handle(object sender)
        {
            var tmpHdl = this.handler;
            if (tmpHdl != null)
              return  this.handler(sender, EventArgs.Empty);
            return false;
        }

        private readonly Func<object,EventArgs,bool> handler;

        public NavigationItem(Func<object, EventArgs, bool> handler, string title)
        {
            this.handler = handler;
            this.Title = title;
        }
    }
}