using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
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
        protected virtual void Initialize()
        {
            LayoutInflater layoutInflater =
                (LayoutInflater)this.Context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(Resource.Layout.AbstractQuestionView, this);
            tvTitle.Text = Model.Text;
            etComments.Text = tvComments.Text = Model.Comments;
            this.LongClick += new EventHandler<LongClickEventArgs>(AbstractQuestionView_LongClick);
            etComments.FocusChange += new EventHandler<FocusChangeEventArgs>(etComments_FocusChange);
            
        }

        void etComments_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if(!e.HasFocus)
            {
                etComments.Visibility = ViewStates.Gone;
                tvComments.Visibility = ViewStates.Visible;
            }
        }
        protected virtual void PostInit()
        {
            if (!Model.Enabled)

                EnableDisableView(llWrapper, Model.Enabled);
                
            //    EnableDisableView(this, Model.Enabled);
            if (string.IsNullOrEmpty(Model.Instructions))
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
        void AbstractQuestionView_LongClick(object sender, View.LongClickEventArgs e)
        {
            etComments.Visibility = ViewStates.Visible;
            tvComments.Visibility = ViewStates.Gone;
           
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
        protected TextView tvComments
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvComments); }
        }
        protected EditText etComments
        {
            get { return this.FindViewById<EditText>(Resource.Id.etComments); }
        }
    }
}