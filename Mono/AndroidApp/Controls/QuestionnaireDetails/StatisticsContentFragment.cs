using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidApp.Controls.Statistics;
using AndroidApp.Core;
using AndroidApp.Core.Model.ViewModel.Statistics;
using Java.Interop;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Entities.SubEntities;
using Fragment = Android.Support.V4.App.Fragment;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class StatisticsContentFragment : AbstractScreenChangingFragment
    {
        public StatisticsViewModel Model { get; private set; }
        protected AlertDialog answeredDilog;
        protected AlertDialog unansweredDilog;
        protected AlertDialog invaliDilog;
        public StatisticsContentFragment(Guid questionnaireKey)
            : this()
        {
            this.Model =
                CapiApplication.LoadView<StatisticsInput, StatisticsViewModel>(new StatisticsInput(questionnaireKey));
        }
        protected StatisticsContentFragment()
            : base()
        {
            this.RetainInstance = true;
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
            btnAnswered.Text += string.Format(" - {0}", this.Model.AnsweredQuestions.Count);
            if (this.Model.AnsweredQuestions.Count == 0)
            {
                btnAnswered.Enabled = false;
            }
            else
            {
                var answeredPopup = new AlertDialog.Builder(this.Activity);
                var answeredQuestionsView = new StatisticsTableQuestionsView(this.Activity, Model.AnsweredQuestions,
                                                                             OnScreenChanged
                                                                             , new string[2] {"Question", "Answer"},
                                                                             new Func
                                                                                 <StatisticsQuestionViewModel, string>[2
                                                                                 ]
                                                                                 {
                                                                                     (s) => s.Text,
                                                                                     (s) => s.AnswerString
                                                                                 });
                answeredQuestionsView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                                                    ViewGroup.LayoutParams.WrapContent);
                answeredPopup.SetView(answeredQuestionsView);
                //  setAnswerPopup.Show();
                answeredDilog = answeredPopup.Create();
                btnAnswered.Click += btnAnswered_Click;
            }

            btnUnanswered.Text += string.Format(" - {0}", this.Model.UnansweredQuestions.Count);
            if (this.Model.UnansweredQuestions.Count == 0)
            {
                btnUnanswered.Enabled = false;
            }
            else
            {
                var unansweredPopup = new AlertDialog.Builder(this.Activity);
                var unansweredQuestionsView = new StatisticsTableQuestionsView(this.Activity, Model.UnansweredQuestions,
                                                                             OnScreenChanged
                                                                             , new string[1] { "Question"},
                                                                             new Func
                                                                                 <StatisticsQuestionViewModel, string>[1
                                                                                 ]
                                                                                 {
                                                                                     (s) => s.Text
                                                                                 });
                unansweredQuestionsView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                                                    ViewGroup.LayoutParams.WrapContent);
                unansweredPopup.SetView(unansweredQuestionsView);
                unansweredDilog = unansweredPopup.Create();

                btnUnanswered.Click += btnUnanswered_Click;
            }
            btnInvalid.Text += string.Format(" - {0}", this.Model.InvalidQuestions.Count);
            if (this.Model.InvalidQuestions.Count == 0)
            {
                tvErrorWarning.Visibility = ViewStates.Gone;
                btnInvalid.Enabled = false;
            }
            else
            {
                var invalidPopup = new AlertDialog.Builder(this.Activity);
                var invalidQuestionsView = new StatisticsTableQuestionsView(this.Activity, Model.InvalidQuestions,
                                                                    OnScreenChanged, new string[3]{"Question","Answer","Error message"},new Func<StatisticsQuestionViewModel, string>[3]
                                                                        {
                                                                            (s)=> s.Text,
                                                                            (s)=> s.AnswerString,
                                                                            (s)=> s.ErrorMessage
                                                                        } );
                invalidQuestionsView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                invalidPopup.SetView(invalidQuestionsView);
                //  setAnswerPopup.Show();
                invaliDilog = invalidPopup.Create();
                btnInvalid.Click += btnInvalid_Click;
            }


           

            return containerView;
            /*inflater.Inflate(Resource.Layout.ScreenNavigationView, null);
            this.Container.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(Container_ItemClick);*/
            //  return retval;
        }

        void btnComplete_Click(object sender, EventArgs e)
        {
            var command = new ChangeStatusCommand()
                {
                    CompleteQuestionnaireId = Model.QuestionnaireId,
                    Status = Model.InvalidQuestions.Count == 0 ? SurveyStatus.Complete : SurveyStatus.Error,
                    Responsible = CapiApplication.Membership.CurrentUser
                };
            CapiApplication.CommandService.Execute(command);
            this.Activity.StartActivity(typeof(DashboardActivity));
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

    }
}