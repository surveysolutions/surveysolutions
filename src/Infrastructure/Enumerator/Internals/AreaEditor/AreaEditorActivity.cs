using Android.App;
using Android.Content.PM;
using Android.OS;

namespace WB.Infrastructure.Shared.Enumerator.Internals.AreaEditor
{
    [Activity(ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation, 
        Label = "AreaEditorActivity", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen")]
    public class AreaEditorActivity : Activity
    {
        public static event System.Action<AreaEditResult> OnAreaEditCompleted;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
        }
    }
}