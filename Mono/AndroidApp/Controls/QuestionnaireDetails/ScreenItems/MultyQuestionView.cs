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
            LayoutInflater layoutInflater =
                (LayoutInflater) this.Context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(Resource.Layout.QuestionView_MultyChoise, this);
            SelectebleQuestionView typedMode = Model as SelectebleQuestionView;
            tvTitle.Text = Model.Text;

            foreach (var answer in typedMode.Answers)
            {
                CheckBox cb = new CheckBox(this.Context);
                cb.Text = answer.Title;
                cb.Checked = answer.Selected;
                llWrapper.AddView(cb);
            }
            /* var radioButton = new RadioButton[typedMode.Answers.Count()];
            RadioGroup radioGroup = new RadioGroup(this.Context);
            radioGroup.Orientation = Orientation.Vertical;
            int i = 0;
            foreach (var answer in typedMode.Answers)
            {
                radioButton[i] = new RadioButton(this.Context);
                radioGroup.AddView(radioButton[i]);
                radioButton[i].Text = answer.Title;
                radioButton[i].Checked = answer.Selected;
            }*/
           
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