using Android.Util;
using NUnitLite;

namespace WB.Tests.Android.Instrumentation;

[Instrumentation(Name = "wb.tests.android.instrumentation.NUnitInstrumentation")]
public sealed class NUnitInstrumentation : global::Android.App.Instrumentation
{
    public override void OnStart()
    {
        var workDirectory = TargetContext?.FilesDir?.AbsolutePath;
        string[] arguments = string.IsNullOrEmpty(workDirectory) ? [] : [$"--work={workDirectory}"];
        Log.Info(nameof(NUnitInstrumentation), "Starting NUnitLite test run.");
        var exitCode = new AutoRun(typeof(NUnitInstrumentation).Assembly).Execute(arguments);
        Log.Info(nameof(NUnitInstrumentation), $"NUnitLite test run completed with exit code {exitCode}.");
        var results = new Bundle();
        results.PutInt("nunit.exitCode", exitCode);
        Finish(exitCode == 0 ? Result.Ok : Result.Canceled, results);
    }
}

