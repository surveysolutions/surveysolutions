using System;
using Android.App;
using Android.Runtime;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Interviewer.Infrastructure.Internals.Crasher;
using WB.UI.Interviewer.Infrastructure.Internals.Crasher.Attributes;
using WB.UI.Interviewer.Infrastructure.Logging;

namespace WB.UI.Interviewer
{
#if DEBUG
    [Application(Debuggable = true)]
#else
    [Application(Debuggable = false)]
#endif
    [Crasher(UseCustomData = false)]
    public class InterviewerApplication : Application
    {
        protected InterviewerApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            CrashManager.Initialize(this);
            CrashManager.AttachSender(() => new FileReportSender(AndroidPathUtils.GetPathToCrashFile()));
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }
    }
}