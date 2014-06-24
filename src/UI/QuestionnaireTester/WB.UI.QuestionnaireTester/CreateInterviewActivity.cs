using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Practices.ServiceLocation;
using WB.UI.QuestionnaireTester.Extensions;
using WB.UI.QuestionnaireTester.Implementations.Activities;
using WB.UI.QuestionnaireTester.Implementations.Fragments;
using WB.UI.Shared.Android.Activities;
using WB.UI.Shared.Android.Controls.ScreenItems;

namespace WB.UI.QuestionnaireTester
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class CreateInterviewActivity : DoubleBackMvxFragmentActivity
    {
        private IAnswerProgressIndicator AnswerProgressIndicator
        {
            get { return ServiceLocator.Current.GetInstance<IAnswerProgressIndicator>(); }
        }

        protected Guid QuestionnaireId
        {
            get { return Guid.Parse(this.Intent.GetStringExtra("publicKey")); }
        }

        protected FrameLayout flFragmentHolder
        {
            get { return this.FindViewById<FrameLayout>(Resource.Id.flFragmentHolder); }
        }

        protected Button btnNext
        {
            get { return this.FindViewById<Button>(Resource.Id.btnNext); }
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar(this.AnswerProgressIndicator);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.Window.SetSoftInputMode(SoftInput.AdjustPan);

            SetContentView(Resource.Layout.CreateInterview);

            var screen = PrefilledScreenContentFragment.CreatePrefilledScreenContentFragment(QuestionnaireId);

            SupportFragmentManager.BeginTransaction().Add(Resource.Id.flFragmentHolder, screen).Commit();

            btnNext.Click += btnNext_Click;
            this.Title = string.Format("Pre-filled questions");
        }

        void btnNext_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(TesterDetailsActivity));
            intent.SetFlags(ActivityFlags.ReorderToFront);
            intent.PutExtra("publicKey", QuestionnaireId.ToString());
            this.StartActivity(intent);
        }
    }
}