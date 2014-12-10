using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization
{
    public static class MockHelpers
    {
        public static void SetupResponseFromResource(this Mock<HttpMessageHandler> handler, string requestUrl, string resourceName)
        {
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ResourceHelper.ReadResourceFile(resourceName))
                }));
        }
    }
}