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
    public abstract class AbstractQuestionView : LinearLayout
    {
        protected QuestionView Model { get; private set; }


        public AbstractQuestionView(Context context, QuestionView model)
            : base(context)
        {
            this.Model = model;
            Initialize();
        }

        public AbstractQuestionView(Context context, IAttributeSet attrs, QuestionView model)
            : base(context, attrs)
        {
            this.Model = model;
            Initialize();
        }

        public AbstractQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionView model)
            : base(context, attrs, defStyle)
        {
            this.Model = model;
            Initialize();
        }

        protected AbstractQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionView model)
            : base(javaReference, transfer)
        {
            this.Model = model;
            Initialize();
        }

        protected abstract void Initialize();
    }
}