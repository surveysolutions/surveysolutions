using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Shared.Web.Authentication;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class AuthTokenAuthenticationHandler : AuthenticationHandler<AuthTokenAuthenticationSchemeOptions>
    {
        private readonly IUserRepository userRepository;
        private readonly IUserClaimsPrincipalFactory<HqUser> claimFactory;
        private readonly IApiTokenProvider authTokenProvider;
        private bool isUserLocked;

        public AuthTokenAuthenticationHandler(IOptionsMonitor<AuthTokenAuthenticationSchemeOptions> options, 
            ILoggerFactory logger,
            UrlEncoder encoder, 
            ISystemClock clock,
            IUserRepository userRepository,
            IUserClaimsPrincipalFactory<HqUser> claimFactory,
            IApiTokenProvider authTokenProvider) : base(options, logger, encoder, clock)
        {
            this.userRepository = userRepository;
            this.claimFactory = claimFactory;
            this.authTokenProvider = authTokenProvider;
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

            var verificationResult = await authTokenProvider.ValidateTokenAsync(user.Id, creds.Password);
            if (verificationResult)
            {
                var claimsPrincipal = await this.claimFactory.CreateAsync(user);
                return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
            }

            return AuthenticateResult.Fail("Invalid auth token");
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            await base.HandleChallengeAsync(properties);
            if (this.isUserLocked)
            {
                await using StreamWriter bodyWriter = new StreamWriter(Response.Body);
                await bodyWriter.WriteAsync(JsonConvert.SerializeObject(new {Message = "User is locked"}));
            }
        }
    }
}
