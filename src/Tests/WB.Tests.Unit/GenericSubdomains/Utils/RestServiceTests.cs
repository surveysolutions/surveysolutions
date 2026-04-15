using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;
using IHttpClientFactory = WB.Core.Infrastructure.HttpServices.Services.IHttpClientFactory;

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

        /*[Test]
        public void when_host_is_unreachable_should_throw_RestException()
        {
            var settings = Mock.Of<IRestServiceSettings>(x => x.Endpoint == "http://localhost/hq");
            var networkService = Mock.Of<INetworkService>(x => x.IsHostReachable("http://localhost/hq") == false && x.IsNetworkEnabled() == true);
            RestService service = Create.Service.RestService(settings, networkService: networkService);

            // Act
            AsyncTestDelegate act = async () => await service.PostAsync("someUrl", new Progress<TransferProgress>());

            // Assert
            Assert.That(act, Throws.InstanceOf<RestException>().And.Property(nameof(RestException.Type)).EqualTo(RestExceptionType.HostUnreachable));
        }*/
        

        [Test]
        public async Task when_if_add_if_none_match_header_should_not_throw()
        {
            var testUserAgent = "SurveySolutions/11";
            var settings = Mock.Of<IRestServiceSettings>(x => x.Endpoint == "http://localhost/hq" 
                                                              && x.UserAgent == testUserAgent && x.Timeout == TimeSpan.FromMinutes(1));

            var testMessageHandler = new TestMessageHandler();
            var httpClient = new HttpClient(testMessageHandler);
            var httpClientFactory = 
                Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<IHttpStatistician>()) == httpClient);
            RestService service = Create.Service.RestService(httpClientFactory: httpClientFactory, restServiceSettings: settings);

            var hash = Convert.ToBase64String(MD5.Create().ComputeHash(
                Encoding.UTF8.GetBytes("Htest")));
            var headers = new Dictionary<string, string>
            {
                ["If-None-Match"] = hash
            };

            // act
            await service.GetAsync("q", null, null, false, headers, CancellationToken.None);

            // assert
            var httpRequestMessage = testMessageHandler.ExecutedRequests.First();
            Assert.That(httpRequestMessage.Headers.UserAgent.First().Product.Name, Is.EqualTo("SurveySolutions"));
            Assert.That(httpRequestMessage.Headers.UserAgent.First().Product.Version, Is.EqualTo("11"));
        }

        [Test]
        public async Task when_response_has_valid_integrity_header_should_not_throw()
        {
            var settings = Mock.Of<IRestServiceSettings>(x => x.Endpoint == "http://localhost/hq"
                                                              && x.UserAgent == "SurveySolutions/1"
                                                              && x.Timeout == TimeSpan.FromMinutes(1)
                                                              && x.CommunicationIntegrityValidationIgnore == false);

            var responseHeaders = new Dictionary<string, string>
            {
                [IntegrityService.IntegrityHeaderName] = IntegrityService.IntegrityHeaderValue
            };
            var testMessageHandler = new TestMessageHandler(responseHeaders);
            var httpClient = new HttpClient(testMessageHandler);
            var httpClientFactory =
                Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<IHttpStatistician>()) == httpClient);

            var integrityService = new IntegrityService(settings);
            RestService service = Create.Service.RestService(
                restServiceSettings: settings,
                httpClientFactory: httpClientFactory,
                integrityService: integrityService);

            // Act + Assert (should not throw)
            await service.GetAsync("q", null, null, false, null, CancellationToken.None);
        }

        [Test]
        public void when_response_is_missing_integrity_header_should_throw_RestException()
        {
            var settings = Mock.Of<IRestServiceSettings>(x => x.Endpoint == "http://localhost/hq"
                                                              && x.UserAgent == "SurveySolutions/1"
                                                              && x.Timeout == TimeSpan.FromMinutes(1)
                                                              && x.CommunicationIntegrityValidationIgnore == false);

            var testMessageHandler = new TestMessageHandler(); // no integrity header
            var httpClient = new HttpClient(testMessageHandler);
            var httpClientFactory =
                Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<IHttpStatistician>()) == httpClient);

            var integrityService = new IntegrityService(settings);
            RestService service = Create.Service.RestService(
                restServiceSettings: settings,
                httpClientFactory: httpClientFactory,
                integrityService: integrityService);

            // Act
            AsyncTestDelegate act = async () => await service.GetAsync("q", null, null, false, null, CancellationToken.None);

            // Assert
            Assert.That(act, Throws.InstanceOf<RestException>());
        }

        [Test]
        public void when_response_has_incorrect_integrity_header_value_should_throw_RestException()
        {
            var settings = Mock.Of<IRestServiceSettings>(x => x.Endpoint == "http://localhost/hq"
                                                              && x.UserAgent == "SurveySolutions/1"
                                                              && x.Timeout == TimeSpan.FromMinutes(1)
                                                              && x.CommunicationIntegrityValidationIgnore == false);

            var responseHeaders = new Dictionary<string, string>
            {
                [IntegrityService.IntegrityHeaderName] = "wrong-value"
            };
            var testMessageHandler = new TestMessageHandler(responseHeaders);
            var httpClient = new HttpClient(testMessageHandler);
            var httpClientFactory =
                Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<IHttpStatistician>()) == httpClient);

            var integrityService = new IntegrityService(settings);
            RestService service = Create.Service.RestService(
                restServiceSettings: settings,
                httpClientFactory: httpClientFactory,
                integrityService: integrityService);

            // Act
            AsyncTestDelegate act = async () => await service.GetAsync("q", null, null, false, null, CancellationToken.None);

            // Assert
            Assert.That(act, Throws.InstanceOf<RestException>());
        }

        [Test]
        public async Task when_integrity_validation_is_ignored_and_header_is_missing_should_not_throw()
        {
            var settings = Mock.Of<IRestServiceSettings>(x => x.Endpoint == "http://localhost/hq"
                                                              && x.UserAgent == "SurveySolutions/1"
                                                              && x.Timeout == TimeSpan.FromMinutes(1)
                                                              && x.CommunicationIntegrityValidationIgnore == true);

            var testMessageHandler = new TestMessageHandler(); // no integrity header
            var httpClient = new HttpClient(testMessageHandler);
            var httpClientFactory =
                Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<IHttpStatistician>()) == httpClient);

            var integrityService = new IntegrityService(settings);
            RestService service = Create.Service.RestService(
                restServiceSettings: settings,
                httpClientFactory: httpClientFactory,
                integrityService: integrityService);

            // Act + Assert (should not throw even though integrity header is absent)
            await service.GetAsync("q", null, null, false, null, CancellationToken.None);
        }
    }
}
