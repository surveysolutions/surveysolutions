using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Java.Lang;

namespace AndroidApp.Core.Unmanaged
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
