using System;
using System.IO;
using NUnitLite;

namespace WB.Tests.Android.Instrumentation
{
    /// <summary>
    /// Custom <see cref="global::Android.App.Instrumentation"/> that runs the NUnitLite test
    /// suite contained in this assembly when the test APK is launched via
    /// `adb shell am instrument -w wb.tests.android.instrumentation/.NUnitInstrumentation`.
    /// Test results (NUnit xml) are written to the device external files
    /// directory so they can be pulled off the device by the CI pipeline.
    /// </summary>
    [global::Android.App.Instrumentation(Name = "wb.tests.android.instrumentation.NUnitInstrumentation")]
    public class NUnitInstrumentation : global::Android.App.Instrumentation
    {
        public NUnitInstrumentation(IntPtr handle, global::Android.Runtime.JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        public override void OnCreate(global::Android.OS.Bundle arguments)
        {
            base.OnCreate(arguments);
            Start();
        }

        public override void OnStart()
        {
            base.OnStart();

            var writer = new StringWriter();
            var extendedWriter = new NUnit.Common.ExtendedTextWrapper(writer);
            var resultsPath = Path.Combine(
                Context?.GetExternalFilesDir(null)?.AbsolutePath ?? Path.GetTempPath(),
                "TestResults.xml");

            var runner = new AutoRun(typeof(NUnitInstrumentation).Assembly);
            var returnCode = runner.Execute(new[] { $"--result={resultsPath}" }, extendedWriter, TextReader.Null);

            var results = new global::Android.OS.Bundle();
            results.PutString("stream", writer.ToString());
            results.PutInt("returnCode", returnCode);

            Finish(global::Android.App.Result.Ok, results);
        }
    }
}

