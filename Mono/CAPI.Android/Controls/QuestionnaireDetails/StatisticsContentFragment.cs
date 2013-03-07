using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.Statistics;
using CAPI.Android.Core.Model.ViewModel.Statistics;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class StatisticsContentFragment : AbstractScreenChangingFragment
    {
        public StatisticsViewModel Model { get; private set; }
        protected Guid questionnaireKey;
        protected AlertDialog answeredDilog;
        protected AlertDialog unansweredDilog;
        protected AlertDialog invaliDilog;
       // protected AlertDialog.Builder popupBuilder;
        public StatisticsContentFragment(Guid questionnaireKey)
            : this()
        {
            this.questionnaireKey = questionnaireKey;
         /*   this.Model =
                CapiApplication.LoadView<StatisticsInput, StatisticsViewModel>(new StatisticsInput(questionnaireKey));*/
        }

        public StatisticsContentFragment()
            : base()
        {
            this.RetainInstance = true;
        }

        public void RecalculateStatistics()
        {
            if (containerView == null)
                return;
            this.Model =
                CapiApplication.LoadView<StatisticsInput, StatisticsViewModel>(
                    new StatisticsInput(questionnaireKey));
            if (SurveyStatus.IsStatusAllowCapiSync(this.Model.Status))
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
            var unansweredQuestionsView = new StatisticsTableQuestionsView(this.Activity, Model.UnansweredQuestions,
                                                                           OnScreenChanged
                                                                           , new string[1] { "Question" },
                                                                           new Func
                                                                               <StatisticsQuestionViewModel, string>
                                                                               [1
                                                                               ]
                                                                                   {
                                                                                       (s) => s.Text
                                                                                   });
            unansweredQuestionsView.LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);
            popupBuilder.SetView(unansweredQuestionsView);
            unansweredDilog = popupBuilder.Create();

            popupBuilder = new AlertDialog.Builder(this.Activity);
            var invalidQuestionsView = new StatisticsTableQuestionsView(this.Activity, Model.InvalidQuestions,
                                                                        OnScreenChanged,
                                                                        new string[3] { "Question", "Answer", "Error message" },
                                                                        new Func
                                                                            <StatisticsQuestionViewModel, string>[3]
                                                                                {
                                                                                    (s) => s.Text,
                                                                                    (s) => s.AnswerString,
                                                                                    (s) => s.ErrorMessage
                                                                                });
            invalidQuestionsView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                                               ViewGroup.LayoutParams.WrapContent);
            popupBuilder.SetView(invalidQuestionsView);
            invaliDilog = popupBuilder.Create();

            popupBuilder = new AlertDialog.Builder(this.Activity);
            var answeredQuestionsView = new StatisticsTableQuestionsView(this.Activity, Model.AnsweredQuestions,
                                                                         OnScreenChanged
                                                                         , new string[2] { "Question", "Answer" },
                                                                         new Func
                                                                             <StatisticsQuestionViewModel, string>[2
                                                                             ]
                                                                                 {
                                                                                     (s) => s.Text,
                                                                                     (s) => s.AnswerString
                                                                                 });
            answeredQuestionsView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                                                ViewGroup.LayoutParams.WrapContent);
            popupBuilder.SetView(answeredQuestionsView);
            //  setAnswerPopup.Show();
            answeredDilog = popupBuilder.Create();

        }

      /*  private void DestroyDialog(AlertDialog dialog)
        {
            if (dialog != null)
            {
                dialog.Dispose();
                dialog = null;
            }
        }*/

        public override void OnSaveInstanceState(Bundle p0)
        {
            base.OnSaveInstanceState(p0);
            p0.PutString("questionnaireKey", questionnaireKey.ToString());
        }
        public override void OnViewStateRestored(Bundle p0)
        {
            base.OnViewStateRestored(p0);
            if (p0 == null)
                return;
            var publicKey = p0.GetString("questionnaireKey");
            if (string.IsNullOrEmpty(publicKey))
                return;
            questionnaireKey = Guid.Parse(publicKey);
          /*  this.Model =
                CapiApplication.LoadView<StatisticsInput, StatisticsViewModel>(new StatisticsInput(Guid.Parse(publicKey)));*/
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
            /*inflater.Inflate(Resource.Layout.ScreenNavigationView, null);
            this.Container.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(Container_ItemClick);*/
            //  return retval;
        }

        void btnComplete_Click(object sender, EventArgs e)
        {

            var status = SurveyStatus.IsStatusAllowCapiSync(this.Model.Status)
                             ? SurveyStatus.Initial
                             : Model.InvalidQuestions.Count == 0 ? SurveyStatus.Complete : SurveyStatus.Error;
            status.ChangeComment = etComments.Text;
            var command = new ChangeStatusCommand()
                {
                    CompleteQuestionnaireId = Model.QuestionnaireId,
                    Status = status,
                    Responsible = CapiApplication.Membership.CurrentUser
                };
            CapiApplication.CommandService.Execute(command);
         /*   var m = this.Activity.GetSystemService(Context.ActivityService) as ActivityManager;
            var tasks = m.GetRunningTasks(1);
            var dashboard = tasks.Last();*/
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