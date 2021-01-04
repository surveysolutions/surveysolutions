﻿using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        public static void AddHqAuthorization(this IServiceCollection services)
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
                    .AddRequirements(new WorkspaceRequirement())
                    .Build();
            });

            services.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = "/Account/LogOn";
                opt.AccessDeniedPath = "/Error/401";
                opt.ExpireTimeSpan = TimeSpan.FromDays(1);
                opt.Cookie.Path = "/";

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
