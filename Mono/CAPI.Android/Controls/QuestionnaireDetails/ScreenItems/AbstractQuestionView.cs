using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
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
        private readonly IMvxAndroidBindingContext bindingActivity;

        private readonly int templateId;


        public void ClearBindings()
        {
            bindingActivity.ClearBindings(this);
        }

        protected IMvxAndroidBindingContext BindingActivity
        {
            get { return bindingActivity; }
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


        public AbstractQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context)
        {
            this.bindingActivity = new MvxAndroidBindingContext(context, bindingActivity.LayoutInflater, source);
            templateId = Resource.Layout.AbstractQuestionView;
            Content = BindingActivity.BindingInflate(templateId, this);
            this.Model = source;
            this.QuestionnairePublicKey = questionnairePublicKey;
            this.CommandService = CapiApplication.CommandService;

            Initialize();

            PostInit();
        }
        protected virtual void Initialize()
        {
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