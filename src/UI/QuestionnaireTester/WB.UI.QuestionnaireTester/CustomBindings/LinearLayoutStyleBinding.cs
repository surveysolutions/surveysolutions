using System;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
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
            UpdateLayoutStyle(androidControl, value);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        private void UpdateLayoutStyle(LinearLayout androidControl, LinearLayoutStyle value)
        {
            switch (value)
            {
                case LinearLayoutStyle.Active:
                case LinearLayoutStyle.NonAnswered:
                    androidControl.SetBackgroundDrawable(androidControl.Resources.GetDrawable(Resource.Drawable.active_question_background));
                    break;
                case LinearLayoutStyle.Invalid:
                    androidControl.SetBackgroundDrawable(androidControl.Resources.GetDrawable(Resource.Drawable.error_question_background));
                    break;
                case LinearLayoutStyle.Answered:
                    Drawable transparentDrawable = new ColorDrawable(Color.Transparent);
                    androidControl.SetBackgroundDrawable(transparentDrawable);
                    break;

                default:
                    throw new ArgumentException("LinearLayoutStyle");
            }
        }
    }
}