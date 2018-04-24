using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ImageButtonPlaybackToggleBinding : BaseBinding<ImageButton, bool>
    {
        public ImageButtonPlaybackToggleBinding(ImageButton androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueToView(ImageButton control, bool value)
        {
            control.SetImageResource(value
                ? Resource.Drawable.stop_icon
                : Resource.Drawable.play_icon);
        }
    }
}
