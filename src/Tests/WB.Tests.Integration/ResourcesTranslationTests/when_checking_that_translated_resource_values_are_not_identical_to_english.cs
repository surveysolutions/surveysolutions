using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    [TestFixture]
    internal class when_checking_that_translated_resource_values_are_not_identical_to_english : ResourcesTranslationTestsContext
    {
        // Resource files that are excluded entirely from this check, because their values
        // are proper nouns (country names, currency codes, etc.) that legitimately stay
        // the same or nearly the same across many languages and are impractical to fully
        // translate/maintain (e.g. "Country.resx": ISO code -> country name).
        private static readonly HashSet<string> ResourceFilesToIgnore = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Country",
        };

        // Values that are allowed to stay untranslated in ANY language,
        // e.g. abbreviations, brand names, technical tokens, etc.
        // Comparison is case-insensitive.
        private static readonly HashSet<string> AllowedIdenticalValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "OK",
            "PDF",
            "CSV",
            "URL",
            "Email",
            "E-mail",
            "SMS",
            "ID",
            "GPS",
            "DDI",
            "CATI",
            "CAPI",
            "CAWI",
            "Wi-Fi",
            "Survey Solutions",
            "Dropbox",
            "Google Drive",
            "OneDrive",
            "MB",
            "Xlsx",
            "GeoJSON",
            "Shapefiles",
            "Paradata",
            "Internet",
            "ON",
            "OFF",
            "Kb",
            "Tab",
            "Ping:",
            "AI",
            "ApiUser",
            "ESRI API Key",
            // brand / product names
            "The World Bank Group",
            "Survey Solutions Interviewer",
            "Survey Solutions Supervisor",
            "Survey Solutions Questionnaire Tester",
            "Survey Solutions Web Survey",
            "Amazon Simple Email Service (SES)",
            "SendGrid Email Delivery Service",
            "WKT (Well-Known Text)",
            // international/technical loanwords that stay the same across most languages
            "audio",
            "Audio",
            "Macros",
            "Variable",
            "Variables",
            "Variables: {0}",
            "Port",
            // pure formatting / placeholder strings - nothing to translate
            ",",
            "{0} (CAWI)",
            "{0} (v{1})",
            "{0:dd MMM HH:mm}",
            "<li><b>{0}</b> [{1}]</li>",
            "Excel (xlsx)",
            "Tab (txt)",
            "{{current}} / {{count}}",
            "{{width}} × {{height}}px",
            "({{questionsCount}}Q, {{groupsCount}}S, {{rostersCount}}R)",
            "E-mail: {{email}}",
            "ver.",
            "ver. {{version}}",
            "{{title}} (ver. {{version}})",
            "{{name}} (ver. {{version}})",
            "[ver. {{version}}] {{name}} ",
            "[ver. {{version}}] {{name}}",
            "[ver. {1}] {0}",
            "{0} (ver. {1})",
            // technical audio-recording specs (units, no translatable text)
            "32 kbps, Mono, 16 kHz",
            "64 kbps, Mono, 22.05 kHz",
            "64 kbps, Mono, 44.1 kHz",
            "128 kbps, Stereo, 44.1 kHz",
            "128 kbps, Stereo, 48 kHz",
        };

        // Values that are known to be correct, legitimate translations in a *specific*
        // culture even though they are spelled the same as in English (cognates,
        // internationally recognized units/terms, etc.). A word allowed here for "fr"
        // does NOT automatically get allowed for "ar" or any other culture - add it to
        // the relevant culture's set explicitly.
        private static readonly Dictionary<string, HashSet<string>> AllowedIdenticalValuesByCulture =
            new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["fr"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Action", "Date", "Message", "Code", "Format", "Version", "Total",
                    "Configuration", "Diagnostics", "Instruction", "Invitation", "Type",
                    "Public", "Description", "Exception", "Exceptions", "Confirmation",
                    "Info", "Test", "Maintenance",
                    "Questionnaire", "Questionnaires", "Mode", "Point", "Points",
                    "Multipoint", "Question", "Questions", "Sections", "Max",
                    "Min", "Forum", "Contact", "Administration",
                    "Classifications", "Expression", "Consultants",
                    "altitude", "latitude", "longitude", "version {0}", "Invitations",
                    "{0} notifications",
                    "{0} sections", "Questions: {0}", "Sections: {0}",
                    "Percentile 05", "Percentile 50", "Percentile 95",
                },
                ["es"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Legal", "Total", "No", "Original", "original", "Error", "Error:",
                    "Manual", "Supervisor", "supervisor", "1 error", "{{count}} error",
                    "{{interviewer}} (supervisor: {{supervisor}})",
                    "decimal",
                },
                ["pt"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Total", "Legal", "Status", "Designer", "Manual",
                    "Supervisor", "supervisor", "link", "altitude", "latitude", "longitude",
                    "{{interviewer}} (supervisor: {{supervisor}})",
                    "decimal",
                },
                ["cs"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Export", "Test", "Role", "Text", "Import", "Region", "Server URL:",
                    "Metadata",
                },
                ["id"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Admin", "Legal", "Status", "Total", "Edit", "Forum", "Login",
                    "Mode", "Log", "Format", "format", "Metadata", "Status: {{ name }}",
                    "Administrator", "Supervisor", "link", "Label", "polyline", "Host",
                    "Edit: {0}", "Edit Supervisor", "Valid", "format: {{- type}}",
                },
                ["ro"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Legal", "Admin", "Total", "Format", "Administrator", "Contact",
                    "numeric:", "text", "numeric", "Designer", "Export", "Forum", "Manual",
                    "Public", "format: {{- type}}", "Import",
                },
                ["sq"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Forum", "Metadata", "Designer", "roster:", "Administrator",
                },
            };


        [TestCase("ru")]
        [TestCase("es")]
        [TestCase("fr")]
        [TestCase("cs")]
        [TestCase("pt")]
        [TestCase("uk")]
        [TestCase("ar")]
        [TestCase("id")]
        [TestCase("ka")]
        [TestCase("km")]
        [TestCase("ro")]
        [TestCase("th")]
        [TestCase("sq")]
        [TestCase("vi")]
        [TestCase("zh")]
        public void should_not_have_values_identical_to_english(string culture)
        {
            var translatedResourceFiles = TestEnvironment
                .GetAllFilesInSolution($"*.{culture}.resx")
                .ToList();

            Assert.That(translatedResourceFiles, Is.Not.Empty);

            AllowedIdenticalValuesByCulture.TryGetValue(culture, out var cultureAllowedValues);
            cultureAllowedValues ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var identicalResources =
                (from translatedResourceFile in translatedResourceFiles
                 let originalResourceFile = ToOriginalResourceFileName(translatedResourceFile)
                 let resourceFileName = GetTranslatedResourceFileNameWithoutExtension(translatedResourceFile)
                 where !ResourceFilesToIgnore.Contains(resourceFileName)
                 let originalResources = GetStringResourcesFromResX(originalResourceFile)
                 from translatedResource in GetStringResourcesFromResX(translatedResourceFile)
                 where IsNotPluralForm(translatedResource.Key)
                 where !string.IsNullOrWhiteSpace(translatedResource.Value)
                 let normalizedTranslatedValue = NormalizeWhitespace(translatedResource.Value)
                 where !AllowedIdenticalValues.Contains(normalizedTranslatedValue)
                 where !cultureAllowedValues.Contains(normalizedTranslatedValue)
                 where originalResources.ContainsKey(translatedResource.Key)
                 where string.Equals(
                     normalizedTranslatedValue,
                     NormalizeWhitespace(originalResources[translatedResource.Key]),
                     StringComparison.Ordinal)
                 select $"{resourceFileName}: {translatedResource.Key} = '{translatedResource.Value}'")
                .OrderBy(x => x)
                .ToList();

            Assert.That(identicalResources, Is.Empty,
                $"Found {identicalResources.Count} resource(s) with value identical to English. " +
                $"If this is intentional (e.g. abbreviation, proper noun, cognate word that is spelled " +
                $"the same in {culture}), add the value to {nameof(AllowedIdenticalValues)} (all cultures) " +
                $"or to {nameof(AllowedIdenticalValuesByCulture)}[\"{culture}\"] (this culture only). " +
                $"Otherwise, it likely means the resource was never translated:\r\n" +
                string.Join("\r\n", identicalResources));
        }

        private static string NormalizeWhitespace(string value)
        {
            return Regex.Replace(value.Trim(), @"\s+", " ");
        }
    }
}



