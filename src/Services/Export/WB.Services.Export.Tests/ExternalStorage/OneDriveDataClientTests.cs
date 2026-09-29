using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using NUnit.Framework;
using WB.Services.Export.ExportProcessHandlers.Externals;

namespace WB.Services.Export.Tests.ExternalStorage
{
    public class OneDriveDataClientTests
    {
        [Test]
        public async Task should_add_bearer_token_to_authorization_header()
        {
            var provider = OneDriveDataClient.CreateAuthenticationProvider("access-token");
            var request = new RequestInformation
            {
                URI = new Uri("https://graph.microsoft.com/v1.0/me/drive")
            };

            await provider.AuthenticateRequestAsync(request);

            Assert.That(request.Headers["Authorization"].Single(), Is.EqualTo("Bearer access-token"));
            Assert.That(request.Headers.ContainsKey("bearer"), Is.False);
        }

        [Test]
        public async Task should_not_send_token_to_an_untrusted_host()
        {
            var provider = OneDriveDataClient.CreateAuthenticationProvider("access-token");
            var request = new RequestInformation
            {
                URI = new Uri("https://example.com/v1.0/me/drive")
            };

            await provider.AuthenticateRequestAsync(request);

            Assert.That(request.Headers.ContainsKey("Authorization"), Is.False);
        }

        [TestCase("")]
        [TestCase(" ")]
        public void should_reject_empty_access_token(string accessToken)
        {
            Assert.That(
                () => OneDriveDataClient.CreateAuthenticationProvider(accessToken),
                Throws.ArgumentException.With.Property("ParamName").EqualTo("accessToken"));
        }
    }
}

