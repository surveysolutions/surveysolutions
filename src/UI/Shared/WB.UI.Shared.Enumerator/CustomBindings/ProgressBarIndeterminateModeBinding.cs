using System;
using System.Collections.Generic;
using Android.Animation;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android;
using ArgbEvaluator = Android.Support.Graphics.Drawable.ArgbEvaluator;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ProgressBarIndeterminateModeBinding : BaseBinding<ProgressBar, bool>
    {
        ValueAnimator animator;
        readonly ArgbEvaluator evaluator = new ArgbEvaluator();

        public ProgressBarIndeterminateModeBinding(ProgressBar androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(ProgressBar control, bool value)
        {
            control.Indeterminate = value;

            if (value)
            {
                //StartAnimation(control);
            }
        }

        private void StartAnimation(ProgressBar control)
        {
//            control.Visibility = ViewStates.Visible;
//
//            animator = TimeAnimator.OfFloat(0.0f, 1.0f);
//            animator.SetDuration(500);
//            animator.RepeatCount = ValueAnimator.Infinite;
//            animator.RepeatMode = ValueAnimatorRepeatMode.Reverse;
//            animator.Update += AnimatorOnUpdate;
//
//            animator.Start();

            Bitmap b = DrawableToBitmap(control.IndeterminateDrawable);
            AnimationDrawable shiftedAnimation = GetAnimation(b);

            //      R.id.img_3 is ImageView in my application
            control.IndeterminateDrawable = shiftedAnimation;
            shiftedAnimation.Start();
        }

        private AnimationDrawable GetAnimation(Bitmap bitmap)
        {
            AnimationDrawable animation = new AnimationDrawable();
            animation.OneShot = false;

            List<Bitmap> shiftedBitmaps = GetShiftedBitmaps(bitmap);
            int duration = 50;

            foreach (Bitmap image in shiftedBitmaps)
            {
                BitmapDrawable navigationBackground = new BitmapDrawable(Resources.System, image);
                navigationBackground.TileModeX = Shader.TileMode.Repeat;

                animation.AddFrame(navigationBackground, duration);
            }
            return animation;
        }

        private List<Bitmap> GetShiftedBitmaps(Bitmap bitmap)
        {
            List<Bitmap> shiftedBitmaps = new List<Bitmap>();
            int fragments = 10;
            int shiftLength = bitmap.Width / fragments;

            for (int i = 0; i < fragments; ++i)
            {
                shiftedBitmaps.Add(GetShiftedBitmap(bitmap, shiftLength * i));
            }

            return shiftedBitmaps;
        }

        private Bitmap GetShiftedBitmap(Bitmap bitmap, int shiftX)
        {
            Bitmap newBitmap = Bitmap.CreateBitmap(bitmap.Width, bitmap.Height, bitmap.GetConfig());
            Canvas newBitmapCanvas = new Canvas(newBitmap);

            Rect srcRect1 = new Rect(shiftX, 0, bitmap.Width, bitmap.Height);
            Rect destRect1 = new Rect(srcRect1);
            destRect1.Offset(-shiftX, 0);
            newBitmapCanvas.DrawBitmap(bitmap, srcRect1, destRect1, null);

            Rect srcRect2 = new Rect(0, 0, shiftX, bitmap.Height);
            Rect destRect2 = new Rect(srcRect2);
            destRect2.Offset(bitmap.Width - shiftX, 0);
            newBitmapCanvas.DrawBitmap(bitmap, srcRect2, destRect2, null);

            return newBitmap;
        }



        public static Bitmap DrawableToBitmap(Drawable drawable)
        {
            Bitmap bitmap = null;

            if (drawable is BitmapDrawable) {
                BitmapDrawable bitmapDrawable = (BitmapDrawable)drawable;
                if (bitmapDrawable.Bitmap != null)
                {
                    return bitmapDrawable.Bitmap;
                }
            }

            if (drawable.IntrinsicWidth <= 0 || drawable.IntrinsicHeight <= 0)
            {
                bitmap = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888); // Single color bitmap will be created of 1x1 pixel
            }
            else
            {
                bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
            }

            Canvas canvas = new Canvas(bitmap);
            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);
            return bitmap;
        }



        private void AnimatorOnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs animatorUpdateEventArgs)
        {
            int start = Color.ParseColor("#FDB72B");
            int mid = Color.ParseColor("#88FDB72B");
            int end = Color.Transparent;

            var fraction = animatorUpdateEventArgs.Animation.AnimatedFraction;
            int newStrat = (int)evaluator.Evaluate(fraction, start, end);
            int newMid = (int)evaluator.Evaluate(fraction, mid, start);
            int newEnd = (int)evaluator.Evaluate(fraction, end, mid);
            int[] newArray = { newStrat, newMid, newEnd };

            GradientDrawable gradient = (GradientDrawable)Target.IndeterminateDrawable;
            gradient.SetColors(newArray);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;
    }
}
