using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class LoadingActivity : BaseActivity<LoadingViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.loading;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Task.Run(async () =>
            {
                await ViewModel.RestoreInterviewAndNavigateThere();
                this.Finish();
            });
        }

        public override async void OnBackPressed()
        {
            await this.ViewModel.NavigateBackToDashboardCommand.ExecuteAsync();
            this.Finish();
        }
    }
}