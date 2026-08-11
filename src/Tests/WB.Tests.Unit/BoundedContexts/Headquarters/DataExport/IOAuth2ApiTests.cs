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
            Assert.That(handler.LastRequest?.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(handler.LastRequest?.Content?.Headers.ContentType?.MediaType,
                Is.EqualTo("application/x-www-form-urlencoded"));

            var requestBody = await handler.LastRequest!.Content!.ReadAsStringAsync();
            Assert.Multiple(() =>
            {
                Assert.That(requestBody, Does.Contain("client_id=client-id"));
                Assert.That(requestBody, Does.Contain("client_secret=client-secret"));
                Assert.That(requestBody, Does.Contain("code=code"));
                Assert.That(requestBody, Does.Contain("grant_type=authorization_code"));
                Assert.That(requestBody, Does.Contain("redirect_uri=https%3A%2F%2Fexample.com%2Fredirect"));
                Assert.That(requestBody, Does.Contain("scope=offline_access%20Files.ReadWrite"));
            });
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
