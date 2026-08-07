using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport
{
    [TestFixture]
    [TestOf(typeof(IOAuth2Api))]
    internal class IOAuth2ApiTests
    {
        [Test]
        public async Task when_requesting_token_should_post_to_configured_token_uri()
        {
            // arrange
            var handler = new CapturingMessageHandler();
            var configuredTokenUri = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/token");
            var client = RestService.For<IOAuth2Api>(new HttpClient(handler)
            {
                BaseAddress = configuredTokenUri
            });

            var request = new ExternalStorageAccessTokenRequest
            {
                ClientId = "client-id",
                ClientSecret = "client-secret",
                Code = "code",
                GrantType = "authorization_code",
                RedirectUri = "https://example.com/redirect",
                Scope = "offline_access Files.ReadWrite"
            };

            // act
            await client.GetTokensByAuthorizationCodeAsync(request);

            // assert
            Assert.That(handler.LastRequest?.RequestUri, Is.EqualTo(configuredTokenUri));
        }

        private class CapturingMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.LastRequest = request;
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"token\",\"expires_in\":3600,\"token_type\":\"Bearer\",\"scope\":\"offline_access Files.ReadWrite\",\"refresh_token\":\"refresh\"}", Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            }
        }
    }
}
