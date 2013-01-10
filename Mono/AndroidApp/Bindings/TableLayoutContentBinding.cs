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
using AndroidApp.ViewModel.Model;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.Binding.Interfaces;

namespace AndroidApp.Bindings
{
    public class TableLayoutContentBinding: MvxBaseAndroidTargetBinding
    {
        private readonly DashboardSurveyItems _control;
        public TableLayoutContentBinding(DashboardSurveyItems control)
        {
            _control = control;
        }
        #region Overrides of MvxBaseTargetBinding

        public override void SetValue(object value)
        {
            var content = value as IList<DashboardQuestionnaireItem>;
            if (content == null)
            {
                _control.Items =new List<DashboardQuestionnaireItem>(0);
                return;
            }
            _control.Items = content;
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