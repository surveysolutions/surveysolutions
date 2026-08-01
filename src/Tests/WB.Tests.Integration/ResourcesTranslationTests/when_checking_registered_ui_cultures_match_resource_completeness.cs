using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    // Cross-checks each ASP.NET Core app's registered SupportedUICultures list
    // (in its Startup.cs) against the actual translation completeness of that
    // app's own resource files.
    //
    // Guards against two classes of bugs found in the language support audit:
    //   1. A culture is REGISTERED as supported but its resources are barely
    //      translated .
    //   2. A culture's resources ARE mostly/fully translated but the culture was
    //      never added to SupportedUICultures, so users can never select it
    //
    // This test currently WARNS (non-blocking) for known, tracked gaps rather
    // than failing the build outright, since fixing them is a staged rollout.
    // Once a specific app/culture pair reaches the completeness threshold,
    // the warning simply stops firing for that pair.
    [TestFixture]
    internal class when_checking_registered_ui_cultures_match_resource_completeness : ResourcesTranslationTestsContext
    {
        private static readonly Regex CultureInfoRegex = new Regex(
            @"new\s+CultureInfo\(\s*""([a-zA-Z-]+)""\s*\)", RegexOptions.Compiled);

        private const double MinimumAcceptablePctForRegisteredCulture = 90.0;

        private class AppDefinition
        {
            public string Name;
            public string StartupRelativePath;
            public string[] ResourceDirs;
        }

        private static readonly AppDefinition[] Apps =
        {
            new AppDefinition
            {
                Name = "Designer",
                StartupRelativePath = "UI/WB.UI.Designer/Startup.cs",
                ResourceDirs = new[] { "Core/BoundedContexts/Designer", "UI/WB.UI.Designer" }
            },
            new AppDefinition
            {
                Name = "Headquarters",
                StartupRelativePath = "UI/WB.UI.Headquarters.Core/Startup.cs",
                ResourceDirs = new[] { "Core/BoundedContexts/Headquarters", "UI/WB.UI.Headquarters.Core", "Core/SharedKernels" }
            },
            new AppDefinition
            {
                Name = "WebTester",
                StartupRelativePath = "UI/WB.UI.WebTester/Startup.cs",
                ResourceDirs = new[] { "UI/WB.UI.WebTester", "Core/SharedKernels" }
            },
        };

        private static IEnumerable<string> AppNames() => Apps.Select(a => a.Name);

        [TestCaseSource(nameof(AppNames))]
        public void should_report_registered_cultures_vs_resource_completeness(string appName)
        {
            var app = Apps.Single(a => a.Name == appName);

            var startupPath = TestEnvironment.GetSourcePath(app.StartupRelativePath);
            Assert.That(File.Exists(startupPath), $"Could not find {startupPath} -- has the app been renamed/moved?");

            var startupContent = File.ReadAllText(startupPath);
            var registeredCultures = CultureInfoRegex
                .Matches(startupContent)
                .Select(m => m.Groups[1].Value)
                .Where(c => !string.Equals(c, "en", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            TestContext.WriteLine($"{appName}: registered non-English UI cultures = {string.Join(", ", registeredCultures)}");

            Assert.That(registeredCultures, Is.Not.Empty,
                $"{appName}/Startup.cs: expected to find at least one `new CultureInfo(\"xx\")` " +
                "in SupportedUICultures -- regex may need updating if Startup.cs was refactored.");

            var appResourceFiles = app.ResourceDirs
                .SelectMany(dir => TestEnvironment.GetAllFilesFromSourceFolder(dir, "*.resx"))
                .Distinct()
                .ToList();

            var englishFiles = appResourceFiles
                .Where(f => !Regex.IsMatch(f, @"\.[a-z]{2}(-[A-Za-z]+)?\.resx$", RegexOptions.IgnoreCase))
                .ToList();

            Assert.That(englishFiles, Is.Not.Empty, $"{appName}: could not find any base .resx files under {string.Join(", ", app.ResourceDirs)}");

            var problems = new List<string>();

            foreach (var culture in registeredCultures)
            {
                var missingFiles = new List<string>();
                var missingKeysCount = 0;
                var totalKeysCount = 0;

                foreach (var enFile in englishFiles)
                {
                    var root = enFile.Substring(0, enFile.Length - ".resx".Length);
                    var locFile = $"{root}.{culture}.resx";

                    var enKeys = GetStringResourcesFromResX(enFile)
                        .Where(kv => !kv.Key.EndsWith("_other") && !kv.Key.EndsWith("_plural") && !string.IsNullOrEmpty(kv.Value))
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    totalKeysCount += enKeys.Count;

                    if (!File.Exists(locFile))
                    {
                        missingFiles.Add(Path.GetFileName(enFile));
                        missingKeysCount += enKeys.Count;
                        continue;
                    }

                    var locKeys = GetStringResourcesFromResX(locFile);
                    missingKeysCount += enKeys.Keys.Count(k => !locKeys.ContainsKey(k) || string.IsNullOrEmpty(locKeys[k]));
                }

                var pct = totalKeysCount == 0 ? 100.0 : (totalKeysCount - missingKeysCount) * 100.0 / totalKeysCount;
                TestContext.WriteLine($"  {culture}: {pct:0.0}% key coverage, {missingFiles.Count} file(s) entirely missing");

                // A culture that is REGISTERED but below this threshold is exposed to users
                // while showing mostly-untranslated UI -- this is the "id in HQ" class of bug.
                if (pct < MinimumAcceptablePctForRegisteredCulture)
                {
                    problems.Add($"'{culture}' is registered in {app.StartupRelativePath} but only {pct:0.0}% translated " +
                                 $"({missingFiles.Count} file(s) missing entirely: {string.Join(", ", missingFiles.Take(10))}" +
                                 (missingFiles.Count > 10 ? ", ..." : "") + ")");
                }
            }

            if (problems.Count > 0)
            {
                Assert.Warn($"{appName}: {problems.Count} registered-but-undertranslated culture(s):\n" +
                             string.Join("\n", problems));
            }
        }
    }
}

