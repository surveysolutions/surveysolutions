using System;
using System.IO;
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
        private readonly HeadquartersSettings headquartersSettings;

        public HeadquartersSynchronizer(ICommandService commandService, HeadquartersSettings headquartersSettings)
        {
            this.commandService = commandService;
            this.headquartersSettings = headquartersSettings;
        }

        public async void Pull(string login, string password)
        {
            using (var handler = new HttpClientHandler { Credentials = new NetworkCredential(login, password) })
            using (var client = new HttpClient(handler))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, this.GetFeedUrl());

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return;

                string responseBody = await response.Content.ReadAsStringAsync();

                var command = CreateCommandFromResponseBody(responseBody);

                this.commandService.Execute(command);
            }
        }

        private Uri GetFeedUrl()
        {
            return new Uri(this.headquartersSettings.Url.TrimEnd('/') + "/api/feed/v0");
        }

        private static CreateUserCommand CreateCommandFromResponseBody(string responseBody)
        {
            dynamic responseFeed = JObject.Parse(responseBody);

            string login = responseFeed.Login;
            string passwordHash = responseFeed.PasswordHash;

            return new CreateUserCommand(
                Guid.NewGuid(), login, passwordHash, string.Empty, new[] { UserRoles.Supervisor }, false, null);
        }
    }
}