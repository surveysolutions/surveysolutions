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
using AndroidApp.Events;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class AbstractScreenChangingFragment : Android.Support.V4.App.Fragment, IScreenChanging
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
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}