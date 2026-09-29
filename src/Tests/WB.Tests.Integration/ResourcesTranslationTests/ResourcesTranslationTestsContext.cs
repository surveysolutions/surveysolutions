using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    internal class ResourcesTranslationTestsContext
    {

        private static readonly Regex UiStringFormatParameterRegex = new Regex(@"{(?!{{)\S+?}}", RegexOptions.Compiled);
        private static readonly Regex StringFormatParameterRegex = new Regex(@"{(?!{)\S+?}", RegexOptions.Compiled);
        private static readonly Dictionary<string, Dictionary<string, string>> ResourceByFileCache =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<string>> LinkedResourcesByProjectCache =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly object ResourceCacheLock = new object();
        private static readonly object LinkedResourcesCacheLock = new object();

        protected static IEnumerable<string> GetStringResourceNamesFromResX(string relativePathToResX)
        {
            string fullPathToResX = TestEnvironment.GetSourcePath(relativePathToResX);

            return GetStringResourcesFromResX(fullPathToResX)
                .Keys
                .OrderBy(x => x)
                .ToList();
        }

        protected static Dictionary<string, string> GetStringResourcesFromResX(string fullPathToResX)
        {
            fullPathToResX = Path.GetFullPath(fullPathToResX);

            lock (ResourceCacheLock)
            {
                if (ResourceByFileCache.TryGetValue(fullPathToResX, out var cached))
                    return cached;
            }

            try
            {
                var doc = XDocument
                    .Load(fullPathToResX)
                    .Root
                    .TreeToEnumerable(_ => _.Elements())
                    .Where(element => element.Name == "data")
                    .OrderBy(element => element.Attribute("name").Value);
                
                var resources = doc.ToDictionary(
                    element => element.Attribute("name").Value,
                    element => element.Elements().Single(x => x.Name == "value").Value
                );

                lock (ResourceCacheLock)
                {
                    ResourceByFileCache[fullPathToResX] = resources;
                }

                return resources;
            }
            catch (Exception exc)
            {
                throw new Exception($"Resouce loading error for file {fullPathToResX}", exc);
            }
        }

        protected static string GetUiStringFormatEntriesAsString(string value)
        {
            return string.Join(",", GetUiStringFormatEntries(value).OrderBy(_ => _));
        }

        private static IEnumerable<string> GetUiStringFormatEntries(string value)
        {
            return UiStringFormatParameterRegex.Matches(value).Cast<Match>().Select(match => match.Value);
        }

        protected static string GetStringFormatEntriesAsString(string value)
        {
            return string.Join(",", GetStringFormatEntries(value).OrderBy(_ => _));
        }

        private static IEnumerable<string> GetStringFormatEntries(string value)
        {
            return StringFormatParameterRegex.Matches(value).Cast<Match>().Select(match => match.Value);
        }

        protected static string ToOriginalResourceFileName(string translatedResourceFileName)
        {
            return RemoveTranslatedResourceFileExtension(translatedResourceFileName) + ".resx";
        }

        protected static string GetOriginalResourceFileNameWithoutExtension(string resourceFileName)
        {
            return RemoveOriginalResourceFileExtension(Path.GetFileName(resourceFileName));
        }

        protected static string GetTranslatedResourceFileNameWithoutExtension(string resourceFileName)
        {
            return RemoveTranslatedResourceFileExtension(Path.GetFileName(resourceFileName));
        }

        private static string RemoveOriginalResourceFileExtension(string resourceFileName)
        {
            return TrimEndAfterLastDot(resourceFileName);
        }

        private static string RemoveTranslatedResourceFileExtension(string resourceFileName)
        {
            return TrimEndAfterLastDot(TrimEndAfterLastDot(resourceFileName));
        }

        private static string TrimEndAfterLastDot(string value)
        {
            return value.Substring(0, value.LastIndexOf('.'));
        }

        protected static bool IsNotPluralForm(string resourceName)
        {
            return !(resourceName.EndsWith("_other") || resourceName.EndsWith("_plural"));
        }

        protected IEnumerable<string> GetAllLinkedResourceFiles(IEnumerable<string> csprojFiles)
        {
            var yieldedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var csproj in csprojFiles)
            {
                foreach (var resourceFile in GetLinkedResourceFilesForProject(csproj))
                {
                    if (yieldedFiles.Add(resourceFile))
                        yield return resourceFile;
                }
            }
        }

        private static IReadOnlyList<string> GetLinkedResourceFilesForProject(string csproj)
        {
            csproj = Path.GetFullPath(csproj);

            lock (LinkedResourcesCacheLock)
            {
                if (LinkedResourcesByProjectCache.TryGetValue(csproj, out var cached))
                    return cached;
            }

            var fi = new FileInfo(csproj);
            if (fi.Directory == null)
                return Array.Empty<string>();

            Console.WriteLine($"Scanning {csproj}");
            var results = new List<string>();

            using (XmlReader reader = XmlReader.Create(csproj))
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (string.Equals(reader.Name, "Project", StringComparison.OrdinalIgnoreCase))
                    {
                        var sdk = reader.GetAttribute("Sdk");

                        if (sdk != null)
                        {
                            Console.WriteLine("Detected new csproj format.");
                            results.AddRange(Directory.EnumerateFiles(fi.Directory.FullName, "*.resx", SearchOption.AllDirectories));
                            break;
                        }
                    }

                    if (!string.Equals(reader.Name, "Content", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(reader.Name, "EmbeddedResource", StringComparison.OrdinalIgnoreCase))
                        continue;

                    while (reader.MoveToNextAttribute())
                    {
                        if (!string.Equals(reader.Name, "Include", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var readerValue = reader.Value.Replace('\\', Path.DirectorySeparatorChar);
                        if (!readerValue.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var fullPath = Path.Combine(fi.Directory.FullName, readerValue);
                        results.Add(fullPath);
                    }
                }
            }

            var distinct = results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            lock (LinkedResourcesCacheLock)
            {
                LinkedResourcesByProjectCache[csproj] = distinct;
            }

            return distinct;
        }
    }
}
