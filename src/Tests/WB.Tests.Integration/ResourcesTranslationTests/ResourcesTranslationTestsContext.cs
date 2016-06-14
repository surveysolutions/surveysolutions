using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    internal class ResourcesTranslationTestsContext
    {
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
                return XDocument
                .Load(fullPathToResX)
                .Root
                .TreeToEnumerable(_ => _.Elements())
                .Where(element => element.Name == "data")
                .OrderBy(element => element.Attribute("name").Value)
                .ToDictionary(
                    element => element.Attribute("name").Value,
                    element => element.Elements().Single().Value
                );
            }
            catch (Exception exc)
            {
                throw new Exception($"Resouce loading error for file {fullPathToResX}", exc);
            }
            
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
    }
}