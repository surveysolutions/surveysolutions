using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Graphics.Drawable;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Platform;

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
            AnimatedVectorDrawableCompat.RegisterAnimationCallback(control.Drawable, new AnimationCallbackCompat(Target, value));
            var animatable = control.Drawable as IAnimatable;
            animatable.Start();
        }

        class AnimationCallbackCompat : Animatable2CompatAnimationCallback
        {
            private readonly ImageButton btn;
            private readonly bool isPlaying;

            public AnimationCallbackCompat(ImageButton btn, bool isPlaying)
            {
                this.btn = btn;
                this.isPlaying = isPlaying;
            }

            public override void OnAnimationEnd(Drawable drawable)
            {
                btn.SetImageResource(isPlaying
                    ? Resource.Drawable.stop_to_play_animation
                    : Resource.Drawable.play_to_stop_animation);
            }
        }
    }
}
