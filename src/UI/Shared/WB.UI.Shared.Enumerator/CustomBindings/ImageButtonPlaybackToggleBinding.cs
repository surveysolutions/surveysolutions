using Android.Graphics.Drawables;
using Android.Support.Graphics.Drawable;
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
            var controlDrawable = control.Drawable;

            switch (controlDrawable)
            {
                case AnimatedVectorDrawableCompat drawableCompat:
                    drawableCompat.RegisterAnimationCallback(new AnimationCallbackCompat(Target, value));
                    drawableCompat.Start();
                    break;
                case AnimatedVectorDrawable animatedDrawable:
                    animatedDrawable.RegisterAnimationCallback(new AnimationCallback(Target, value));
                    animatedDrawable.Start();
                    break;
            }
        }

        class AnimationCallback : Animatable2AnimationCallback
        {
            private readonly ImageButton btn;
            private readonly bool isPlaying;

            public AnimationCallback(ImageButton btn, bool isPlaying)
            {
                this.btn = btn;
                this.isPlaying = isPlaying;
            }

            public override void OnAnimationEnd(Drawable drawable)
            {
                if (isPlaying)
                {
                    btn.SetImageDrawable(btn.Context.GetDrawable(Resource.Drawable.stop_to_play_animation));
                }
                else
                {
                    btn.SetImageDrawable(btn.Context.GetDrawable(Resource.Drawable.play_to_stop_animation));
                }
            }
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
                if (isPlaying)
                {
                    btn.SetImageDrawable(btn.Context.GetDrawable(Resource.Drawable.stop_to_play_animation));
                }
                else
                {
                    btn.SetImageDrawable(btn.Context.GetDrawable(Resource.Drawable.play_to_stop_animation));
                }
            }
        }
    }
}
