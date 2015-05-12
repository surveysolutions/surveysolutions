using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace WB.UI.QuestionnaireTester.Views.CustomControls
{
    public class AnswerFrameLinearLayout : MvxLinearLayout
    {
        private bool isAnswered;
        private bool isInvalid;

        public AnswerFrameLinearLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public AnswerFrameLinearLayout(Context context, IAttributeSet attrs, IMvxAdapterWithChangedEvent adapter) : base(context, attrs, adapter)
        {
        }

        public bool Answered
        {
            get { return isAnswered; }
            set 
            { 
                isAnswered = value;
                UpdateLayoutStyle();
            }
        }

        public bool Invalid
        {
            get { return isInvalid; }
            set
            {
                isInvalid = value;
                UpdateLayoutStyle();
            }
        }

        private void UpdateLayoutStyle()
        {
            if (Invalid)
            {
                //SetBackground(Resource.Drawable.error_question_background);
                this.SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.error_question_background));
            }
            else if (Answered)
            {
                //SetBackground(Resource.Drawable.);
                Drawable transparentDrawable = new ColorDrawable(Color.Transparent);
                this.SetBackgroundDrawable(transparentDrawable);
            }
            else
            {
                //SetBackground(Resource.Drawable.active_question_background);
                this.SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.active_question_background));
            }
        }

        private void SetBackground(int drawableId)
        {
            var sdk = Android.OS.Build.VERSION.SdkInt;
            if (sdk < BuildVersionCodes.JellyBean)
            {
                this.SetBackgroundDrawable(Resources.GetDrawable(drawableId));
            }
            else
            {
                //this.SetBackground(Resources.GetDrawable(drawableId));
                this.SetBackground(drawableId);
            }
        }
    }
}