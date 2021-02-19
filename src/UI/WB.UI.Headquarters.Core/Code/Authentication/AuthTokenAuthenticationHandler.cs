using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Shared.Web.Authentication;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class AuthTokenAuthenticationHandler : AuthenticationHandler<AuthTokenAuthenticationSchemeOptions>
    {
        private readonly IUserRepository userRepository;
        private readonly IUserClaimsPrincipalFactory<HqUser> claimFactory;
        private readonly IApiTokenProvider authTokenProvider;
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;
        private bool isUserLocked;
        private bool forceChangePassword;

        public AuthTokenAuthenticationHandler(IOptionsMonitor<AuthTokenAuthenticationSchemeOptions> options, 
            ILoggerFactory logger,
            UrlEncoder encoder, 
            ISystemClock clock,
            IUserRepository userRepository,
            IUserClaimsPrincipalFactory<HqUser> claimFactory,
            IApiTokenProvider authTokenProvider, 
            IWorkspaceContextAccessor workspaceContextAccessor) : base(options, logger, encoder, clock)
        {
            this.userRepository = userRepository;
            this.claimFactory = claimFactory;
            this.authTokenProvider = authTokenProvider;
            this.workspaceContextAccessor = workspaceContextAccessor;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();
            BasicCredentials creds;
            try
            {
                creds = Request.Headers.ParseBasicCredentials();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Information, e, "failed to authorize");
                return AuthenticateResult.NoResult();
            }

            var user = await userRepository.FindByNameAsync(creds.Username);
            
            if(user == null) return AuthenticateResult.Fail("No user found");
            
            if (user.IsArchivedOrLocked)
            {
                this.isUserLocked = true;
                return AuthenticateResult.Fail("User is locked");
            }
            
            if (user.ForceChangePassword)
            {
                var changePasswordApi = Request.Path.HasValue 
                                        && Request.Path.Value != null
                                        && Request.Path.Value.EndsWith("/users/changePassword");
                if (!changePasswordApi)
                {
                    this.forceChangePassword = true;
                    return AuthenticateResult.Fail("User must change password");
                }
            }

            var verificationResult = await authTokenProvider.ValidateTokenAsync(user.Id, creds.Password);
            if (verificationResult)
            {
                var claimsPrincipal = await this.claimFactory.CreateAsync(user);
                return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
            }

            return AuthenticateResult.Fail("Invalid auth token");
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            await base.HandleForbiddenAsync(properties);
            var currentWorkspace = this.workspaceContextAccessor.CurrentWorkspace();
            if (currentWorkspace?.DisabledAtUtc != null)
            {
                await using StreamWriter bodyWriter = new StreamWriter(Response.Body);
                await bodyWriter.WriteAsync(JsonConvert.SerializeObject(new {Message = "Workspace is disabled"}));
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            await base.HandleChallengeAsync(properties);
            if (this.isUserLocked)
            {
                await using StreamWriter bodyWriter = new StreamWriter(Response.Body);
                await bodyWriter.WriteAsync(JsonConvert.SerializeObject(new {Message = "User is locked"}));
            }
            if (this.forceChangePassword)
            {
                await using StreamWriter bodyWriter = new StreamWriter(Response.Body);
                var serverError = new ServerError()
                {
                    Code = ServerErrorCodes.ForceChangePassword,
                    Message = "Your must change your password for access to server"
                };
                await bodyWriter.WriteAsync(JsonConvert.SerializeObject(serverError));
                //await bodyWriter.WriteAsync(JsonConvert.SerializeObject(new {Message = "Force change password"}));
            }
        }
    }
}
