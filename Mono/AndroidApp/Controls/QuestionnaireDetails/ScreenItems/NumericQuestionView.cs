using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public class NumericQuestionView : AbstractQuestionView
    {
       /* public NumericQuestionView(Context context, QuestionViewModel model) : base(context, model)
        {
        }*/

        public NumericQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source)
            : base(context, bindingActivity, source)
        {
        }

       /* public NumericQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model) : base(context, attrs, defStyle, model)
        {
        }

        public NumericQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model) : base(javaReference, transfer, model)
        {
        }*/

        protected override void Initialize()
        {
            base.Initialize();
            etAnswer=new EditText(this.Context);
            etAnswer.Text = ((ValueQuestionViewModel)Model).Answer;
            llWrapper.AddView(etAnswer);
        }

        protected TextView tvTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvTitle); }
        }

        protected EditText etAnswer { get; set; }
    }
}