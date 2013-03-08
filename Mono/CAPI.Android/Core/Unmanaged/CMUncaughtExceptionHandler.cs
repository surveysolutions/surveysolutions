using Java.Lang;

namespace CAPI.Android.Core.Unmanaged
{
    public class CMUncaughtExceptionHandler : Java.Lang.Object, Java.Lang.Thread.IUncaughtExceptionHandler
    {
        public void UncaughtException(Thread thread, Throwable throwable)
        {
            CapiApplication.Restart();
         //   CapiApplication.IsHardRestart = true;
        }
        
    } 
}
