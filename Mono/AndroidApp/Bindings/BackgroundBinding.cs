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
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.Binding.Interfaces;

namespace AndroidApp.Bindings
{
    public class BackgroundBinding: MvxBaseAndroidTargetBinding
    {
        private readonly View _control;
        public BackgroundBinding(View control)
        {
            _control = control;
        }
        #region Overrides of MvxBaseTargetBinding

        public override void SetValue(object value)
        {
            var backGroundId = (int) value;
            if (backGroundId == -1)
            {
            //    var llWrapper=_control.FindViewById<LinearLayout>(Resource.Id.llWrapper);
             //   EnableDisableView(_control, false);
                return;

            }
            _control.SetBackgroundResource(backGroundId);
        }

        public override Type TargetType
        {
            get { return typeof(int); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        #endregion

       
    }
}