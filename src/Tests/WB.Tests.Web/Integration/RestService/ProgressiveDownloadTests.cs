using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using NUnit.Framework;
using Microsoft.Owin.Testing;
using Moq;
using Owin;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Shared.Web.Extensions;

namespace WB.Tests.Integration.RestService
{
    class ProgressiveDownloadTests
    {
        // just a random big enough file
        private static byte[] ActualContent = File.ReadAllBytes(typeof(NHibernate.ISession).Assembly.Location);
        private byte[] ActualContentHash;
        private SHA1 hasher = SHA1.Create();

        [SetUp]
        public void Setup()
        {
            this.ActualContentHash = hasher.ComputeHash(ActualContent);
        }

        [Test]
        public async Task TestThatWeCanDownloadFileInOneChunk()
        {
            using (var server = TestServer.Create(Configuration))
            {
                var response = await server.HttpClient.GetAsync("/api/test");

                var content = await response.Content.ReadAsByteArrayAsync();

                var contentHash = hasher.ComputeHash(content);

                Assert.That(AsString(contentHash), Is.EqualTo(AsString(ActualContentHash)));
                Assert.That(response.Headers.AcceptRanges, Is.Not.Empty);
                Assert.That(response.Content.Headers.ContentLength, Is.EqualTo(content.Length));
                Assert.That(response.Content.Headers.ContentLength, Is.EqualTo(ActualContent.Length));
            }
        }


        [Test]
        public async Task KP_13477__should_not_return_bytes_range_support_for_old_clients()
        {
            // test with old client
            using (var server = TestServer.Create(Configuration))
            {
                var client = server.HttpClient;

                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("org.worldbank.solutions.interviewer",
                    new Version(1, 2, 26644).ToString()));

                var response = await client.GetAsync("/api/test");

                var content = await response.Content.ReadAsByteArrayAsync();
                var contentHash = hasher.ComputeHash(content);

                // assert
                Assert.That(response.Headers.AcceptRanges, Is.Empty);

                Assert.That(AsString(contentHash), Is.EqualTo(AsString(ActualContentHash)));
                Assert.That(response.Content.Headers.ContentLength, Is.EqualTo(content.Length));
                Assert.That(response.Content.Headers.ContentLength, Is.EqualTo(ActualContent.Length));
            }

            // test with newer client
            using (var server = TestServer.Create(Configuration))
            {
                var client = server.HttpClient;

                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("org.worldbank.solutions.interviewer",
                    new Version(1, 2, 26644 + 1).ToString()));

                var response = await client.GetAsync("/api/test");

                var content = await response.Content.ReadAsByteArrayAsync();
                var contentHash = hasher.ComputeHash(content);

                // assert
                Assert.That(response.Headers.AcceptRanges, Is.Not.Empty);

                Assert.That(AsString(contentHash), Is.EqualTo(AsString(ActualContentHash)));
                Assert.That(response.Content.Headers.ContentLength, Is.EqualTo(content.Length));
                Assert.That(response.Content.Headers.ContentLength, Is.EqualTo(ActualContent.Length));
            }
        }

        [Test]
        public async Task should_respect_range_requests()
        {
            using (var server = TestServer.Create(Configuration))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

                request.Headers.Range = new RangeHeaderValue(100, 109);

                var http = server.HttpClient;

                var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                var content = await response.Content.ReadAsByteArrayAsync();

                var actualSpan = ActualContent.Skip(100).Take(10).ToArray();

                Assert.That(AsString(actualSpan), Is.EqualTo(AsString(content)));
            }
        }

        [Test]
        public async Task should_be_possible_to_download_file_using_multiple_chunks()
        {
            using (var server = TestServer.Create(Configuration))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

                var http = server.HttpClient;

                var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                var downloader = new FastBinaryFilesHttpHandler(
                    Mock.Of<IHttpClientFactory>(r =>
                        r.CreateClient(It.IsAny<IHttpStatistician>()) == server.HttpClient),
                    Mock.Of<IRestServiceSettings>(r => r.BufferSize == 4096 && r.MaxDegreeOfParallelism == 1), null);

                var content = await downloader.DownloadBinaryDataAsync(http, response, null, CancellationToken.None);
                var contentHash = hasher.ComputeHash(content);


                Assert.That(AsString(contentHash), Is.EquivalentTo(AsString(ActualContentHash)));
            }
        }

        private string AsString(byte[] hash) => string.Join("", hash.Select(b => b.ToString("x2")).ToArray());

        private void Configuration(IAppBuilder app)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(name: "DefaultApi", routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Services.Replace(typeof(IHttpControllerTypeResolver), new HttpControllerTypeResolver());
            app.UseWebApi(config);
        }

        private class TestController : ApiController
        {
            [HttpGet]
            public HttpResponseMessage Get()
            {
                var progressive = new ProgressiveDownload(Request);
                return progressive.ResultMessage(new MemoryStream(ActualContent), "application/dll");
            }
        }

        private class HttpControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new List<Type> { typeof(TestController) };
            }
        }
    }
}
