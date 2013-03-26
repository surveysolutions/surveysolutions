using System;
using CAPI.Android.Events;
using Fragment = Android.Support.V4.App.Fragment;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class AbstractScreenChangingFragment : Fragment, IScreenChanging
    {
     
        public AbstractScreenChangingFragment()
        {
            
        }

        protected virtual void OnScreenChanged(ScreenChangedEventArgs evt)
        {
            var handler = ScreenChanged;
            if (handler != null)
                handler(this, evt);
        }
        public override void OnDetach()
        {
            ScreenChanged = null;
            base.OnDetach();
        }
        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}