using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    /// <summary>
    /// Non-blocking translation completeness dashboard.
    ///
    /// Unlike <see cref="when_checking_availability_of_resources_in_languages_comparing_to_English"/>
    /// (which hard-fails the build for the small set of cultures that are already
    /// fully translated), this fixture runs across the FULL canonical culture list
    /// and only WARNS on gaps, so it can run for
    /// partially-translated cultures without breaking CI.
    ///
    /// As a culture reaches 100% for a given resource set, move it from here into
    /// the strict <see cref="when_checking_availability_of_resources_in_languages_comparing_to_English"/>
    /// test's [TestCase] list -- that is the actual "this culture is now fully
    /// supported" acceptance gate.
    /// </summary>
    [TestFixture]
    internal class when_reporting_translation_completeness_for_all_target_cultures : ResourcesTranslationTestsContext
    {
        // Canonical set of cultures mentioned across the product's language-support
        // conversation/docs (docs.mysurvey.solutions/faq/language/) plus "sq" (Albanian),
        // which is already partially shipped in Designer/Headquarters despite not being
        // documented there.
        private static readonly string[] AllTargetCultures =
        {
            "ru", "fr", "es", "zh", "cs", "uk", "ar", "pt", "ka", "ro", "id", "km", "th", "vi", "sq"
        };

        private IReadOnlyList<string> englishResourceFiles;
        private IReadOnlyList<string> englishResourceNames;
        private Dictionary<string, IReadOnlyList<string>> translatedResourceFilesByCulture;

        [OneTimeSetUp]
        public void one_time_setup()
        {
            var allResxFiles = TestEnvironment
                .GetAllFilesInSolution("*.resx")
                .ToList();

            englishResourceFiles = allResxFiles
                .Except(TestEnvironment.GetAllFilesInSolution("*.??.resx"))
                .Except(TestEnvironment.GetAllFilesInSolution("*.??-??.resx"))
                .ToList();

            englishResourceNames = (
                from resourceFile in englishResourceFiles
                let resourceFileName = GetOriginalResourceFileNameWithoutExtension(resourceFile)
                from resource in GetStringResourcesFromResX(resourceFile)
                where IsNotPluralForm(resource.Key) && !string.IsNullOrEmpty(resource.Value)
                select $"{resourceFileName}: {resource.Key}")
                .ToList();

            translatedResourceFilesByCulture = AllTargetCultures.ToDictionary(
                culture => culture,
                culture => (IReadOnlyList<string>)allResxFiles.Where(file => file.EndsWith($".{culture}.resx")).ToList());
        }

        [TestCaseSource(nameof(AllTargetCultures))]
        public void should_report_resource_completeness_for_culture(string culture)
        {
            var translatedResourceFiles = translatedResourceFilesByCulture[culture];

            var missingFiles = englishResourceFiles
                .Where(en => !translatedResourceFiles.Any(tr =>
                    tr.Replace($".{culture}.", ".") == en))
                .OrderBy(x => x)
                .ToList();


            var translatedResourceNames = (
                from resourceFile in translatedResourceFiles
                let resourceFileName = GetTranslatedResourceFileNameWithoutExtension(resourceFile)
                from resource in GetStringResourcesFromResX(resourceFile)
                where IsNotPluralForm(resource.Key) && !string.IsNullOrEmpty(resource.Value)
                select $"{resourceFileName}: {resource.Key}").ToList();

            var missingKeys = englishResourceNames.Except(translatedResourceNames).OrderBy(x => x).ToList();

            var totalFiles = englishResourceFiles.Count;
            var presentFiles = totalFiles - missingFiles.Count;
            var filePct = totalFiles == 0 ? 100.0 : presentFiles * 100.0 / totalFiles;

            var totalKeys = englishResourceNames.Count;
            var presentKeys = totalKeys - missingKeys.Count;
            var keyPct = totalKeys == 0 ? 100.0 : presentKeys * 100.0 / totalKeys;

            TestContext.WriteLine($"Culture '{culture}': files {presentFiles}/{totalFiles} ({filePct:0.0}%), keys {presentKeys}/{totalKeys} ({keyPct:0.0}%)");

            if (missingFiles.Count > 0)
            {
                TestContext.WriteLine($"  Missing files ({missingFiles.Count}):");
                foreach (var f in missingFiles.Take(50))
                    TestContext.WriteLine($"    {f}");
            }

            if (missingKeys.Count > 0)
            {
                TestContext.WriteLine($"  Missing/untranslated keys ({missingKeys.Count}), showing up to 50:");
                foreach (var k in missingKeys.Take(50))
                    TestContext.WriteLine($"    {k}");
            }

            if (missingFiles.Count > 0 || missingKeys.Count > 0)
            {
                Assert.Warn(
                    $"Culture '{culture}' is not yet fully translated: " +
                    $"{missingFiles.Count} file(s) missing, {missingKeys.Count} key(s) missing/untranslated " +
                    $"({keyPct:0.0}% key coverage). See test output for details. " +
                    "This is a tracked, non-blocking gap -- see the language rollout plan.");
            }
        }

        private static bool IsNotPluralForm(string resourceName)
        {
            return !(resourceName.EndsWith("_other") || resourceName.EndsWith("_plural"));
        }
    }
}

