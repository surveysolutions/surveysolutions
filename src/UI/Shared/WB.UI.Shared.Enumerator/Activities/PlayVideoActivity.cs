using Android.Content.PM;
using Android.Views;
using Com.Google.Android.Exoplayer2.UI;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities;

[Activity(WindowSoftInputMode = SoftInput.StateHidden, 
    Theme = "@style/AppTheme", 
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
    HardwareAccelerated = false,
    NoHistory = true,
    Exported = false)]
public class PlayVideoActivity: BaseActivity<PlayVideoViewModel>
{
    protected override int ViewResourceId => Resource.Layout.interview_video_view;

    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);
    
        var styledPlayerView = FindViewById<StyledPlayerView>(Resource.Id.video_player_view);
        if (styledPlayerView?.Player != null)
        {
            styledPlayerView.Player.PlayWhenReady = true;
        }
    }

    private void Cancel()
    {
        this.Finish();
    }

    protected override bool BackButtonCustomAction => true;
    protected override void BackButtonPressed()
    {
        Cancel();
    }
}
