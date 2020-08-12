using System;
using NUnit.Framework;
using WB.UI.Shared.Web.Versions;

namespace WB.Tests.Web
{
    public class ProductVersionTests 
    {
        [Test]
        public void should_produce_version_info_for_version_without_hotfix()
        {
            var v = Version.Parse("20.5.1.28407");

            var formatVersion = ProductVersion.FormatVersion(v);

            Assert.That(formatVersion, Is.EqualTo("20.05.1 (build 28407)"));
        }

        [Test]
        public void should_produce_version_info_without_hotfix_when_it_is_zero()
        {
            var v = Version.Parse("20.5.0.28407");

            var formatVersion = ProductVersion.FormatVersion(v);

            Assert.That(formatVersion, Is.EqualTo("20.05 (build 28407)"));
        }
        
        [Test]
        public void should_produce_correct_month_info()
        {
            var v = Version.Parse("20.11.0.29407");

            var formatVersion = ProductVersion.FormatVersion(v);

            Assert.That(formatVersion, Is.EqualTo("20.11 (build 29407)"));
        }
    }
}
