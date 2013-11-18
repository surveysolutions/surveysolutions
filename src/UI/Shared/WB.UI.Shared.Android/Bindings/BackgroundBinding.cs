using System;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.Shared.Android.Bindings
{
    public class BackgroundBinding : MvvmBindingWrapper<View>
    {
        public BackgroundBinding(View control)
            : base(control)
        {
        }

        #region Overrides of MvxBaseTargetBinding


        protected override void SetValueToView(View view, object value)
        {
            var status = (QuestionStatus) value;

            int bgId = Resource.Drawable.questionShape;
            var enabled = status.HasFlag(QuestionStatus.Enabled) && status.HasFlag(QuestionStatus.ParentEnabled);
            if (!enabled)
                bgId = Resource.Drawable.questionDisabledShape;
            else if (!status.HasFlag(QuestionStatus.Valid))
                bgId = Resource.Drawable.questionInvalidShape;
            else if (status.HasFlag(QuestionStatus.Answered))
                bgId = Resource.Drawable.questionAnsweredShape;

            view.SetBackgroundResource(bgId);

            var llWrapper = view.FindViewById<LinearLayout>(Resource.Id.llWrapper);

            if (llWrapper != null)
            {
                llWrapper.EnableDisableView(enabled);
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