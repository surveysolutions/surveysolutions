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

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public class MultyQuestionView : AbstractQuestionView
    {
        public MultyQuestionView(Context context, QuestionView model)
            : base(context, model)
        {
        }

        public MultyQuestionView(Context context, IAttributeSet attrs, QuestionView model)
            : base(context, attrs, model)
        {
        }

        public MultyQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionView model)
            : base(context, attrs, defStyle, model)
        {
        }

        public MultyQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionView model)
            : base(javaReference, transfer, model)
        {
        }

        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            base.Initialize();
            SelectebleQuestionView typedMode = Model as SelectebleQuestionView;

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