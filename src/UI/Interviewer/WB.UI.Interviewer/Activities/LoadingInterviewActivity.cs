using Android.App;
using Android.Views;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;

namespace WB.UI.Interviewer.Activities
{
    [MvxActivityPresentation]
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        NoHistory = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        Exported = false)]
    public class LoadingInterviewActivity : ProgressInterviewActivity<LoadingInterviewViewModel>
    {
        public override bool IsSupportMenu => true;
    }
}
