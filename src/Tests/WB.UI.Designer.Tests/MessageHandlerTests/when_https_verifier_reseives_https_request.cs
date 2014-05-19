using System.Net;
using System.Net.Http;
using System.Threading;
using Machine.Specifications;
using NUnit.Framework;
using WB.UI.Designer.Code.MessageHandlers;

namespace WB.UI.Designer.Tests.MessageHandlerTests
{
    public class when_https_verifier_reseives_https_request : MessageHandlerTestContext
    {
        Establish context = () =>
        {
            FakeInnerHandler innerhandler = CreateFakeInnerHandler();
            innerhandler.Message = new HttpResponseMessage(HttpStatusCode.OK);
            client = new HttpMessageInvoker(CreateHttpsVerifier(innerhandler));
            requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost");
        };

        Because of = () =>
            message = client.SendAsync(requestMessage, new CancellationToken(false)).Result;

        private It should_return_OK_status_code = () =>
            message.StatusCode.ShouldEqual(HttpStatusCode.OK);

        private static HttpResponseMessage message;
        private static HttpMessageInvoker client;
        private static HttpRequestMessage requestMessage;
    }


}
