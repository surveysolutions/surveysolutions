using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Commands.Questionnaire.Completed;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public abstract class AbstractQuestionView : LinearLayout, IMvxBindingContextOwner
    {
        public event EventHandler AnswerSet;
        public bool IsCommentsEditorFocused { get; private set; }
        protected QuestionViewModel Model { get; private set; }

        protected Guid QuestionnairePublicKey { get; private set; }
        protected ICommandService CommandService { get; private set; }
        protected IAnswerOnQuestionCommandService AnswerCommandService { get; private set; }
        private readonly IMvxAndroidBindingContext _bindingContext;

        private readonly int templateId;


        public IMvxBindingContext BindingContext
        {
            get { return _bindingContext; }
            set { throw new NotImplementedException("BindingContext is readonly in the question"); }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ClearAllBindings();
            }

            base.Dispose(disposing);
        }

        protected View Content { get; set; }


        public AbstractQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context)
        {
            this._bindingContext = new MvxAndroidBindingContext(context, bindingActivity.LayoutInflater, source);
            templateId = Resource.Layout.AbstractQuestionView;
            Content = _bindingContext.BindingInflate(templateId, this);
            this.Model = source;
            this.QuestionnairePublicKey = questionnairePublicKey;
            this.CommandService = CapiApplication.CommandService;
            this.AnswerCommandService = commandService;

            Initialize();

            PostInit();
        }

        protected virtual void Initialize()
        {
            etComments.ImeOptions = ImeAction.Done;
            etComments.SetSelectAllOnFocus(true);
            etComments.SetSingleLine(true);
            etComments.EditorAction += etComments_EditorAction;
            etComments.ImeOptions = ImeAction.Done;
            etComments.FocusChange += etComments_FocusChange;
            llWrapper.LongClick += AbstractQuestionView_LongClick;
            llWrapper.FocusChange += llWrapper_FocusChange;
            llWrapper.Clickable = true;
            /*llWrapper.Focusable = true;
            llWrapper.FocusableInTouchMode = true;*/
        }

        void llWrapper_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            
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

        private void AbstractQuestionView_LongClick(object sender, LongClickEventArgs e)
        {
            IsCommentsEditorFocused = true;
            SetEditCommentsVisibility(true);
            etComments.RequestFocus();
            ShowKeyboard(etComments);
        }

        void etComments_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            IsCommentsEditorFocused = e.HasFocus;
          /*  if (!e.HasFocus)
            {
                SaveComment();
                HideKeyboard(etComments);
            }*/
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

        protected void ExecuteSaveAnswerCommand(SetAnswerCommand command)
        {
            tvError.Visibility = ViewStates.Gone;
            AnswerCommandService.Execute(command);
        }

        protected virtual void SaveAnswerErrorHappend()
        {
        }

        private Exception GetDippestException(Exception e)
        {
            if (e.InnerException == null)
                return e;
            return GetDippestException(e.InnerException);
        }

        private void etComments_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            SaveComment();
            etComments.ClearFocus();
            HideKeyboard(etComments);
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


        private void SetEditCommentsVisibility(bool visible)
        {
            etComments.Visibility = tvCommentsTitle.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
            tvComments.Visibility = visible ? ViewStates.Invisible : ViewStates.Gone;
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