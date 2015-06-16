using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Extensions;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.BoundedContexts.Supervisor.Users.Implementation
{
    internal class HeadquartersLoginService : IHeadquartersLoginService
    {
        private readonly ILogger logger;
        private readonly Action<ICommand> executeCommand;
        private readonly Func<HttpMessageHandler> messageHandler;
        private readonly IHeadquartersSettings headquartersSettings;
        private readonly IHeadquartersUserReader headquartersUserReader;
        private readonly IPasswordHasher passwordHasher;

        public HeadquartersLoginService(ILogger logger, 
            ICommandService commandService,
            Func<HttpMessageHandler> messageHandler,
            IHeadquartersSettings headquartersSettings,
            IHeadquartersUserReader headquartersUserReader,
            IPasswordHasher passwordHasher)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (headquartersSettings == null) throw new ArgumentNullException("headquartersSettings");
            if(passwordHasher == null) throw new ArgumentNullException("passwordHasher");

            this.logger = logger;
            this.executeCommand = command => commandService.Execute(command, origin: Constants.HeadquartersSynchronizationOrigin);
            this.messageHandler = messageHandler;
            this.headquartersSettings = headquartersSettings;
            this.headquartersUserReader = headquartersUserReader;
            this.passwordHasher = passwordHasher;
        }

        public async Task LoginAndCreateAccount(string login, string password)
        {
            try
            {
                using (var client = new HttpClient(this.messageHandler()))
                {
                    client.AppendAuthToken(this.headquartersSettings);
                
                    var requestUri = this.BuildValidationUri(login, password);
                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.CacheControl = new CacheControlHeaderValue {
                        NoCache = true
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        this.logger.Error(string.Format("Failed to login user {0} response code: {1}, Endpoint: {3}. Response content: {2}. ", login, response.StatusCode, errorContent, requestUri));
                        return;
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();
                    var validationResult = JsonConvert.DeserializeObject<SupervisorValidationResult>(responseBody);

                    if (validationResult.isValid)
                    {
                        string userDetailsUrl = validationResult.userDetailsUrl;
                        UserView userDocument = await headquartersUserReader.GetUserByUri(new Uri(userDetailsUrl));

                        var command = new CreateUserCommand(userDocument.PublicKey,
                            userDocument.UserName,
                            userDocument.Password,
                            userDocument.Email,
                            new[] { UserRoles.Supervisor },
                            userDocument.IsLockedBySupervisor,
                            userDocument.IsLockedByHQ,
                            null,
                            userDocument.PersonName,
                            userDocument.PhoneNumber);

                        this.executeCommand(command);
                    }
                    else
                    {
                        this.logger.Warn(string.Format("Failed to login user {0}, endpoint used: {1}.", login, requestUri));
                    }
                }
            }
            catch (HttpRequestException e)
            {
                this.logger.Warn(string.Format("Failed to login user {0}", login), e);
            }
        }

        private Uri BuildValidationUri(string login, string password)
        {
            var passwordHash = passwordHasher.Hash(password);
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