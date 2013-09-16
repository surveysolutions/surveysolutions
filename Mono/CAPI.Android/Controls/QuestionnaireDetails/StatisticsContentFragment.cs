using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.Statistics;
using CAPI.Android.Core.Model.ViewModel.Statistics;
using CAPI.Android.Extensions;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class StatisticsContentFragment : AbstractScreenChangingFragment
    {
        public StatisticsViewModel Model { get; private set; }
        private const string QUESTIONNAIRE_ID = "questionnaireId";
        private Guid? questionnaireKey;
        protected Guid QuestionnaireKey {
            get
            {
                if (!questionnaireKey.HasValue)
                {
                    questionnaireKey = Guid.Parse(Arguments.GetString(QUESTIONNAIRE_ID));
                }
                return questionnaireKey.Value;
            }
        }
    
        protected AlertDialog answeredDilog;
        protected AlertDialog unansweredDilog;
        protected AlertDialog invaliDilog;
        public static StatisticsContentFragment NewInstance(Guid questionnaireKey)
        {
            StatisticsContentFragment myFragment = new StatisticsContentFragment();

            Bundle args = new Bundle();
            args.PutString(QUESTIONNAIRE_ID, questionnaireKey.ToString());
            myFragment.Arguments = args;

            return myFragment;
        }
        public StatisticsContentFragment()
            : base()
        {
        }

        public void RecalculateStatistics()
        {
            if (containerView == null)
                return;
            this.Model =
                CapiApplication.LoadView<StatisticsInput, StatisticsViewModel>(
                    new StatisticsInput(QuestionnaireKey));
            if (this.Model.Status == InterviewStatus.Completed)
            {
                btnComplete.Text = "Reinit";
            }

            btnAnswered.Text = string.Format("Answered - {0}", this.Model.AnsweredQuestions.Count);
            btnAnswered.Enabled = this.Model.AnsweredQuestions.Count != 0;

            btnUnanswered.Text = string.Format("Unanswered - {0}", this.Model.UnansweredQuestions.Count);
            btnUnanswered.Enabled = this.Model.UnansweredQuestions.Count != 0;

            btnInvalid.Text = string.Format("Invalid - {0}", this.Model.InvalidQuestions.Count);
            btnInvalid.Enabled = this.Model.InvalidQuestions.Count != 0;
            tvErrorWarning.Visibility = btnInvalid.Enabled ? ViewStates.Visible : ViewStates.Gone;

            var popupBuilder = new AlertDialog.Builder(this.Activity);
            var unansweredQuestionsView = CreatePopupView(Model.UnansweredQuestions, new Func
                                                                                         <StatisticsQuestionViewModel,
                                                                                         string>
                                                                                         [1
                                                                                         ]
                {
                    (s) => s.Text
                });
            popupBuilder.SetView(unansweredQuestionsView);
            unansweredDilog = popupBuilder.Create();

            popupBuilder = new AlertDialog.Builder(this.Activity);
            var invalidQuestionsView = CreatePopupView(Model.InvalidQuestions, new Func
                                                                                   <StatisticsQuestionViewModel, string>
                                                                                   [3]
                {
                    (s) => s.Text,
                    (s) => s.AnswerString,
                    (s) => s.ErrorMessage
                });

            popupBuilder.SetView(invalidQuestionsView);
            invaliDilog = popupBuilder.Create();

            popupBuilder = new AlertDialog.Builder(this.Activity);
            var answeredQuestionsView =

                CreatePopupView(Model.AnsweredQuestions, new Func
                                                             <StatisticsQuestionViewModel, string>[2
                                                             ]
                    {
                        (s) => s.Text,
                        (s) => s.AnswerString
                    });
            popupBuilder.SetView(answeredQuestionsView);
            answeredDilog = popupBuilder.Create();

        }

        private View CreatePopupView(IList<StatisticsQuestionViewModel> questions, IList<Func<StatisticsQuestionViewModel, string>> valueFucntions)
        {
            var invalidQuestionsView = new ListView(this.Activity);
            invalidQuestionsView.Adapter = new StatisticsDataAdapter(questions, valueFucntions, this.Activity,
                                                                     OnScreenChanged);

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
          
            containerView = inflater.Inflate(Resource.Layout.StatisticsContent, null);
            btnComplete.Click += btnComplete_Click;

            btnAnswered.Click += btnAnswered_Click;
            btnUnanswered.Click += btnUnanswered_Click;
            btnInvalid.Click += btnInvalid_Click;
            RecalculateStatistics();
            return containerView;
        }

        void btnComplete_Click(object sender, EventArgs e)
        {
            if (Model.Status == InterviewStatus.Completed)
            {
                CapiApplication.CommandService.Execute(new RestartInterviewCommand(Model.QuestionnaireId, CapiApplication.Membership.CurrentUser.Id));
            }
            else
            {
                CapiApplication.CommandService.Execute(new CompleteInterviewCommand(Model.QuestionnaireId, CapiApplication.Membership.CurrentUser.Id));
            }
            this.Activity.Finish();
        }

        protected override void OnScreenChanged(Events.ScreenChangedEventArgs evt)
        {
            if (invaliDilog != null && invaliDilog.IsShowing)
                invaliDilog.Hide();
            if (answeredDilog != null && answeredDilog.IsShowing)
                answeredDilog.Hide();
            if (unansweredDilog != null && unansweredDilog.IsShowing)
                unansweredDilog.Hide();
            base.OnScreenChanged(evt);
        }
        void btnUnanswered_Click(object sender, EventArgs e)
        {
            unansweredDilog.Show();
        }

        void btnInvalid_Click(object sender, EventArgs e)
        {
            invaliDilog.Show();
        }

        private void btnAnswered_Click(object sender, EventArgs e)
        {
            answeredDilog.Show();
        }

        protected View containerView;

        protected Button btnAnswered
        {
            get { return containerView.FindViewById<Button>(Resource.Id.btnAnswered); }
        }
        protected Button btnUnanswered
        {
            get { return containerView.FindViewById<Button>(Resource.Id.btnUnanswered); }
        }
        protected Button btnInvalid
        {
            get { return containerView.FindViewById<Button>(Resource.Id.btnInvalid); }
        }
        protected Button btnComplete
        {
            get { return containerView.FindViewById<Button>(Resource.Id.btnComplete); }
        }
        
        protected TextView tvErrorWarning
        {
            get { return containerView.FindViewById<TextView>(Resource.Id.tvErrorWarning); }
        }
        protected EditText etComments
        {
            get { return containerView.FindViewById<EditText>(Resource.Id.etComments); }
        }
        protected TextView tvComments
        {
            get { return containerView.FindViewById<TextView>(Resource.Id.tvComments); }
        }
        
    }
}