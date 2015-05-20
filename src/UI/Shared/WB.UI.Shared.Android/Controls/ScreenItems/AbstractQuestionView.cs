using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Microsoft.Practices.ServiceLocation;

using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.UI.Shared.Android.Events;
using WB.UI.Shared.Android.Extensions;
using Exception = System.Exception;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public abstract class AbstractQuestionView : LinearLayout, IMvxBindingContextOwner
    {
        public event EventHandler<AnswerSetEventArgs> AnswerSet;
        public event EventHandler<AnswerSavedEventArgs> AnswerSaved;
        
        public bool IsCommentsEditorFocused { get; private set; }
        protected QuestionViewModel Model { get; private set; }
        protected IAuthentication Membership { get; private set; }
        protected Guid QuestionnairePublicKey { get; private set; }
        protected ICommandService CommandService { get; private set; }
        protected IAnswerOnQuestionCommandService AnswerCommandService { get; private set; }
        protected Context CurrentContext { get; private set; }
        private readonly IMvxAndroidBindingContext _bindingContext;

        private readonly int templateId;


        public IMvxBindingContext BindingContext
        {
            get { return this._bindingContext; }
            set { throw new NotImplementedException("BindingContext is readonly in the question"); }
        }

        protected override void Dispose(bool disposing)
        {
            this.isDisposed = true;

            if (disposing)
            {
#if DEBUG
                Console.WriteLine(string.Format("disposing question '{0}'", this.Model.Text));
#endif
                this.ClearAllBindings();

                if (this.instructionDialog != null)
                {
                    this.instructionDialog.Dispose();
                    this.instructionDialog = null;
                }
            }

            base.Dispose(disposing);
        }

        protected View Content { get; set; }


        public AbstractQuestionView(
            Context context, 
            IMvxAndroidBindingContext bindingActivity, 
            QuestionViewModel source, 
            Guid questionnairePublicKey, 
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context)
        {
            this._bindingContext = new MvxAndroidBindingContext(context, bindingActivity.LayoutInflater, source);
            this.templateId = Resource.Layout.AbstractQuestionView;
            this.Content = this._bindingContext.BindingInflate(this.templateId, this);
            this.Model = source;
            this.QuestionnairePublicKey = questionnairePublicKey;
            this.CommandService = commandService;
            this.AnswerCommandService = answerCommandService;
            this.Membership = membership;

            this.CurrentContext = context;
            this.Initialize();

            this.PostInit();
        }

        protected virtual void Initialize()
        {
            this.etComments.ImeOptions = ImeAction.Done;
            this.etComments.SetSelectAllOnFocus(true);
            this.etComments.SetSingleLine(true);
            this.etComments.EditorAction += this.etComments_EditorAction;
            this.etComments.ImeOptions = ImeAction.Done;
            this.etComments.FocusChange += this.etComments_FocusChange;
            this.llWrapper.LongClick += this.AbstractQuestionView_LongClick;
            this.llWrapper.Clickable = true;
        }

        
        protected void SaveAnswer(string newAnswer, AnswerQuestionCommand saveAnswerCommand)
        {
            this.ExecuteSaveAnswerCommand(saveAnswerCommand);

            this.FireAnswerSetEvent(newAnswer);
        }

        private void FireAnswerSetEvent(string newAnswer)
        {
            var handler = this.AnswerSet;
            if (handler != null)
                handler(this, new AnswerSetEventArgs(this.Model.PublicKey, newAnswer));
        }

        protected void FireAnswerSavedEvent()
        {
            var handler = this.AnswerSaved;
            if (handler != null)
                handler(this, new AnswerSavedEventArgs(this.Model.PublicKey));
        }

        private void AbstractQuestionView_LongClick(object sender, LongClickEventArgs e)
        {
            this.SetEditCommentsVisibility(true);
            this.etComments.RequestFocus();
            this.ShowKeyboard(this.etComments);
        }

        void etComments_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
            {
                this.SaveComment();
            }

            this.IsCommentsEditorFocused = e.HasFocus;
        }

        private void SaveComment()
        {
            string newComments = this.etComments.Text.Trim();
            if (newComments != this.Model.Comments)
            {
                this.CommandService.Execute(new CommentAnswerCommand(this.QuestionnairePublicKey,
                                                                this.Membership.CurrentUser.Id,
                                                                this.Model.PublicKey.Id,
                                                                this.Model.PublicKey.InterviewItemPropagationVector,
                                                                DateTime.UtcNow,
                                                                newComments));
                this.tvComments.Text = newComments;

            }
        }

        protected void ExecuteSaveAnswerCommand(AnswerQuestionCommand command)
        {
            this.AnswerCommandService.AnswerOnQuestion(command, this.SaveAnswerErrorHandler, this.SaveAnswerSuccessHandler);
        }

        protected void HideAllErrorMessages()
        {
            this.tvError.Visibility = ViewStates.Gone;
            this.tvMError.Visibility = ViewStates.Gone;
        }

        private void SaveAnswerErrorHandler(Exception ex)
        {
            if (this.isDisposed)
                return;

            ((Activity)this.CurrentContext).RunOnUiThread(() =>
            {
                if (this.isDisposed)
                    return;

                this.SaveAnswerErrorHandlerImpl(ex);
            });
        }

        private void SaveAnswerSuccessHandler(string ex)
        {
            if (this.isDisposed)
                return;

            ((Activity)this.CurrentContext).RunOnUiThread(() =>
            {
                if (this.isDisposed)
                    return;

                this.tvError.Text = Model.ValidationMessage;
                this.FireAnswerSavedEvent();
            });
        }

        private void SaveAnswerErrorHandlerImpl(Exception ex)
        {
            var message = this.GetDeepestException(ex).Message;

            if (ShowErrorMessageOnUi(message)) return;

            var logger = ServiceLocator.Current.GetInstance<ILogger>();
            logger.Error("Error on answer set.", ex);
            logger.Error("Error message: " + this.tvError.Text);
        }

        protected bool ShowErrorMessageOnUi(string message)
        {
            this.PutAnswerStoredInModelToUI();
            this.FireAnswerSetEvent(this.GetAnswerStoredInModelAsString());

            if (!this.Model.IsEnabled())
            {
                return true;
            }

            this.tvError.Visibility = ViewStates.Visible;
            this.tvError.Text = message;
            return false;
        }

        protected abstract string GetAnswerStoredInModelAsString();

        protected abstract void PutAnswerStoredInModelToUI();

        private Exception GetDeepestException(Exception e)
        {
            if (e.InnerException == null)
                return e;
            return this.GetDeepestException(e.InnerException);
        }

        private void etComments_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Done)
            {
                this.etComments.ClearFocus();
                HideKeyboard(this.etComments);
            }
        }

        protected void HideKeyboard(EditText editor)
        {
            InputMethodManager imm = (InputMethodManager)this.CurrentContext.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(editor.WindowToken, 0);
        }

        protected void ShowKeyboard(EditText editor)
        {
            InputMethodManager imm = (InputMethodManager)this.CurrentContext.GetSystemService(Context.InputMethodService);
            imm.ShowSoftInput(editor, 0);
        }

        private void SetEditCommentsVisibility(bool visible)
        {
            this.etComments.Visibility = this.tvCommentsTitle.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
            this.tvComments.Visibility = visible ? ViewStates.Gone : ViewStates.Visible;
        }
        protected override void OnAttachedToWindow()
        {
            this.llWrapper.EnableDisableView(this.Model.IsEnabled());
            base.OnAttachedToWindow();
        }
        protected virtual void PostInit()
        {
           
            if (string.IsNullOrEmpty(this.Model.Instructions))
                this.btnInstructions.Visibility = ViewStates.Gone;
            else
            {
                this.btnInstructions.Click += new EventHandler(this.btnInstructions_Click);
            }
        }

        void btnInstructions_Click(object sender, EventArgs e)
        {
            if (this.instructionDialog == null)
            {
                this.instructionDialog = new AlertDialog.Builder(this.CurrentContext);
                this.instructionDialog.SetMessage(this.Model.Instructions);
            }
            this.instructionDialog.Show();
        }

        private AlertDialog.Builder instructionDialog = null;
        private bool isDisposed;

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
        protected TextView tvMError
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvMError); }
        }
        protected EditText etComments
        {
            get { return this.FindViewById<EditText>(Resource.Id.etComments); }
        }

        protected void InitializeViewAndButtonView(View textView, string buttonText, EventHandler buttonClickHandler)
        {
            var wrapper = new RelativeLayout(this.CurrentContext)
            {
                LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent)
            };

            var buttonLayoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            buttonLayoutParams.AddRule(LayoutRules.AlignParentRight);
            var button = new Button(this.CurrentContext) { Text = buttonText, LayoutParameters = buttonLayoutParams };
            button.Click += buttonClickHandler;

            wrapper.AddView(button);
            wrapper.AddView(textView);

            this.llWrapper.AddView(wrapper);
        }

        protected virtual async Task<bool> ConfirmRosterDecreaseAsync(string[] rosterTitles, int countOfRemovedRows)
        {
            if (rosterTitles.Length == 0 || countOfRemovedRows < 1)
                return true;

            var result = false;

            var alert = new AlertDialog.Builder(this.CurrentContext);

            alert.SetTitle(this.CurrentContext.Resources.GetText(Resource.String.Warning));
            alert.SetMessage(string.Format(this.CurrentContext.Resources.GetText(Resource.String.AreYouSureYouWantToRemoveRowFromRosterFormat), countOfRemovedRows));
            alert.SetPositiveButton(this.CurrentContext.Resources.GetText(Resource.String.Yes), (e, s) => { result = true; });
            alert.SetNegativeButton(this.CurrentContext.Resources.GetText(Resource.String.No), (EventHandler<DialogClickEventArgs>) null);

            var dialog = alert.Create();

            dialog.Show();

            await dialog.Confirmation();

            return result;
        }
    }
}