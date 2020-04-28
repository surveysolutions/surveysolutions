using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
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
    public static class AuthExtensions
    {
        public static void AddHqAuthorization(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddIdentity<HqUser, HqRole>()
               .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
               .AddUserStore<HqUserStore>()
               .AddRoleStore<HqRoleStore>()
               .AddDefaultTokenProviders()
               .AddSignInManager<HqSignInManager>();

            services.AddAuthorization();

            services.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = "/Account/LogOn";
                opt.AccessDeniedPath = "/Error/401";
                opt.ExpireTimeSpan = TimeSpan.FromDays(1);

                opt.ForwardDefaultSelector = ctx =>
                {
                    if (ctx.Request.Headers.ContainsKey(HeaderNames.Authorization))
                    {
                        AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(ctx.Request.Headers[HeaderNames.Authorization]);
                        return authHeader.Scheme;
                    }

                    return null;
                };

                opt.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                        else
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        {
                            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        }
                        else
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });


            services
                .AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddScheme<BasicAuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", opts =>
                {
                    opts.Realm = "WB.Headquarters";
                })
                .AddScheme<AuthTokenAuthenticationSchemeOptions, AuthTokenAuthenticationHandler>("AuthToken", _ => { })
                .AddScheme<AuthenticationSchemeOptions, TenantTokenAuthenticationHandler>("TenantToken", _ => { });

            //adding AzureAD
            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => Configuration.Bind("AzureAd", options));
            
            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                options.Authority = options.Authority + "/v2.0/";

                // Per the code below, this application signs in users in any Work and School
                // accounts and any Microsoft Personal Accounts.
                // If you want to direct Azure AD to restrict the users that can sign-in, change 
                // the tenant value of the appsettings.json file in the following way:
                // - only Work and School accounts => 'organizations'
                // - only Microsoft Personal accounts => 'consumers'
                // - Work and School and Personal accounts => 'common'

                // If you want to restrict the users that can sign-in to only one tenant
                // set the tenant value in the appsettings.json file to the tenant ID of this
                // organization, and set ValidateIssuer below to true.
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateAudience = true;
                options.TokenValidationParameters.ValidateLifetime = true;

                // If you want to restrict the users that can sign-in to several organizations
                // Set the tenant value in the appsettings.json file to 'organizations', set
                // ValidateIssuer, above to 'true', and add the issuers you want to accept to the
                // options.TokenValidationParameters.ValidIssuers collection

                options.SignInScheme = IdentityConstants.ExternalScheme;
            });

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
