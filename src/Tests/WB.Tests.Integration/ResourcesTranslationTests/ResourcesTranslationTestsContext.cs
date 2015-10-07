using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    internal class ResourcesTranslationTestsContext
    {
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
    }
}