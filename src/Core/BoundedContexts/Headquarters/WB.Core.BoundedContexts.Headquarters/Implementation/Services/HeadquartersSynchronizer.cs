using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Commands.User;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
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

        public void Pull(string login, string password)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, this.GetFeedUrl());

                string base64Credential = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", login, password)));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credential);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.SendAsync(request).Result;

                if (!response.IsSuccessStatusCode)
                    return;

                string responseBody = response.Content.ReadAsStringAsync().Result;

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
            if (string.IsNullOrWhiteSpace(responseBody))
                throw new ArgumentException("Response body is empty.");

            dynamic responseFeed = JObject.Parse(responseBody);

            if (responseFeed == null)
                throw new ArgumentException("Response body cannot be parsed to valid feed - result feed is null.");

            string login = responseFeed.Login;
            string passwordHash = responseFeed.PasswordHash;

            return new CreateUserCommand(
                Guid.NewGuid(), login, passwordHash, string.Empty, new[] { UserRoles.Supervisor }, false, null);
        }
    }
}