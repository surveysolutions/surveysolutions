using Android.Content.PM;
using Android.Views;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities;

[Activity(WindowSoftInputMode = SoftInput.StateHidden, 
    Theme = "@style/AppTheme", 
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
    HardwareAccelerated = false,
    Exported = false)]
public class PlayVideoActivity: BaseActivity<PlayVideoViewModel>
{
    protected override int ViewResourceId => Resource.Layout.interview_video_view;
        
    private void Cancel()
    {
        this.Finish();
    }

    protected override bool BackButtonCustomAction => true;
    protected override void BackButtonPressed()
    {
        Cancel();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
            
        
    }
}
