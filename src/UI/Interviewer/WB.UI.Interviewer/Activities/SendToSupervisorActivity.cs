using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Theme = "@style/GrayAppTheme", WindowSoftInputMode = SoftInput.StateHidden)]
    public class SendToSupervisorActivity : BaseActivity<SendToSupervisorViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.send_to_supervisor;

        //protected override void OnCreate(Bundle bundle)
        //{
        //    base.OnCreate(bundle);

        //    ImageView imageView = (ImageView)FindViewById(Resource.Id.myimg);
        //    AnimatedVectorDrawable animatedVectorDrawable =
        //        (AnimatedVectorDrawable)GetDrawable(Resource.Drawable.loading_carousel);
        //    imageView.SetImageDrawable(animatedVectorDrawable);
        //    animatedVectorDrawable.Start();
        //}

        

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.send_to_supervisor, menu);
            return base.OnCreateOptionsMenu(menu);
        }
    }
}
