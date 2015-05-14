using System;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public enum LinearLayoutStyle
    {
        Active,
        Answered,
        Invalid,
        NonAnswered
    }


    public class LinearLayoutStyleBinding : BindingWrapper<LinearLayout, LinearLayoutStyle>
    {
        public LinearLayoutStyleBinding(LinearLayout androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(LinearLayout androidControl, LinearLayoutStyle value)
        {
            UpdateLayoutStyle(value);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        private void UpdateLayoutStyle(LinearLayoutStyle value)
        {
            switch (value)
            {
                case LinearLayoutStyle.Active:
                    SetBackgroundDrawable(Resource.Drawable.question_background_active);
                    break;
                case LinearLayoutStyle.NonAnswered:
                    SetBackgroundDrawable(Resource.Drawable.question_background_nonanswered);
                    break;
                case LinearLayoutStyle.Invalid:
                    SetBackgroundDrawable(Resource.Drawable.question_background_error);
                    break;
                case LinearLayoutStyle.Answered:
                    SetBackgroundDrawable(Resource.Drawable.question_background_answered);
                    break;
            }
        }

        private void SetBackgroundDrawable(int questionBackgroundActive)
        {
            Target.SetBackgroundDrawable(Target.Resources.GetDrawable(questionBackgroundActive));
        }
    }
}