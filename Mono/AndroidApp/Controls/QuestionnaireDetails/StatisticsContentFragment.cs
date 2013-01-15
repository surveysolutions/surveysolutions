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
using AndroidApp.ViewModel.Statistics;
using Fragment = Android.Support.V4.App.Fragment;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class StatisticsContentFragment : Fragment
    {
        public StatisticsViewModel Model { get; private set; }
        protected AlertDialog answeredDilog;
        protected AlertDialog invaliDilog;
        public StatisticsContentFragment(Guid questionnaireKey)
            : base()
        {
            this.Model =
                CapiApplication.LoadView<StatisticsInput, StatisticsViewModel>(new StatisticsInput(questionnaireKey));
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
            btnAnswered.Text += string.Format(" - {0}", this.Model.AnsweredQuestions.Count);
            if (this.Model.AnsweredQuestions.Count == 0)
            {
                btnAnswered.Enabled = false;
            }
            else
            {
                var answeredPopup = new AlertDialog.Builder(this.Activity);
                var answeredQuestionsView = new AnsweredQuestionsView(this.Activity, Model.AnsweredQuestions);
                answeredQuestionsView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent);
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
                var invalidQuestionsView = new InvalidQuestionsView(this.Activity, Model.InvalidQuestions);
                invalidQuestionsView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent);
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

        void btnUnanswered_Click(object sender, EventArgs e)
        {
        
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
        protected TextView tvErrorWarning
        {
            get { return containerView.FindViewById<TextView>(Resource.Id.tvErrorWarning); }
        }

    }
}