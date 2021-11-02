using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Shared.Web.Authentication;

namespace WB.UI.Headquarters.Code.Authentication
{
    public static class AuthType
    {
        public const string TenantToken = "TenantToken";
        public const string AuthToken = "AuthToken";
        public const string ApiKey = "ApiKey";
        public const string Basic = "Basic";
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

                var expireTimeSpan = configuration.GetValue<TimeSpan>("Authentication:TicketExpirationTimeOut");                
                opt.ExpireTimeSpan = expireTimeSpan == TimeSpan.Zero? TimeSpan.FromDays(1) : expireTimeSpan;
                opt.Cookie.Path = "/";

                var securityPolicy = configuration.GetValue<Boolean?>("Policies:CookiesSecurePolicyAlways");

                if (securityPolicy.HasValue)
                    opt.Cookie.SecurePolicy =
                        securityPolicy.Value ? CookieSecurePolicy.Always : CookieSecurePolicy.None;
                else
                    opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

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
            
            var builder = services.AddAuthentication(IdentityConstants.ApplicationScheme);
            
            builder.AddScheme<BasicAuthenticationSchemeOptions, BasicAuthenticationHandler>(AuthType.Basic, opts =>
                {
                    opts.Realm = "WB.Headquarters";
                })
                .AddScheme<AuthTokenAuthenticationSchemeOptions, AuthTokenAuthenticationHandler>(AuthType.AuthToken, _ => { })
                .AddScheme<AuthenticationSchemeOptions, TenantTokenAuthenticationHandler>(AuthType.TenantToken, _ => { });

            var isJwtBearerEnabled = configuration.GetValue<bool>("JwtBearer:Enabled");
            
            services.Configure<TokenProviderOptions>(options =>
            {
                options.IsBearerEnabled = isJwtBearerEnabled;
                options.Audience = configuration.GetValue<string>("JwtBearer:Audience");
                options.Issuer = configuration.GetValue<string>("JwtBearer:Issuer");
                options.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(configuration.GetValue<string>("JwtBearer:SecretKey"))),
                    SecurityAlgorithms.HmacSha256);
            });


            if (isJwtBearerEnabled)
            {
                var signingKey =
                    new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(configuration.GetValue<string>("JwtBearer:SecretKey")));
                
                builder.AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = signingKey,
                            ValidateIssuer = true,
                            ValidIssuer = configuration.GetValue<string>("JwtBearer:Issuer"),
                            ValidateAudience = true,
                            ValidAudience = configuration.GetValue<string>("JwtBearer:Audience"),
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                            
                        };
                    options.EventsType = typeof(HqJwtAuthenticationEvents);
                });
                
                services.AddTransient<HqJwtAuthenticationEvents>();
            }

            var passwordOptions = configuration.GetSection("PasswordOptions").Get<PasswordOptions>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = passwordOptions.RequireDigit;
                options.Password.RequireLowercase = passwordOptions.RequireLowercase;
                options.Password.RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric;
                options.Password.RequireUppercase = passwordOptions.RequireUppercase;
                options.Password.RequiredLength = passwordOptions.RequiredLength;
                options.Password.RequiredUniqueChars = passwordOptions.RequiredUniqueChars;

                options.Lockout.MaxFailedAccessAttempts =
                    configuration.GetValue<int>("Authentication:MaxFailedAccessAttemptsBeforeLockout");
                options.Lockout.DefaultLockoutTimeSpan =
                    configuration.GetValue<TimeSpan>("Authentication:LockoutDuration");
            });
        }
    }
}
