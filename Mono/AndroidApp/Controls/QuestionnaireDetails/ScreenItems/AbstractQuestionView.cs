using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using AndroidApp.Extensions;
using Cirrious.MvvmCross.Binding.Droid.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Main.Core.Commands.Questionnaire.Completed;
using Ncqrs.Commanding.ServiceModel;

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public abstract class AbstractQuestionView : LinearLayout
    {
        protected QuestionViewModel Model { get; private set; }

        protected Guid QuestionnairePublicKey { get; private set; }
        protected ICommandService CommandService { get; private set; }
        private readonly IMvxBindingActivity _bindingActivity;

        private readonly int _templateId;


        public void ClearBindings()
        {
            _bindingActivity.ClearBindings(this);
        }

        protected IMvxBindingActivity BindingActivity
        {
            get { return _bindingActivity; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearBindings();
            }

            base.Dispose(disposing);
        }

        protected View Content { get; set; }


        public AbstractQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context)
        {
            _bindingActivity = bindingActivity;
            _templateId = Resource.Layout.AbstractQuestionView;
            Content = BindingActivity.BindingInflate(source, _templateId, this);
            this.Model = source;
            this.QuestionnairePublicKey = questionnairePublicKey;
            this.CommandService = CapiApplication.CommandService;

            Initialize();

            PostInit();
        }
        
        //public AbstractQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model)
        //    : base(context, attrs, defStyle)
        //{
        //    this.Model = model;
        //    Initialize();
        //    PostInit();
        //}

        //protected AbstractQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model)
        //    : base(javaReference, transfer)
        //{
        //    this.Model = model;
        //    Initialize();
        //    PostInit();

        //}
        protected virtual void Initialize()
        {
         /*   LayoutInflater layoutInflater =
                (LayoutInflater)this.Context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(Resource.Layout.AbstractQuestionView, this);*/
        //    tvTitle.Text = Model.Text + (Model.Mandatory ? "*" : "");
        //    etComments.Text = tvComments.Text = Model.Comments;
            etComments.ImeOptions = ImeAction.Done;
            etComments.SetSelectAllOnFocus(true);
            etComments.SetSingleLine(true);
            etComments.EditorAction += etComments_EditorAction;
            etComments.FocusChange += etComments_FocusChange;
            llWrapper.LongClick += new EventHandler<LongClickEventArgs>(AbstractQuestionView_LongClick);
            llWrapper.Clickable = true;
        }

        void etComments_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
            {
                SetEditCommentsVisibility(false);
                etComments.Text = tvComments.Text;
                HideCommentKeyboard();
            }

        }

    
        void etComments_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
         
            CommandService.Execute(new SetCommentCommand(this.QuestionnairePublicKey, this.Model.PublicKey.PublicKey,
                                                         etComments.Text, this.Model.PublicKey.PropagationKey, CapiApplication.Membership.CurrentUser));
            etComments.ClearFocus();
            SetEditCommentsVisibility(false);
            HideCommentKeyboard();
        }
        private void  HideCommentKeyboard()
        {
            InputMethodManager imm
                  = (InputMethodManager)this.Context.GetSystemService(
                      Context.InputMethodService);
            imm.HideSoftInputFromWindow(etComments.WindowToken, 0);
        }
        private void SetEditCommentsVisibility(bool visible)
        {
            etComments.Visibility = tvCommentsTitle.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
            tvComments.Visibility = visible ? ViewStates.Gone : ViewStates.Visible;
        }

        protected virtual void PostInit()
        {
            llWrapper.EnableDisableView(this.Model.Status.HasFlag(QuestionStatus.Enabled));
          /*  this.Enabled = true;*/
            /* else
            {
                if (!Model.Valid)
                    llRoot.SetBackgroundResource(Resource.Drawable.questionInvalidShape);
                else if(Model.Answered)
                    llRoot.SetBackgroundResource(Resource.Drawable.questionAnsweredShape);
            }*/
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
            SetEditCommentsVisibility(true);
            
            etComments.RequestFocus();
           
        }

       


        protected LinearLayout llRoot
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llRoot); }
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
        protected TextView tvCommentsTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvCommentsTitle); }
        }
        protected TextView tvError
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvError); }
        }
        protected EditText etComments
        {
            get { return this.FindViewById<EditText>(Resource.Id.etComments); }
        }
       
    }
}