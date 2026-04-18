using System.IO;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Properties;

namespace WB.Tests.Integration.TesterResourcesTests
{
    internal class when_getting_login_layout : TesterResourcesTestsContext
    {
        [Test]
        public void should_have_login_account_hint_bound_to_tester_resources()
        {
            var loginLayoutPath = TestEnvironment.GetSourcePath("UI/Tester/WB.UI.Tester/Resources/layout/login.axml");
            var loginLayout = File.ReadAllText(loginLayoutPath);

            loginLayout.Should().Contain("Localization('TesterLoginAccountHintText')");
            TesterUIResources.ResourceManager.GetString("TesterLoginAccountHintText")
                .Should().NotBeNullOrWhiteSpace();
        }
    }
}
