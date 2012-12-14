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
    public class SingleChoiseQuestionView : AbstractQuestionView
    {
        public SingleChoiseQuestionView(Context context, QuestionViewModel model) : base(context, model)
        {
        }

        public SingleChoiseQuestionView(Context context, IAttributeSet attrs, QuestionViewModel model) : base(context, attrs, model)
        {
        }

        public SingleChoiseQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model) : base(context, attrs, defStyle, model)
        {
        }

        public SingleChoiseQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model) : base(javaReference, transfer, model)
        {
        }

        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            base.Initialize();
            SelectebleQuestionViewModel typedMode = Model as SelectebleQuestionViewModel;
            var radioButton = new RadioButton[typedMode.Answers.Count()];
            RadioGroup radioGroup = new RadioGroup(this.Context);
            radioGroup.Orientation = Orientation.Vertical;
            int i = 0;
            foreach (var answer in typedMode.Answers)
            {
                radioButton[i] = new RadioButton(this.Context);
                radioGroup.AddView(radioButton[i]);
                radioButton[i].Text = answer.Title;
                radioButton[i].Checked = answer.Selected;
            }
            llWrapper.AddView(radioGroup);
        }

   
     
        #endregion
    }
}