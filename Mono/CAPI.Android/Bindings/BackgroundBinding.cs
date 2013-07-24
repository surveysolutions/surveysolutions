using System;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace CAPI.Android.Bindings
{
    public class BackgroundBinding : MvxAndroidTargetBinding
    {
        private readonly View control;

        public BackgroundBinding(View control)
            : base(control)
        {
            this.control = control;
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

            control.SetBackgroundResource(bgId);

            var llWrapper = control.FindViewById<LinearLayout>(Resource.Id.llWrapper);

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