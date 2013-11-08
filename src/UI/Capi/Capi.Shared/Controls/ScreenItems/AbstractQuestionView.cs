using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.UI.Capi.Shared.Events;
using WB.UI.Capi.Shared.Extensions;

namespace WB.UI.Capi.Shared.Controls.ScreenItems
{
    public abstract class AbstractQuestionView : LinearLayout, IMvxBindingContextOwner
    {
        public event EventHandler<AnswerSetEventArgs> AnswerSet;
        public bool IsCommentsEditorFocused { get; private set; }
        protected QuestionViewModel Model { get; private set; }
        protected IAuthentication Membership { get; private set; }
        protected Guid QuestionnairePublicKey { get; private set; }
        protected ICommandService CommandService { get; private set; }
        protected IAnswerOnQuestionCommandService AnswerCommandService { get; private set; }
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
                Console.WriteLine(string.Format("disposing question '{0}'", this.Model.Text));

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
            this.llWrapper.FocusChange += this.llWrapper_FocusChange;
            this.llWrapper.Clickable = true;
            /*llWrapper.Focusable = true;
            llWrapper.FocusableInTouchMode = true;*/
        }

        void llWrapper_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            
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

        private void AbstractQuestionView_LongClick(object sender, LongClickEventArgs e)
        {
            this.IsCommentsEditorFocused = true;
            this.SetEditCommentsVisibility(true);
            this.etComments.RequestFocus();
            this.ShowKeyboard(this.etComments);
        }

        void etComments_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            this.IsCommentsEditorFocused = e.HasFocus;
          /*  if (!e.HasFocus)
            {
                SaveComment();
                HideKeyboard(etComments);
            }*/
        }

        private void SaveComment()
        {
            string newComments = this.etComments.Text.Trim();
            if (newComments != this.Model.Comments)
            {
                this.CommandService.Execute(new CommentAnswerCommand(this.QuestionnairePublicKey,
                                                                this.Membership.CurrentUser.Id,
                                                                this.Model.PublicKey.Id,
                                                                this.Model.PublicKey.PropagationVector,
                                                                DateTime.UtcNow,
                                                                newComments));
                this.tvComments.Text = newComments;

            }
            this.SetEditCommentsVisibility(false);
            this.etComments.Text = this.tvComments.Text;

        }

        protected void ExecuteSaveAnswerCommand(AnswerQuestionCommand command)
        {
            this.tvError.Visibility = ViewStates.Gone;
            this.AnswerCommandService.AnswerOnQuestion(command, this.SaveAnswerErrorHandler);
        }

        private void SaveAnswerErrorHandler(Exception ex)
        {
            if (this.isDisposed)
                return;

            ((Activity) this.Context).RunOnUiThread(() =>
            {
                if (this.isDisposed)
                    return;

                this.SaveAnswerErrorHandlerImpl(ex);
            });
        }

        private void SaveAnswerErrorHandlerImpl(Exception ex)
        {
            this.PutAnswerStoredInModelToUI();
            this.FireAnswerSetEvent(this.GetAnswerStoredInModelAsString());

            if (!this.Model.IsEnabled())
                return;

            var logger = ServiceLocator.Current.GetInstance<ILogger>();
            logger.Error("Error on answer set.", ex);
            this.tvError.Visibility = ViewStates.Visible;
            this.tvError.Text = this.GetDeepestException(ex).Message;
            logger.Error("Error message: " + this.tvError.Text);
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
            this.SaveComment();
            this.etComments.ClearFocus();
            this.HideKeyboard(this.etComments);
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
                this.instructionDialog = new AlertDialog.Builder(this.Context);
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
        protected EditText etComments
        {
            get { return this.FindViewById<EditText>(Resource.Id.etComments); }
        }

    }
}