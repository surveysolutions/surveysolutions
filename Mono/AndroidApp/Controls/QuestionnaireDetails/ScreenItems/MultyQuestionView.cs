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
    public class MultyQuestionView : AbstractQuestionView
    {
      /*  public MultyQuestionView(Context context, QuestionViewModel model)
            : base(context, model)
        {
        }
        */
        public MultyQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source)
            : base(context, bindingActivity, source)
        {
        }

        /*public MultyQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model)
            : base(context, attrs, defStyle, model)
        {
        }

        public MultyQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model)
            : base(javaReference, transfer, model)
        {
        }*/

        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            base.Initialize();
            SelectebleQuestionViewModel typedMode = Model as SelectebleQuestionViewModel;

            foreach (var answer in typedMode.Answers)
            {
                CheckBox cb = new CheckBox(this.Context);
                cb.Text = answer.Title;
                cb.Checked = answer.Selected;
                llWrapper.AddView(cb);
            }
           
        }

        #endregion

    }
}