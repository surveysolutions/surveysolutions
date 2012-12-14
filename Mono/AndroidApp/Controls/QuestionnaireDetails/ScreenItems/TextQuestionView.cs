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
    public class TextQuestionView : AbstractQuestionView
    {
        public TextQuestionView(Context context, QuestionView model) : base(context, model)
        {
        }

        public TextQuestionView(Context context, IAttributeSet attrs, QuestionView model) : base(context, attrs, model)
        {
        }

        public TextQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionView model) : base(context, attrs, defStyle, model)
        {
        }

        public TextQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionView model) : base(javaReference, transfer, model)
        {
        }

        protected override void Initialize()
        {
            LayoutInflater layoutInflater = (LayoutInflater)this.Context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(Resource.Layout.QuestionView_Text, this);

            tvTitle.Text = Model.Text;
            etAnswer.Text = ((ValueQuestionView) Model).Answer;
        }

        protected TextView tvTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvTitle); }
        }
        protected EditText etAnswer
        {
            get { return this.FindViewById<EditText>(Resource.Id.etAnswer); }
        }
    }
}