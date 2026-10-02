using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestFixture]
    [TestOf(typeof(CountryListProvider))]
    public class CountryListProviderTests
    {
        [Test]
        public void GetCountryCodes_should_return_unique_non_empty_codes()
        {
            var codes = CountryListProvider.GetCountryCodes();

            codes.Should().NotBeEmpty();
            codes.Should().OnlyHaveUniqueItems();
            codes.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c));
        }

        [Test]
        public void GetCountryCodes_should_contain_well_known_codes()
        {
            CountryListProvider.GetCountryCodes().Should().Contain(new[] { "AFG", "ALB" });
        }

        [Test]
        public void GetCountryItems_should_map_every_code()
        {
            var items = CountryListProvider.GetCountryItems();

            items.Select(i => i.Code).Should().Equal(CountryListProvider.GetCountryCodes());
        }

        [Test]
        public void GetCountryTitleByCode_should_match_item_title()
        {
            var item = CountryListProvider.GetCountryItems().First();

            CountryListProvider.GetCountryTitleByCode(item.Code).Should().Be(item.Title);
        }

        [Test]
        public void GetCountryTitleByCode_should_return_null_for_unknown_code()
        {
            CountryListProvider.GetCountryTitleByCode("NOT_A_COUNTRY").Should().BeNull();
        }

        [Test]
        public void CountryItem_should_hold_values()
        {
            var item = new CountryItem("XYZ", "Title");

            item.Code.Should().Be("XYZ");
            item.Title.Should().Be("Title");
        }
    }
}

