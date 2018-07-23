using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.UI.Shared.Enumerator.Settings
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
