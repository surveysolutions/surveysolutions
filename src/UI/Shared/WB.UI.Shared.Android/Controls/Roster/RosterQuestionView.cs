using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Events;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.Shared.Android.Controls.Roster
{
    public class RosterQuestionView : LinearLayout, IMvxBindingContextOwner
    {
        private readonly IMvxAndroidBindingContext bindingContext;
        private readonly Context CurrentContext;
        private readonly Guid questionnaireId;
        private readonly IQuestionViewFactory questionViewFactory;
        protected AlertDialog dialog;
        protected AbstractQuestionView questionView;

        protected QuestionViewModel Model { get; private set; }
        protected View Content { get; set; }

        protected LinearLayout llWrapper
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llWrapper); }
        }

        protected TextView tvTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvTitle); }
        }

        public IMvxBindingContext BindingContext
        {
            get { return this.bindingContext; }
            set { throw new NotImplementedException("BindingContext is readonly in the roster view"); }
        }

        public RosterQuestionView(Context context, QuestionViewModel source, Guid questionnarieId, IQuestionViewFactory questionViewFactory)
            : base(context)
        {
            this.CurrentContext = context;
            this.bindingContext = new MvxAndroidBindingContext(context, context.ToBindingContext().LayoutInflater, source);
            this.Model = source;
            this.questionnaireId = questionnarieId;
            this.questionViewFactory = questionViewFactory;
            
            this.Content = this.bindingContext.BindingInflate(Resource.Layout.RosterQuestion, this);
            this.llWrapper.Click += this.rowViewItem_Click;
        }
        protected override void OnAttachedToWindow()
        {
            this.llWrapper.EnableDisableView(this.Model.IsEnabled());
            base.OnAttachedToWindow();
        }

        void rowViewItem_Click(object sender, EventArgs e)
        {
            this.CheckIsDialogWasCreatedBeforeAndCreateIfDialogIsAbsent();
        }

        private void CheckIsDialogWasCreatedBeforeAndCreateIfDialogIsAbsent()
        {
            if (this.dialog == null)
            {
                var setAnswerPopup = new AlertDialog.Builder(this.CurrentContext);
                this.questionView = this.questionViewFactory.CreateQuestionView(this.CurrentContext, this.Model,
                    this.questionnaireId);

                this.questionView.AnswerSet += this.questionView_AnswerSet;
                this.questionView.AnswerSaved += this.questionView_AnswerSaved;

                ScrollView scrollPane = new ScrollView(this.CurrentContext);
                scrollPane.AddView(questionView);

                setAnswerPopup.SetView(scrollPane);
                this.dialog = setAnswerPopup.Create();

                dialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            }

            if (!this.dialog.IsShowing)
                this.dialog.Show();
        }

        private void questionView_AnswerSet(object sender, AnswerSetEventArgs e)
        {
            this.tvTitle.Text = e.AnswerSting;
        }

        private void questionView_AnswerSaved(object sender, AnswerSavedEventArgs e)
        {
            if (!this.questionView.IsCommentsEditorFocused && !this.questionTypesWhichShouldNotCloseDialog.Contains(this.Model.QuestionType))
                this.dialog.Dismiss();
        }

        private readonly QuestionType[] questionTypesWhichShouldNotCloseDialog = { QuestionType.MultyOption, QuestionType.TextList, QuestionType.Multimedia };

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ClearAllBindings();

                if (this.dialog != null)
                {
                    this.dialog.Dispose();
                    this.dialog = null;
                }
                if (this.questionView != null)
                {
                    this.questionView.AnswerSet -= this.questionView_AnswerSet;
                    this.questionView.AnswerSaved -= this.questionView_AnswerSaved;
                    this.questionView.Dispose();
                    this.questionView = null;
                }
                if (this.llWrapper != null)
                    this.llWrapper.Click -= this.rowViewItem_Click;

            }
            base.Dispose(disposing);
        }
    }
}
