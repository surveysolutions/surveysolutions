﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View.User;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Extensions;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Users.Implementation
{
    internal class HeadquartersLoginService : IHeadquartersLoginService
    {
        private readonly ILogger logger;
        private readonly Action<ICommand> executeCommand;
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
            this.executeCommand = command => commandService.Execute(command, origin: Constants.HeadquartersSynchronizationOrigin);
            this.messageHandler = messageHandler;
            this.headquartersSettings = headquartersSettings;
            this.headquartersUserReader = headquartersUserReader;
        }

        public void LoginAndCreateAccount(string login, string password)
        {
            using (var client = new HttpClient(messageHandler))
            {
                client.AppendAuthToken(this.headquartersSettings);
                
                var requestUri = this.BuildValidationUri(login, password);
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.CacheControl = new CacheControlHeaderValue {
                    NoCache = true
                };

                HttpResponseMessage response = client.SendAsync(request).Result;

                if (!response.IsSuccessStatusCode)
                {
                    this.logger.Error(string.Format("Failed to login user {0} response code: {1}, Endpoint: {3}. Response content: {2}. ", login, response.StatusCode, response.Content, requestUri));
                    return;
                }

                string responseBody = response.Content.ReadAsStringAsync().Result;
                var validationResult = JsonConvert.DeserializeObject<SupervisorValidationResult>(responseBody);

                if (validationResult.isValid)
                {
                    string userDetailsUrl = validationResult.userDetailsUrl;
                    UserView userDocument = headquartersUserReader.GetUserByUri(new Uri(userDetailsUrl)).Result;

                    var command = new CreateUserCommand(userDocument.PublicKey,
                        userDocument.UserName,
                        userDocument.Password,
                        userDocument.Email,
                        new[] { UserRoles.Supervisor },
                        userDocument.isLockedBySupervisor,
                        userDocument.IsLockedByHQ,
                        null);

                    this.executeCommand(command);
                }
                else
                {
                    this.logger.Warn(string.Format("Failed to login user {0}, endpoint used: {1}.", login, requestUri));
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