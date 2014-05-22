using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.Events;

namespace WB.UI.Shared.Android.Frames
{
    public abstract class StatisticsContentFragment : AbstractScreenChangingFragment
    {
        protected abstract InterviewViewModel GetInterviewViewModel(Guid interviewId);
        public InterviewViewModel Model { get; private set; }
        public const string QUESTIONNAIRE_ID = "questionnaireId";
        private Guid? questionnaireKey;
        protected Guid QuestionnaireKey {
            get
            {
                if (!this.questionnaireKey.HasValue)
                {
                    this.questionnaireKey = Guid.Parse(this.Arguments.GetString(QUESTIONNAIRE_ID));
                }
                return this.questionnaireKey.Value;
            }
        }
    
        protected AlertDialog openedDilog;

        public StatisticsContentFragment()
            : base()
        {
        }

        private View CreatePopupView(IList<QuestionViewModel> questions, IList<Func<QuestionViewModel, string>> valueFunctions)
        {
            var invalidQuestionsView = new ListView(this.Activity);

            invalidQuestionsView.Adapter = new StatisticsDataAdapter(questions, valueFunctions, this.Activity, ChangeScreen);

            invalidQuestionsView.ScrollingCacheEnabled = false;
            return invalidQuestionsView;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no
                // reason to create our view.
                return null;
            }
          
            this.containerView = inflater.Inflate(Resource.Layout.StatisticsContent, null);
            this.btnComplete.Click += this.btnComplete_Click;

            this.btnAnswered.Click += this.btnAnswered_Click;
            this.btnUnanswered.Click += this.btnUnanswered_Click;
            this.btnInvalid.Click += this.btnInvalid_Click;

            this.Model = this.GetInterviewViewModel(this.QuestionnaireKey);
            Model.PropertyChanged += Model_PropertyChanged;
            if (this.Model.Status == InterviewStatus.Completed)
            {
                this.btnComplete.Text = "Reinit";
            }

            this.UpdateCounters();

            return this.containerView;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Model.PropertyChanged -= Model_PropertyChanged;
            }

            base.Dispose(disposing);
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Statistics")
                return;

            this.UpdateCounters();
        }

        private void UpdateCounters()
        {
            this.btnAnswered.Text = string.Format("Answered - {0}", this.Model.Statistics.AnsweredQuestions.Count);
            this.btnAnswered.Enabled = this.Model.Statistics.AnsweredQuestions.Count != 0;

            this.btnUnanswered.Text = string.Format("Unanswered - {0}", this.Model.Statistics.UnansweredQuestions.Count);
            this.btnUnanswered.Enabled = this.Model.Statistics.UnansweredQuestions.Count != 0;

            this.btnInvalid.Text = string.Format("Invalid - {0}", this.Model.Statistics.InvalidQuestions.Count);
            this.btnInvalid.Enabled = this.Model.Statistics.InvalidQuestions.Count != 0;
            this.tvErrorWarning.Visibility = this.btnInvalid.Enabled ? ViewStates.Visible : ViewStates.Gone;
        }

        private void ChangeScreen(InterviewItemId questionId)
        {
            var screen = Model.Screens.Select(s => s.Value).OfType<QuestionnaireScreenViewModel>().FirstOrDefault(
                s => s.Items.Any(i => i.PublicKey == questionId));

            if (screen == null)
                return;
            this.OnScreenChanged(new ScreenChangedEventArgs(screen.ScreenId));
        }

        void btnComplete_Click(object sender, EventArgs e)
        {
            this.PreCompleteAction();
        }

        protected virtual void PreCompleteAction()
        {
        }

        protected override void OnScreenChanged(ScreenChangedEventArgs evt)
        {
            if (this.openedDilog != null && this.openedDilog.IsShowing)
                this.openedDilog.Hide();
            base.OnScreenChanged(evt);
        }

        void btnUnanswered_Click(object sender, EventArgs e)
        {
            var popupBuilder = new AlertDialog.Builder(this.Activity);
            var unansweredQuestionsView = this.CreatePopupView(this.Model.Statistics.UnansweredQuestions,
                new Func<QuestionViewModel, string>[1]
                {
                    (s) => s.Text
                });
            popupBuilder.SetView(unansweredQuestionsView);
            openedDilog = popupBuilder.Create();
            openedDilog.Show();
        }

        void btnInvalid_Click(object sender, EventArgs e)
        {
            var popupBuilder = new AlertDialog.Builder(this.Activity);
            var invalidQuestionsView = this.CreatePopupView(this.Model.Statistics.InvalidQuestions,
                new Func<QuestionViewModel, string>[3]
                    {
                        (s) => s.Text,
                        (s) => s.AnswerString,
                        (s) => s.IsMandatoryAndEmpty ? s.MandatoryValidationMessage : s.ValidationMessage
                    });

            popupBuilder.SetView(invalidQuestionsView);
            openedDilog = popupBuilder.Create();
            openedDilog.Show();
        }

        private void btnAnswered_Click(object sender, EventArgs e)
        {
            var popupBuilder = new AlertDialog.Builder(this.Activity);
            var answeredQuestionsView = this.CreatePopupView(this.Model.Statistics.AnsweredQuestions,
                new Func<QuestionViewModel, string>[2]
                    {
                        (s) => s.Text,
                        (s) => s.AnswerString
                    });
            popupBuilder.SetView(answeredQuestionsView);
            this.openedDilog = popupBuilder.Create();
            this.openedDilog.Show();
        }

        protected View containerView;

        protected Button btnAnswered
        {
            get { return this.containerView.FindViewById<Button>(Resource.Id.btnAnswered); }
        }
        protected Button btnUnanswered
        {
            get { return this.containerView.FindViewById<Button>(Resource.Id.btnUnanswered); }
        }
        protected Button btnInvalid
        {
            get { return this.containerView.FindViewById<Button>(Resource.Id.btnInvalid); }
        }
        protected Button btnComplete
        {
            get { return this.containerView.FindViewById<Button>(Resource.Id.btnComplete); }
        }
        
        protected TextView tvErrorWarning
        {
            get { return this.containerView.FindViewById<TextView>(Resource.Id.tvErrorWarning); }
        }
        protected EditText etComments
        {
            get { return this.containerView.FindViewById<EditText>(Resource.Id.etComments); }
        }
        protected TextView tvComments
        {
            get { return this.containerView.FindViewById<TextView>(Resource.Id.tvComments); }
        }
        
    }
}