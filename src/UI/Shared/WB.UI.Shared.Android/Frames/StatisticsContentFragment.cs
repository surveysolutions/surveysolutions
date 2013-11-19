using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.Statistics;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.Events;

namespace WB.UI.Shared.Android.Frames
{
    public abstract class StatisticsContentFragment : AbstractScreenChangingFragment
    {
        protected abstract StatisticsViewModel GetStatisticsViewModel(Guid questionnaireId);

        public StatisticsViewModel Model { get; private set; }
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
    
        protected AlertDialog answeredDilog;
        protected AlertDialog unansweredDilog;
        protected AlertDialog invaliDilog;

        public StatisticsContentFragment()
            : base()
        {
        }

        public void RecalculateStatistics()
        {
            if (this.containerView == null)
                return;
            this.Model =
                this.GetStatisticsViewModel(this.QuestionnaireKey);
            if (this.Model.Status == InterviewStatus.Completed)
            {
                this.btnComplete.Text = "Reinit";
            }

            this.btnAnswered.Text = string.Format("Answered - {0}", this.Model.AnsweredQuestions.Count);
            this.btnAnswered.Enabled = this.Model.AnsweredQuestions.Count != 0;

            this.btnUnanswered.Text = string.Format("Unanswered - {0}", this.Model.UnansweredQuestions.Count);
            this.btnUnanswered.Enabled = this.Model.UnansweredQuestions.Count != 0;

            this.btnInvalid.Text = string.Format("Invalid - {0}", this.Model.InvalidQuestions.Count);
            this.btnInvalid.Enabled = this.Model.InvalidQuestions.Count != 0;
            this.tvErrorWarning.Visibility = this.btnInvalid.Enabled ? ViewStates.Visible : ViewStates.Gone;

            var popupBuilder = new AlertDialog.Builder(this.Activity);
            var unansweredQuestionsView = this.CreatePopupView(this.Model.UnansweredQuestions, 
                new Func<StatisticsQuestionViewModel,string>[1]
                {
                    (s) => s.Text
                });
            popupBuilder.SetView(unansweredQuestionsView);
            this.unansweredDilog = popupBuilder.Create();

            popupBuilder = new AlertDialog.Builder(this.Activity);
            var invalidQuestionsView = this.CreatePopupView(this.Model.InvalidQuestions, 
                new Func<StatisticsQuestionViewModel, string>[3]
                    {
                        (s) => s.Text,
                        (s) => s.AnswerString,
                        (s) => s.ErrorMessage
                    });

            popupBuilder.SetView(invalidQuestionsView);
            this.invaliDilog = popupBuilder.Create();

            popupBuilder = new AlertDialog.Builder(this.Activity);
            var answeredQuestionsView = this.CreatePopupView(this.Model.AnsweredQuestions, 
                new Func<StatisticsQuestionViewModel, string>[2]
                    {
                        (s) => s.Text,
                        (s) => s.AnswerString
                    });
            popupBuilder.SetView(answeredQuestionsView);
            this.answeredDilog = popupBuilder.Create();

        }

        private View CreatePopupView(IList<StatisticsQuestionViewModel> questions, IList<Func<StatisticsQuestionViewModel, string>> valueFucntions)
        {
            var invalidQuestionsView = new ListView(this.Activity);
            invalidQuestionsView.Adapter = new StatisticsDataAdapter(questions, valueFucntions, this.Activity,
                                                                     this.OnScreenChanged);

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
            this.RecalculateStatistics();
            return this.containerView;
        }
        
        void btnComplete_Click(object sender, EventArgs e)
        {
            this.PreCompleteAction();
        }

        protected virtual void PreCompleteAction()
        {
            /*
            var logManipulator = CapiApplication.Kernel.Get<IChangeLogManipulator>();

            if (this.Model.Status == InterviewStatus.Completed)
            {
                CapiApplication.CommandService.Execute(
                    new RestartInterviewCommand(this.Model.QuestionnaireId, CapiApplication.Membership.CurrentUser.Id, this.etComments.Text));

                logManipulator.CreateOrReopenDraftRecord(this.Model.QuestionnaireId);
            }
            else
            {
                CapiApplication.CommandService.Execute(
                    new CompleteInterviewCommand(this.Model.QuestionnaireId, CapiApplication.Membership.CurrentUser.Id, this.etComments.Text));

                logManipulator.CloseDraftRecord(this.Model.QuestionnaireId);
            }*/
        }

        protected override void OnScreenChanged(ScreenChangedEventArgs evt)
        {
            if (this.invaliDilog != null && this.invaliDilog.IsShowing)
                this.invaliDilog.Hide();
            if (this.answeredDilog != null && this.answeredDilog.IsShowing)
                this.answeredDilog.Hide();
            if (this.unansweredDilog != null && this.unansweredDilog.IsShowing)
                this.unansweredDilog.Hide();
            base.OnScreenChanged(evt);
        }
        void btnUnanswered_Click(object sender, EventArgs e)
        {
            this.unansweredDilog.Show();
        }

        void btnInvalid_Click(object sender, EventArgs e)
        {
            this.invaliDilog.Show();
        }

        private void btnAnswered_Click(object sender, EventArgs e)
        {
            this.answeredDilog.Show();
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