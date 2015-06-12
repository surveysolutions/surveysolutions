using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Integration.TesterResourcesTests
{
    internal class TesterResourcesTestsContext
    {
        private static readonly string[] AndroidUnitOfMeasures = { "dip", "dp", "sp", "pt", "px", "mm", "in" };

        protected static IEnumerable<string> GetXmlResourcesHavingHardcodedDimensions(string resourcesRelativePath)
        {
            var resourcesFullPath = TestEnvironment.GetSourceFolder(resourcesRelativePath);

            return TestEnvironment
                .GetAllFilesFromSourceFolder(resourcesRelativePath, "*.xml", "*.axml")
                .Where(HasHardcodedDimensions)
                .Select(xmlResourceFullPath => xmlResourceFullPath.Replace(resourcesFullPath, string.Empty).TrimStart('\\'));
        }

        protected static IEnumerable<string> GetDimensionsNames(string resourcesRelativePath)
        {
            return TestEnvironment
                .GetAllFilesFromSourceFolder(resourcesRelativePath, "*.xml")
                .SelectMany(GetDimensionsNamesFromResource);
        }

        private static bool HasHardcodedDimensions(string xmlResourcePath)
        {
            return XDocument
                .Load(xmlResourcePath)
                .Root
                .TreeToEnumerable(_ => _.Elements())
                .Any(HasHardcodedDimensions);
        }

        private static IEnumerable<string> GetDimensionsNamesFromResource(string xmlResourcePath)
        {
            return XDocument
                .Load(xmlResourcePath)
                .Root
                .TreeToEnumerable(_ => _.Elements())
                .Where(element => element.Name == "dimen")
                .Select(element => element.Attribute("name").Value);
        }

        private static bool HasHardcodedDimensions(XElement element)
        {
            return IsDimension(element.Value)
                || element.Attributes().Any(attribute => IsDimension(attribute.Value));
        }

        private static bool IsDimension(string value)
        {
            return AndroidUnitOfMeasures.Any(unitOfMeasure => IsDimension(value, unitOfMeasure));
        }

        private static bool IsDimension(string value, string unitOfMeasure)
        {
            return value.EndsWith(unitOfMeasure)
                && value.Replace(unitOfMeasure, string.Empty).IsDecimal();
        }
    }
}