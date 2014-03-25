using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using WB.UI.Capi.Implementations.Activities;
using WB.UI.Capi.Implementations.Fragments;
using WB.UI.Shared.Android.Activities;

namespace WB.UI.Capi
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class CreateInterviewActivity : DoubleBackMvxFragmentActivity
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
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.Window.SetSoftInputMode(SoftInput.AdjustPan);

            SetContentView(Resource.Layout.CreateInterview);

            var screen = PrefilledScreenContentFragment.CreatePrefilledScreenContentFragment(this.QuestionnaireId);

            this.SupportFragmentManager.BeginTransaction().Add(Resource.Id.flFragmentHolder, screen).Commit();

            this.btnNext.Click += this.btnNext_Click;
            this.Title = string.Format("Pre-filled questions");
        }

        void btnNext_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(DataCollectionDetailsActivity));
            intent.SetFlags(ActivityFlags.ReorderToFront);
            intent.PutExtra("publicKey", this.QuestionnaireId.ToString());
            this.StartActivity(intent);
        }
    }
}