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
            PostInit();
        }

        public AbstractQuestionView(Context context, IAttributeSet attrs, QuestionView model)
            : base(context, attrs)
        {
            this.Model = model;
            Initialize();
            PostInit();
        }

        public AbstractQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionView model)
            : base(context, attrs, defStyle)
        {
            this.Model = model;
            Initialize();
            PostInit();
        }

        protected AbstractQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionView model)
            : base(javaReference, transfer)
        {
            this.Model = model;
            Initialize();
            PostInit();

        }

        protected virtual void PostInit()
        {
            if (!Model.Enabled)
                EnableDisableView(this, Model.Enabled);
            if(string.IsNullOrEmpty(Model.Instructions))
                btnInstructions.Visibility = ViewStates.Gone;
            else
            {
                btnInstructions.Click += new EventHandler(btnInstructions_Click);
            }

        }

        void btnInstructions_Click(object sender, EventArgs e)
        {
            var instructionsBuilder = new AlertDialog.Builder(this.Context);
            instructionsBuilder.SetMessage(Model.Instructions);
            instructionsBuilder.Show();
        }

        private void EnableDisableView(View view, bool enabled)
        {
            view.Enabled = enabled;
            ViewGroup group = view as ViewGroup;
            if (group != null)
            {

                for (int idx = 0; idx < group.ChildCount; idx++)
                {
                    EnableDisableView(group.GetChildAt(idx), enabled);
                }
            }

        }

        protected virtual void Initialize()
        {
            LayoutInflater layoutInflater =
                (LayoutInflater) this.Context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(Resource.Layout.AbstractQuestionView, this);
            tvTitle.Text = Model.Text;
        }

        protected TextView tvTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvTitle); }
        }
        protected Button btnInstructions
        {
            get { return this.FindViewById<Button>(Resource.Id.btnInstructions); }
        }
        protected LinearLayout llWrapper
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llWrapper); }
        }
    }
}