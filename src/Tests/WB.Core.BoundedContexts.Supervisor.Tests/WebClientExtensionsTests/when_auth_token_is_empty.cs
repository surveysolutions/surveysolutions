using System.Net.Http;
using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.Extensions;

namespace WB.Core.BoundedContexts.Supervisor.Tests.WebClientExtensionsTests
{
    [Subject(typeof(WebClientExtensions), "AppendAuthToken")]
    public class when_auth_token_is_empty
    {
        Establish context = () =>
        {
            headquartersSettings = Create.HeadquartersSettings(accessToken: null);
            client = new HttpClient();
        };

        Because of = () => client.AppendAuthToken(headquartersSettings);

        It should_not_change_auth_headers_of_client = () => client.DefaultRequestHeaders.Authorization.ShouldBeNull();

        static IHeadquartersSettings headquartersSettings;
        static HttpClient client;
    }
}