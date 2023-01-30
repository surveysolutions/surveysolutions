using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Content.Resources;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ProgressBarCompleteBinding : BaseBinding<ProgressBar, int>
    {
        public ProgressBarCompleteBinding(ProgressBar target)
            : base(target)
        {
        }

        protected override void SetValueToView(ProgressBar view, int value)
        {
            if (view.Progress != view.Max && view.Max != value)
                return;
            if (view.Progress == view.Max && view.Max == value)
                return;
            
            if (value == view.Max)
                SetProgressBarColor(view, Resource.Color.loading_progress_color_complete);
            else
                SetProgressBarColor(view, Resource.Color.loading_progress_color);
        }

        private void SetProgressBarColor(ProgressBar view, int colorId)
        {
#pragma warning disable CA1416
            var color = ContextCompat.GetColor(view.Context, colorId);
            var currentDrawable = view.CurrentDrawable;
            currentDrawable.SetColorFilter(new Color(color), PorterDuff.Mode.SrcIn);
#pragma warning restore CA1416
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;
    }
}
