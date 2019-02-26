using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.GenericSubdomains.Utils
{
    [TestOf(typeof(RestService))]
    public class RestServiceTests
    {
        [Test]
        public void when_invalid_endpoint_in_settings_Should_throw_RestException()
        {
            var settings = Mock.Of<IRestServiceSettings>(x => x.Endpoint == "invalid");
            RestService service = Create.Service.RestService(settings);

            // Act
            AsyncTestDelegate act = async () => await service.PostAsync("someUrl", new Progress<TransferProgress>());

            // Assert
            Assert.That(act, Throws.InstanceOf<RestException>().And.Property(nameof(RestException.Type)).EqualTo(RestExceptionType.InvalidUrl));
        }

        [Test]
        public void when_no_network_should_throw_RestException()
        {
            var networkService = Mock.Of<INetworkService>(x => x.IsNetworkEnabled() == false);
            RestService service = Create.Service.RestService(networkService: networkService);

            // Act
            AsyncTestDelegate act = async () => await service.PostAsync("someUrl", new Progress<TransferProgress>());

            // Assert
            Assert.That(act, Throws.InstanceOf<RestException>().And.Property(nameof(RestException.Type)).EqualTo(RestExceptionType.NoNetwork));
        }

        [Test]
        public void when_host_is_unreachable_should_throw_RestException()
        {
            var settings = Mock.Of<IRestServiceSettings>(x => x.Endpoint == "http://localhost/hq");
            var networkService = Mock.Of<INetworkService>(x => x.IsHostReachable("http://localhost/hq") == false && x.IsNetworkEnabled() == true);
            RestService service = Create.Service.RestService(settings, networkService: networkService);

            // Act
            AsyncTestDelegate act = async () => await service.PostAsync("someUrl", new Progress<TransferProgress>());

            // Assert
            Assert.That(act, Throws.InstanceOf<RestException>().And.Property(nameof(RestException.Type)).EqualTo(RestExceptionType.HostUnreachable));
        }

        [Test]
        public async Task should_pass_user_agent_from_rest_service_settings()
        {
            var testUserAgent = "SurveySolutions/11";
            var settings = Mock.Of<IRestServiceSettings>(x => x.Endpoint == "http://localhost/hq" && x.UserAgent == testUserAgent && x.Timeout == TimeSpan.FromMinutes(1));

            var testMessageHandler = new TestMessageHandler();
            var httpClient = new HttpClient(testMessageHandler);
            IHttpClientFactory httpClientFactory = 
                Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<IHttpStatistician>()) == httpClient);
            RestService service = Create.Service.RestService(httpClientFactory: httpClientFactory, restServiceSettings: settings);

            // act
            await service.PostAsync("q", new Progress<TransferProgress>());

            // assert
            var httpRequestMessage = testMessageHandler.ExecutedRequests.First();
            Assert.That(httpRequestMessage.Headers.UserAgent.First().Product.Name, Is.EqualTo("SurveySolutions"));
            Assert.That(httpRequestMessage.Headers.UserAgent.First().Product.Version, Is.EqualTo("11"));
        }

        class TestMessageHandler : HttpMessageHandler
        {
            public List<HttpRequestMessage> ExecutedRequests { get; } = new List<HttpRequestMessage>();

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.ExecutedRequests.Add(request);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }


}
