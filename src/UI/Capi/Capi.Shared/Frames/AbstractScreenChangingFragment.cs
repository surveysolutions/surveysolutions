using System;
using Android.Support.V4.App;
using WB.UI.Capi.Shared.Controls;
using WB.UI.Capi.Shared.Events;

namespace WB.UI.Capi.Shared.Frames
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