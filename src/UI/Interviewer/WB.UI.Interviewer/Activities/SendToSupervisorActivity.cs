using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Theme = "@style/GrayAppTheme", WindowSoftInputMode = SoftInput.StateHidden)]
    public class SendToSupervisorActivity : BaseActivity<SendToSupervisorViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.send_to_supervisor;
        
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.send_to_supervisor, menu);
            return base.OnCreateOptionsMenu(menu);
        }
    }
}
