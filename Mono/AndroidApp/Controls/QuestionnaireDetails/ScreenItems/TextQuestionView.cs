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
    public class TextQuestionView : AbstractQuestionView
    {
      /*  public TextQuestionView(Context context, QuestionViewModel model) : base(context, model)
        {
        }*/

        public TextQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source)
            : base(context, bindingActivity, source)
        {
        }

     /*   public TextQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model) : base(context, attrs, defStyle, model)
        {
        }

        public TextQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model) : base(javaReference, transfer, model)
        {
        }*/

        protected override void Initialize()
        {
            base.Initialize();
            etAnswer=new EditText(this.Context);
           
            etAnswer.Text = ((ValueQuestionViewModel) Model).Answer;
            llWrapper.AddView(etAnswer);
        }


        protected EditText etAnswer { get; set; }
    }
}