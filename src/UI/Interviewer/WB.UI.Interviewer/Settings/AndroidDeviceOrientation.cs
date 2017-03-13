using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.UI.Interviewer.Settings
{
    public class AndroidDeviceOrientation : IDeviceOrientation
    {
        public DeviceOrientations GetOrientation()
        {
            IWindowManager windowManager = Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

            var rotation = windowManager.DefaultDisplay.Rotation;
            bool isLandscape = rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270;
            return isLandscape ? DeviceOrientations.Landscape : DeviceOrientations.Portrait;
        }
    }
}