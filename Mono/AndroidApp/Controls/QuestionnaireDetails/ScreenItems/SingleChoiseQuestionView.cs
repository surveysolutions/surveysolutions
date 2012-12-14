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
        public SingleChoiseQuestionView(Context context, QuestionView model) : base(context, model)
        {
        }

        public SingleChoiseQuestionView(Context context, IAttributeSet attrs, QuestionView model) : base(context, attrs, model)
        {
        }

        public SingleChoiseQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionView model) : base(context, attrs, defStyle, model)
        {
        }

        public SingleChoiseQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionView model) : base(javaReference, transfer, model)
        {
        }

        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            LayoutInflater layoutInflater =
                (LayoutInflater) this.Context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(Resource.Layout.QuestionView_SingleChoise, this);
            SelectebleQuestionView typedMode = Model as SelectebleQuestionView;
            tvTitle.Text = Model.Text;
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

        protected TextView tvTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvTitle); }
        }
        protected LinearLayout llWrapper
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llWrapper); }
        }
        #endregion
    }
}