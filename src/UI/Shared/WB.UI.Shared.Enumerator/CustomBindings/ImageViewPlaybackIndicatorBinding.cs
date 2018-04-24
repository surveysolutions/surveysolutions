using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Support.Graphics.Drawable;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;

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
                AnimatedVectorDrawableCompat.RegisterAnimationCallback(Target.Drawable, new AnimationCallbackCompat(this));
                var animatable = Target.Drawable as IAnimatable;
                animatable.Start();
            }
            else
            {
                AnimatedVectorDrawableCompat.ClearAnimationCallbacks(Target.Drawable);
            }
        }

        class AnimationCallbackCompat : Animatable2CompatAnimationCallback
        {
            private readonly ImageViewPlaybackIndicatorBinding binding;

            public AnimationCallbackCompat(ImageViewPlaybackIndicatorBinding binding)
            {
                this.binding = binding;
            }

            public override void OnAnimationEnd(Drawable drawable)
            {
                if (this.binding.Target.Visibility == ViewStates.Visible)
                {
                    var animatedVectorDrawable = drawable as IAnimatable;
                    animatedVectorDrawable.Start();
                }
            }
        }
    }
}

