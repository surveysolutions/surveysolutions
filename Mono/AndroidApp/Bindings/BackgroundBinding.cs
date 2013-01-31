using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using AndroidApp.Extensions;
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
            var status = (QuestionStatus) value;


            int bgId = Resource.Drawable.questionShape;
            if (!status.HasFlag(QuestionStatus.Enabled))
                bgId = Resource.Drawable.questionDisabledShape;
            else if (!status.HasFlag(QuestionStatus.Valid))
                bgId = Resource.Drawable.questionInvalidShape;
            else if (status.HasFlag(QuestionStatus.Answered))
                bgId = Resource.Drawable.questionAnsweredShape;

            _control.SetBackgroundResource(bgId);

            var llWrapper = _control.FindViewById<LinearLayout>(Resource.Id.llWrapper);

            if (llWrapper != null)
            {
                llWrapper.EnableDisableView(status.HasFlag(QuestionStatus.Enabled));
            }
        }

        public override Type TargetType
        {
            get { return typeof(QuestionStatus); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        #endregion
       
    }
}