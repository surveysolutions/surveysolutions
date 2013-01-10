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
using AndroidApp.Controls.Dashboard;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.Binding.Interfaces;

namespace AndroidApp.Bindings
{
    public class TableLayoutHeaderBinding : MvxBaseAndroidTargetBinding
    {
        private readonly DashboardSurveyItems _control;
          public TableLayoutHeaderBinding(DashboardSurveyItems control)
        {
            _control = control;
        }
        #region Overrides of MvxBaseTargetBinding

        public override void SetValue(object value)
        {
            var header = value as IList<string>;
            if (header == null)
            {
                _control.Header=new List<string>(0);
                return;
            }
            _control.Header = header;
        }

        public override Type TargetType
        {
            get { return typeof(object); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneTime; }
        }

        #endregion
    }
}