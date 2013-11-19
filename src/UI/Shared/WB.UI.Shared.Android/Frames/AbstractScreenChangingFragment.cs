using System;
using Android.Support.V4.App;
using WB.UI.Shared.Android.Controls;
using WB.UI.Shared.Android.Events;

namespace WB.UI.Shared.Android.Frames
{
    public class AbstractScreenChangingFragment : Fragment, IScreenChanging
    {
        public AbstractScreenChangingFragment()
        {
            
        }

        protected virtual void OnScreenChanged(ScreenChangedEventArgs evt)
        {
            var handler = this.ScreenChanged;
            if (handler != null)
                handler(this, evt);
        }

        public override void OnDetach()
        {
            this.ScreenChanged = null;
            base.OnDetach();
            this.Dispose();
        }

        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}