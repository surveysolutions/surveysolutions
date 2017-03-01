var target = Argument("target", "Default");

Task("Default")
  .Does(() =>
{
  MSBuild("./WB.WebInterview.Stress.sln", configurator =>
    configurator.SetConfiguration("Release")
        .SetVerbosity(Verbosity.Minimal));
});

RunTarget(target);