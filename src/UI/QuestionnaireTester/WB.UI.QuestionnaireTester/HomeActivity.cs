using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Simple;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.UI.QuestionnaireTester.Implementations.Activities;

namespace WB.UI.QuestionnaireTester
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class HomeActivity : MvxSimpleBindingActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Home);

            var buttonStart = FindViewById<LinearLayout>(Resource.Id.btnStart);

            buttonStart.Click += this.btnLogin_Click;
 
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Guid interviewId = Guid.NewGuid();
            Guid interviewUserId = Guid.NewGuid();
            /*var interview = 

            NcqrsEnvironment.Get<ICommandService>().Execute(new SynchronizeInterviewCommand(interviewId, interviewUserId, interview));

            var intent = new Intent(this, typeof(TesterDetailsActivity));
            intent.PutExtra("publicKey", interviewId.ToString());
            this.StartActivity(intent);*/
        }
    }
}