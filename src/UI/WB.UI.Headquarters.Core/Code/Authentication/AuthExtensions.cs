using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Shared.Web.Authentication;

namespace WB.UI.Headquarters.Code.Authentication
{
    public static class AuthType
    {
        public const string TenantToken = "TenantToken";
    }

    public static class AuthExtensions
    {
        public static void AddHqAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<HqUser, HqRole>()
                .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
                .AddUserStore<HqUserStore>()
                .AddRoleStore<HqRoleStore>()
                .AddDefaultTokenProviders()
                .AddClaimsPrincipalFactory<HqUserClaimsPrincipalFactory>()
                .AddUserManager<HqUserManager>()
                .AddSignInManager<HqSignInManager>();

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = "/Account/LogOn";
                opt.AccessDeniedPath = "/Error/401";

                var expireTimeSpan = configuration.GetValue<TimeSpan>("Authentication:TimeOut");
                opt.ExpireTimeSpan = expireTimeSpan;
                opt.Cookie.Path = "/";

                var securityPolicy = configuration.GetValue<Boolean?>("Policies:CookiesSecurePolicyAlways");
                
                if (securityPolicy.HasValue)
                    opt.Cookie.SecurePolicy = securityPolicy.Value ? CookieSecurePolicy.Always : CookieSecurePolicy.None;


                opt.ForwardDefaultSelector = ctx =>
                {
                    if (ctx.Request.Headers.ContainsKey(HeaderNames.Authorization))
                    {
                        AuthenticationHeaderValue authHeader =
                            AuthenticationHeaderValue.Parse(ctx.Request.Headers[HeaderNames.Authorization]);
                        return authHeader.Scheme;
                    }

                    return null;
                };

                opt.EventsType = typeof(HqCookieAuthenticationEvents);
            });

            services.AddTransient<HqCookieAuthenticationEvents>();

            services
                .AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddScheme<BasicAuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", opts =>
                {
                    opts.Realm = "WB.Headquarters";
                })
                .AddScheme<AuthTokenAuthenticationSchemeOptions, AuthTokenAuthenticationHandler>("AuthToken", _ => { })
                .AddScheme<AuthenticationSchemeOptions, TenantTokenAuthenticationHandler>(AuthType.TenantToken, _ => { });

            services.Configure<IdentityOptions>(options =>
            {
                    // Default Password settings.
                    options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 1;
            });
        }
    }
}
