using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport
{
    [TestFixture]
    [TestOf(typeof(ExternalStoragesSettings))]
    internal class ExternalStoragesSettingsTests
    {
        [TestCase(
            "https://login.microsoftonline.com/common/oauth2/v2.0",
            "https://login.microsoftonline.com/common/oauth2/v2.0/token")]
        [TestCase(
            "https://login.microsoftonline.com/common/oauth2/v2.0/",
            "https://login.microsoftonline.com/common/oauth2/v2.0/token")]
        [TestCase(
            "https://login.microsoftonline.com/common/oauth2/v2.0/token",
            "https://login.microsoftonline.com/common/oauth2/v2.0/token")]
        [TestCase(
            "https://login.microsoftonline.com/common/oauth2/v2.0/token/",
            "https://login.microsoftonline.com/common/oauth2/v2.0/token")]
        public void should_resolve_token_endpoint(string configuredUri, string expectedUri)
        {
            var settings = new ExternalStoragesSettings.ExternalStorageOAuth2Settings
            {
                TokenUri = configuredUri
            };

            var result = settings.GetTokenEndpointUri();

            Assert.That(result, Is.EqualTo(new Uri(expectedUri)));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("common/oauth2/v2.0")]
        public void should_reject_invalid_token_uri(string configuredUri)
        {
            var settings = new ExternalStoragesSettings.ExternalStorageOAuth2Settings
            {
                TokenUri = configuredUri
            };

            Assert.That(() => settings.GetTokenEndpointUri(),
                Throws.TypeOf<InvalidOperationException>());
        }
    }
}

