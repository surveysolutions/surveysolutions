using Android.Graphics.Drawables;
using Android.Support.Graphics.Drawable;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ImageViewPlaybackIndicatorBinding :  BaseBinding<ImageView, bool>
    {
        public ImageViewPlaybackIndicatorBinding(ImageView androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueToView(ImageView control, bool isPlaying)
        {
            Target.Visibility = isPlaying ? ViewStates.Visible : ViewStates.Gone;

            if (isPlaying)
            {
                var controlDrawable = control.Drawable;
                switch (controlDrawable)
                {
                    case AnimatedVectorDrawableCompat drawableCompat:
                        drawableCompat.RegisterAnimationCallback(new AnimationCallbackCompat(this));
                        drawableCompat.Start();
                        break;
                    case AnimatedVectorDrawable animatedDrawable:
                        animatedDrawable.RegisterAnimationCallback(new AnimationCallback(this));
                        animatedDrawable.Start();
                        break;
                }
            }
        }

        class AnimationCallback : Animatable2AnimationCallback
        {
            private readonly ImageViewPlaybackIndicatorBinding binding;

            public AnimationCallback(ImageViewPlaybackIndicatorBinding binding)
            {
                this.binding = binding;
            }

            public override void OnAnimationEnd(Drawable drawable)
            {
                if (this.binding.Target.Visibility == ViewStates.Visible)
                {
                    var animatedVectorDrawable = drawable as AnimatedVectorDrawable;
                    animatedVectorDrawable.RegisterAnimationCallback(this);
                    animatedVectorDrawable.Start();
                }
            }
        }

        class AnimationCallbackCompat : Animatable2CompatAnimationCallback
        {
            private readonly ImageViewPlaybackIndicatorBinding inding;

            public AnimationCallbackCompat(ImageViewPlaybackIndicatorBinding inding)
            {
                this.inding = inding;
            }

            public override void OnAnimationEnd(Drawable drawable)
            {
                if (this.inding.Target.Visibility == ViewStates.Visible)
                {
                    var animatedVectorDrawable = drawable as AnimatedVectorDrawableCompat;
                    animatedVectorDrawable.RegisterAnimationCallback(this);
                    animatedVectorDrawable.Start();
                }
            }
        }
    }
}

