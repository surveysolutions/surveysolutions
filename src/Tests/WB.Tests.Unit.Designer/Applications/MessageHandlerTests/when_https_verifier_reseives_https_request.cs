using System.Net;
using System.Net.Http;
using System.Threading;
using Machine.Specifications;

namespace WB.Tests.Unit.Designer.Applications.MessageHandlerTests
{
    internal class when_https_verifier_reseives_https_request : MessageHandlerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            FakeInnerHandler innerhandler = CreateFakeInnerHandler();
            innerhandler.Message = new HttpResponseMessage(HttpStatusCode.OK);
            client = new HttpMessageInvoker(CreateHttpsVerifier(innerhandler));
            requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost");
            BecauseOf();
        }

        private void BecauseOf() =>
            message = client.SendAsync(requestMessage, new CancellationToken(false)).Result;

        [NUnit.Framework.Test] public void should_return_OK_status_code () =>
            message.StatusCode.ShouldEqual(HttpStatusCode.OK);

        private static HttpResponseMessage message;
        private static HttpMessageInvoker client;
        private static HttpRequestMessage requestMessage;
    }


}
