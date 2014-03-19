using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json.Linq;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.User;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services
{
    internal class HeadquartersSynchronizer : IHeadquartersSynchronizer
    {
        private readonly ICommandService commandService;

        public HeadquartersSynchronizer(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        public async void Pull(string login, string password)
        {
            using (var handler = new HttpClientHandler { Credentials = new NetworkCredential(login, password) })
            using (var client = new HttpClient(handler))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Headquarters/api/feed/v0");

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.SendAsync(request);

                string responseBody = await response.Content.ReadAsStringAsync();

                dynamic responseFeed = JObject.Parse(responseBody);

                this.commandService.Execute(
                    new CreateUserCommand(Guid.NewGuid(), responseFeed.Login, responseFeed.PasswordHash, string.Empty, new[] { UserRoles.Supervisor }, false, null));
            }
        }
    }
}