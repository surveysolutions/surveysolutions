using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.UI.Interviewer.CustomControls;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.CustomControls;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "", 
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class DashboardActivity : BaseActivity<DashboardViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.dashboard; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.interviewsList);
            var layoutManager = new LinearLayoutManager(this);
            recyclerView.SetLayoutManager(layoutManager);
            recyclerView.HasFixedSize = true;

            var adapter = new InterviewerDashboardAdapter(this, (IMvxAndroidBindingContext)this.BindingContext);
            recyclerView.Adapter = adapter;
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            this.ViewModel.Synchronization.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "IsSynchronizationInfoShowed")
                    this.SlideDown(this.FindViewById<View>(Resource.Id.synchronization_panel), this.ViewModel.Synchronization.IsSynchronizationInfoShowed);
            };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_synchronization:
                    this.ViewModel.SynchronizationCommand.Execute();
                    break;
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }

        private void SlideDown(View androidControl, bool expand)
        {
            if (androidControl == null) throw new ArgumentException("androidControl");

            var slideDownAnimation = AnimationUtils.LoadAnimation(this, expand ? Resource.Animation.slide_down : Resource.Animation.slide_up);
            slideDownAnimation.Reset();
            androidControl.ClearAnimation();
            androidControl.StartAnimation(slideDownAnimation);
        }

    }
}