using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails.ScreenItems;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Events;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Entities.SubEntities;
using Ninject;

namespace CAPI.Android.Controls.QuestionnaireDetails.Roster
{
    public class RosterQuestionView : LinearLayout, IMvxBindingContextOwner
    {
        private readonly IMvxAndroidBindingContext bindingContext;
        private readonly Context context;
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
            get { return bindingContext; }
            set { throw new NotImplementedException("BindingContext is readonly in the roster view"); }
        }

        public RosterQuestionView(Context context,  QuestionViewModel source, Guid questionnarieId)
            : base(context)
        {
            this.context = context;
            bindingContext = new MvxAndroidBindingContext(context, context.ToBindingContext().LayoutInflater, source);
            this.Model = source;
            this.questionnaireId = questionnarieId;
            this.questionViewFactory = new DefaultQuestionViewFactory(CapiApplication.Kernel.Get<IAnswerOnQuestionCommandService>());
            
            Content = bindingContext.BindingInflate(Resource.Layout.RosterQuestion, this);
            llWrapper.Click += rowViewItem_Click;
        }
        protected override void OnAttachedToWindow()
        {
            llWrapper.EnableDisableView(this.Model.IsEnabled());
            base.OnAttachedToWindow();
        }

        void rowViewItem_Click(object sender, EventArgs e)
        {
            CheckIsDialogWasCreatedBeforeAndCreateIfDialogIsAbsent();
            dialog.Show();
        }

        private void CheckIsDialogWasCreatedBeforeAndCreateIfDialogIsAbsent()
        {
            if (dialog != null)
                return;

            var setAnswerPopup = new AlertDialog.Builder(context);
            questionView = questionViewFactory.CreateQuestionView(context, Model, questionnaireId);
            questionView.AnswerSet += questionView_AnswerSet;
            setAnswerPopup.SetView(questionView);
            
            dialog = setAnswerPopup.Create();
            
        }

        private void questionView_AnswerSet(object sender, AnswerSetEventArgs e)
        {
            tvTitle.Text = e.AnswerSting;
            if (!questionView.IsCommentsEditorFocused)
                if (Model.QuestionType != QuestionType.MultyOption)
                    dialog.Dismiss();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Console.WriteLine(string.Format("disposing roster question '{0}'", Model.Text));

                this.ClearAllBindings();

                if (dialog != null)
                {
                    dialog.Dispose();
                    dialog = null;
                }
            }
            base.Dispose(disposing);
            
        }
    }
}
