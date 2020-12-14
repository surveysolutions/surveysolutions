using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using WB.UI.Shared.Web.Versions;

namespace WB.Tests.Web
{
    public class ProductVersionTests
    {
        [Test]
        [TestCase("20.5.1.28407", ExpectedResult = "20.05.1 (build 28407)")]
        [TestCase("20.5.0.28407", ExpectedResult = "20.05 (build 28407)")]
        [TestCase("20.11.0.29407", ExpectedResult = "20.11 (build 29407)")]
        public string should_produce_version_info_for_version_without_hotfix(string productVersion)
        {
            var v = Version.Parse(productVersion);

            var formatVersion = ProductVersion.FormatVersion(v);

            return formatVersion;
        }
    }
}
