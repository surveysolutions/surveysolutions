using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Main.Core.Commands.Questionnaire.Completed;
using Ncqrs.Commanding.ServiceModel;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public abstract class AbstractQuestionView : LinearLayout
    {
        public event EventHandler AnswerSet;
        public bool IsCommentsEditorFocused { get; private set; }
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

       

      

        protected virtual void SaveAnswer()
        {
            OnAnswerSet();
        }

        protected void OnAnswerSet()
        {
            var handler = AnswerSet;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #region 

        private void AbstractQuestionView_LongClick(object sender, LongClickEventArgs e)
        {
            IsCommentsEditorFocused = true;
            SetEditCommentsVisibility(true);
            etComments.RequestFocus();
        }
        void etComments_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            IsCommentsEditorFocused = e.HasFocus;
            if (!e.HasFocus)
            {
                SaveComment();
                HideKeyboard(etComments);
            }
            else
            {
                ShowKeyboard(etComments);
            }
        }
        
        protected void SaveComment()
        {
            string newComments = etComments.Text.Trim();
            if (newComments != this.Model.Comments)
            {
                CommandService.Execute(new SetCommentCommand(this.QuestionnairePublicKey, this.Model.PublicKey.PublicKey, newComments, this.Model.PublicKey.PropagationKey,
                                                             CapiApplication.Membership.CurrentUser));
            }
            SetEditCommentsVisibility(false);
            etComments.Text = tvComments.Text;
            
        }

        private void etComments_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            etComments.ClearFocus();
        }

        protected void HideKeyboard(EditText editor)
        {
            InputMethodManager imm
                = (InputMethodManager) this.Context.GetSystemService(
                    Context.InputMethodService);
            imm.HideSoftInputFromWindow(editor.WindowToken, 0);
        }

        protected void ShowKeyboard(EditText editor)
        {
            InputMethodManager imm
                = (InputMethodManager) this.Context.GetSystemService(
                    Context.InputMethodService);
            imm.ShowSoftInput(editor, 0);
        }

        #endregion

        private void SetEditCommentsVisibility(bool visible)
        {
            etComments.Visibility = tvCommentsTitle.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
            tvComments.Visibility = visible ? ViewStates.Gone : ViewStates.Visible;
        }
        protected override void OnAttachedToWindow()
        {
            llWrapper.EnableDisableView(this.Model.Status.HasFlag(QuestionStatus.Enabled));
            base.OnAttachedToWindow();
        }
        protected virtual void PostInit()
        {
           
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