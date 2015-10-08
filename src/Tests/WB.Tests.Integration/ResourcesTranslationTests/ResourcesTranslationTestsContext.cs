using System.Collections.Generic;
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
                .Select(element => element.Attribute("name").Value);
        }

        protected static Dictionary<string, string> GetStringResourcesFromResX(string fullPathToResX)
        {
            return XDocument
                .Load(fullPathToResX)
                .Root
                .TreeToEnumerable(_ => _.Elements())
                .Where(element => element.Name == "data")
                .ToDictionary(
                    element => element.Attribute("name").Value,
                    element => element.Elements().Single().Value
                );
        }

        protected static string GetStringFormatEntriesAsString(string value)
        {
            return string.Join(",", GetStringFormatEntries(value).OrderBy(_ => _));
        }

        private static IEnumerable<string> GetStringFormatEntries(string value)
        {
            return StringFormatParameterRegex.Matches(value).Cast<Match>().Select(match => match.Value);
        }
    }
}