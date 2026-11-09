using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(CountryListProvider))]
    internal class CountryListProviderTests
    {
        [TestCase("BOL", "Plurinational State of Bolivia")]
        [TestCase("CPV", "Republic of Cabo Verde (the)")]
        [TestCase("COD", "Democratic Republic of the Congo (the)")]
        [TestCase("COG", "Republic of the Congo (the)")]
        [TestCase("KNA", "St. Kitts and Nevis")]
        [TestCase("SWZ", "Kingdom of Eswatini")]
        [TestCase("FSM", "Federated States of Micronesia")]
        [TestCase("KOR", "Republic of Korea")]
        [TestCase("PRK", "The Democratic People's Republic of Korea")]
        [TestCase("MDA", "Republic of Moldova")]
        [TestCase("TZA", "United Republic of Tanzania")]
        [TestCase("TUR", "Republic of Türkiye")]
        [TestCase("GBR", "United Kingdom of Great Britain and Northern Ireland")]
        [TestCase("USA", "United States of America")]
        [TestCase("VEN", "Bolivarian Republic of Venezuela")]
        [SetUICulture("en")]
        public void when_getting_country_title_by_code_should_return_requested_official_name(string code, string expectedTitle)
        {
            CountryListProvider.GetCountryTitleByCode(code).Should().Be(expectedTitle);
        }

        [Test]
        public void when_getting_country_items_should_return_items_sorted_by_title()
        {
            var countries = CountryListProvider.GetCountryItems();

            countries.Select(x => x.Code).Should().Equal(countries
                .OrderBy(x => x.Title ?? string.Empty, StringComparer.CurrentCulture)
                .ThenBy(x => x.Code)
                .Select(x => x.Code));
        }
    }
}
