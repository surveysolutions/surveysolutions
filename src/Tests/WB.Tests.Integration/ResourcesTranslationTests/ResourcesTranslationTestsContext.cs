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

        protected static IEnumerable<string> GetStringResourceNamesFromResX(string relativePathToResX)
        {
            string fullPathToResX = TestEnvironment.GetSourcePath(relativePathToResX);

            return XDocument
                .Load(fullPathToResX)
                .Root
                .TreeToEnumerable(_ => _.Elements())
                .Where(element => element.Name == "data")
                .OrderBy(element => element.Attribute("name").Value)
                .Select(element => element.Attribute("name").Value);
        }

        protected static Dictionary<string, string> GetStringResourcesFromResX(string fullPathToResX)
        {
            try
            {
                var doc = XDocument
                    .Load(fullPathToResX)
                    .Root
                    .TreeToEnumerable(_ => _.Elements())
                    .Where(element => element.Name == "data")
                    .OrderBy(element => element.Attribute("name").Value);
                
                return doc.ToDictionary(
                    element => element.Attribute("name").Value,
                    element => element.Elements().Single(x => x.Name == "value").Value
                );
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

        protected IEnumerable<string> GetAllLinkedResourceFiles(IEnumerable<string> csprojFiles)
        {
            foreach (var csproj in csprojFiles)
            {
                var fi = new FileInfo(csproj);
                if (fi.Directory == null) continue;

                Console.WriteLine($"Scanning {csproj}");

                using (XmlReader reader = XmlReader.Create(csproj))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (string.Equals(reader.Name, "Project", StringComparison.OrdinalIgnoreCase))
                            {
                                var sdk = reader.GetAttribute("Sdk");

                                if (sdk != null)
                                {
                                    Console.WriteLine($"Detected new csproj format.");
                                    foreach (var resx in Directory.EnumerateFiles(fi.Directory.FullName, "*.resx", SearchOption.AllDirectories))
                                    {
                                        Console.WriteLine($"Got resx file from file system: {resx}");
                                        yield return resx;
                                    }

                                    break;
                                }
                            }

                            if (string.Equals(reader.Name, "Content", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(reader.Name, "EmbeddedResource", StringComparison.OrdinalIgnoreCase))
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (string.Equals(reader.Name, "Include", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (reader.Value.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
                                        {
                                            Console.WriteLine($"Got resx file in csproj: {Path.Combine(fi.Directory.Name, reader.Value)}");
                                            yield return Path.Combine(fi.Directory.FullName, reader.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
