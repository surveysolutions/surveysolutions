﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Shared.Web.Authentication;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationSchemeOptions>
    {
        private readonly UserManager<HqUser> userManager;

        public BasicAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationSchemeOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            ISystemClock clock,
            UserManager<HqUser> userManager) : base(options, logger, encoder, clock)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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
                return AuthenticateResult.Fail(e.Message);
            }

            var user = await this.userManager.FindByNameAsync(creds.Username);
            if(user == null) return AuthenticateResult.Fail("No user found");

            var passwordIsValid = await this.userManager.CheckPasswordAsync(user, creds.Password);
            if(!passwordIsValid) return AuthenticateResult.Fail("Invalid password");

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.FormatGuid()),
                new Claim(ClaimTypes.Name, user.UserName),
            };
            foreach (var userRole in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Name));
            }
            
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            HandleFail();
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            HandleFail();
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        private void HandleFail()
        {
            Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Options.Realm}\", charset=\"UTF-8\"";
        }
    }
}
