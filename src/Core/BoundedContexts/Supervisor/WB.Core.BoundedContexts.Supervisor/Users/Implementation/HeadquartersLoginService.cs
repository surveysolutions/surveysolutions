using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Users.Implementation
{
    internal class HeadquartersLoginService : IHeadquartersLoginService
    {
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly HttpMessageHandler messageHandler;
        private readonly HeadquartersSettings headquartersSettings;
        private readonly IHeadquartersUserReader headquartersUserReader;

        public HeadquartersLoginService(ILogger logger, 
            ICommandService commandService,
            HttpMessageHandler messageHandler,
            HeadquartersSettings headquartersSettings,
            IHeadquartersUserReader headquartersUserReader)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (headquartersSettings == null) throw new ArgumentNullException("headquartersSettings");

            this.logger = logger;
            this.commandService = commandService;
            this.messageHandler = messageHandler;
            this.headquartersSettings = headquartersSettings;
            this.headquartersUserReader = headquartersUserReader;
        }

        public void LoginAndCreateAccount(string login, string password)
        {
            using (var client = new HttpClient(messageHandler))
            {
                var requestUri = this.BuildValidationUri(login, password);
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.SendAsync(request).Result;

                if (!response.IsSuccessStatusCode)
                {
                    this.logger.Error(string.Format("Failed to login user {0} response code: {1}, response content: {2}", login, response.StatusCode, response.Content));
                    return;
                }

                string responseBody = response.Content.ReadAsStringAsync().Result;
                var validationResult = JsonConvert.DeserializeObject<SupervisorValidationResult>(responseBody);

                if (validationResult.isValid)
                {
                    string userDetailsUrl = validationResult.userDetailsUrl;
                    UserDocument userDocument = headquartersUserReader.GetUserByUri(new Uri(userDetailsUrl)).Result;

                    var command = new CreateUserCommand(userDocument.PublicKey, 
                        userDocument.UserName, 
                        userDocument.Password, 
                        userDocument.Email, 
                        new[] { UserRoles.Supervisor },
                        userDocument.IsLockedBySupervisor, 
                        userDocument.IsLockedByHQ, 
                        null);

                    this.commandService.Execute(command);
                }
            }
        }

        private Uri BuildValidationUri(string login, string password)
        {
            var passwordHash = SimpleHash.ComputeHash(password);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["login"] = login;
            query["passwordHash"] = passwordHash;
            string queryString = query.ToString();

            var loginServiceEndpointUrl = this.headquartersSettings.LoginServiceEndpointUrl;
            var uri = new UriBuilder(loginServiceEndpointUrl) { Query = queryString };

            return uri.Uri;
        }
    }
}