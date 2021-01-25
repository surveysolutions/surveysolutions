using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using WB.Core.Infrastructure.Domain;

namespace WB.Tests.Unit
{
    internal static class MockExtensions
    {
        public static void SetupResponseFromResource(this Mock<HttpMessageHandler> handlerMock, string requestUrl, string resourceName)
        {
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ResourceHelper.ReadResourceFile(resourceName))
                }));
        }

        public static void SetupResponseFromResource(this HttpMessageHandler handler, string requestUrl, string resourceName)
        {
            Mock.Get(handler).SetupResponseFromResource(requestUrl, resourceName);
        }
    }
}
