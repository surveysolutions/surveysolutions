using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Fragging;
using WB.UI.QuestionnaireTester.Extensions;
using WB.UI.QuestionnaireTester.Implementations.Activities;
using WB.UI.QuestionnaireTester.Implementations.Fragments;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.QuestionnaireTester
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class CreateInterviewActivity : MvxFragmentActivity
    {
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
            this.CreateActionBar();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.Window.SetSoftInputMode(SoftInput.AdjustPan);

            SetContentView(Resource.Layout.CreateInterview);

            var screen = new PrefilledScreenContentFragment();

            Bundle args = new Bundle();
            args.PutString(ScreenContentFragment.SCREEN_ID, Guid.Empty.ToString());
            args.PutString(ScreenContentFragment.QUESTIONNAIRE_ID, QuestionnaireId.ToString());
            screen.Arguments = args;

            SupportFragmentManager.BeginTransaction().Add(Resource.Id.flFragmentHolder, screen).Commit();

            btnNext.Click += btnNext_Click;
            this.Title = string.Format("List of pre-filled questions");
        }

        void btnNext_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(TesterDetailsActivity));
            intent.SetFlags(ActivityFlags.ReorderToFront);
            intent.PutExtra("publicKey", QuestionnaireId.ToString());
            this.StartActivity(intent);
        }

        public override void OnBackPressed()
        {
            if (isBackWasClickedRecently)
                base.OnBackPressed();

            Toast.MakeText(this, "Press again to exit", ToastLength.Short).Show();
            
            isBackWasClickedRecently = true;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(3000);
                isBackWasClickedRecently = false;
            });
        }

        private bool isBackWasClickedRecently = false;
    }
}