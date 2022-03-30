using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Ncqrs.Domain.Storage;
using Newtonsoft.Json.Serialization;
using reCAPTCHA.AspNetCore;
using VueCliMiddleware;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Versions;
using WB.Infrastructure.Native.Files;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Attributes;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Code.ImportExport;
using WB.UI.Designer.Code.Vue;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Filters;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Models;
using WB.UI.Designer.Modules;
using WB.UI.Designer.Services;
using WB.UI.Designer.Services.Restore;
using WB.UI.Shared.Web.Authentication;
using WB.UI.Shared.Web.Diagnostics;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.Services;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace WB.UI.Designer
{
    public class Startup
    {
        private const string SpaRoot = "questionnaire-app";
        internal const string WebTesterCorsPolicy = "_webTester";
        private readonly IWebHostEnvironment hostingEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private AspCoreKernel? aspCoreKernel;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "Designer";
            });

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            services.AddDbContext<DesignerDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection")));
            
            services.AddScoped<AntiForgeryFilter>();
            services.AddScoped<IPasswordHasher<DesignerIdentityUser>, PasswordHasher>();
            services
                .AddDefaultIdentity<DesignerIdentityUser>()
                .AddRoles<DesignerIdentityRole>()
                .AddEntityFrameworkStores<DesignerDbContext>();

            services.AddHealthChecks()
                .AddCheck<DatabaseConnectionCheck>("database");

            services
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = "boc";
                    sharedOptions.DefaultChallengeScheme = "boc";
                })
                .AddPolicyScheme("boc", "Basic or cookie", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        if (context.Request.Headers.ContainsKey("Authorization"))
                        {
                            return "basic";
                        }

                        return IdentityConstants.ApplicationScheme;
                    };
                })
                .AddScheme<BasicAuthenticationSchemeOptions, BasicAuthenticationHandler>("basic",
                    opts => { opts.Realm = "mysurvey.solutions"; });

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    var hasAcceptHeader = context.Request.Headers.TryGetValue(HeaderNames.Accept, out var accept);
                    if (hasAcceptHeader && accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase) && context.Response.StatusCode == StatusCodes.Status200OK)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }
                    else
                    {
                        context.Response.Redirect(context.RedirectUri);
                    }

                    return Task.CompletedTask;
                };
            });

            services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
            
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddMvc()
                //.SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                })
                .AddMvcOptions(options => options.ModelBinderProviders.Insert(0, new QuestionnaireRevisionBinderProvider()));

            services.AddCors(corsOpt =>
            {
                corsOpt.AddPolicy(WebTesterCorsPolicy, b =>
                {
                    var st = Configuration.GetSection("WebTester").GetValue<string>("BaseUri");
                    Uri uri = new Uri(st);
                    var webTesterOrigin = uri.Scheme + Uri.SchemeDelimiter + uri.Host;
                    if (Regex.IsMatch(st, ":\\d+"))
                    {
                        webTesterOrigin += ":" + uri.Port;
                    }

                    b.WithOrigins(webTesterOrigin).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });

            services.AddDatabaseStoredExceptional(hostingEnvironment, Configuration);

            services.AddTransient<IQuestionnaireRestoreService, QuestionnaireRestoreService>();
            services.AddTransient<IQuestionnaireImportService, QuestionnaireImportService>();
            services.AddTransient<IQuestionnaireExportService, QuestionnaireExportService>();
            services.AddTransient<ICaptchaService, WebCacheBasedCaptchaService>();
            services.AddTransient<ICaptchaProtectedAuthenticationService, CaptchaProtectedAuthenticationService>();
            services.AddSingleton<IProductVersion, ProductVersion>();
            services.AddTransient<IProductVersionHistory, ProductVersionHistory>();
            services.AddTransient<IBasicAuthenticationService, BasicBasicAuthenticationService>();

            services.Configure<CaptchaConfig>(Configuration.GetSection("Captcha"));
            services.Configure<RecaptchaSettings>(Configuration.GetSection("Captcha"));
            services.AddTransient<IRecaptchaService, RecaptchaService>();
            services.AddTransient<IRecipientNotifier, MailNotifier>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.Configure<UiConfig>(Configuration.GetSection("UI"));
            services.Configure<IntegrationsConfig>(Configuration.GetSection("Integrations"));

            var membershipSection = this.Configuration.GetSection("Membership");
            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequireDigit = membershipSection.GetValue("RequireDigit", true);
                opt.Password.RequireLowercase = membershipSection.GetValue("RequireLowercase", true);
                opt.Password.RequireUppercase = membershipSection.GetValue("RequireUppercase", true);
                opt.Password.RequireNonAlphanumeric = membershipSection.GetValue("RequireNonAlphanumeric", false);
                opt.Password.RequiredLength = membershipSection.GetValue("RequiredLength", 6);
                opt.User.RequireUniqueEmail = membershipSection.GetValue("RequireUniqueEmail", true);
            });

            services.Configure<MailSettings>(Configuration.GetSection("Mail"));
            services.AddTransient<IEmailSender, MailSender>();
            services.AddTransient<IViewRenderService, ViewRenderService>();
            services.AddTransient<IQuestionnaireHelper, QuestionnaireHelper>();
            services.AddTransient<IDomainRepository, DomainRepository>();
            services.AddScoped<ILoggedInUser, LoggedInUser>();

            //services.AddHostedService<FontInstalledCheck>(); // Fails in docker

            services.Configure<CompilerSettings>(Configuration.GetSection("CompilerSettings"));
            services.Configure<PdfSettings>(Configuration.GetSection("Pdf"));
            services.Configure<QuestionnaireHistorySettings>(Configuration.GetSection("QuestionnaireHistorySettings"));
            services.Configure<WebTesterSettings>(Configuration.GetSection("WebTester"));

            // In production, the Vue files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = $"{SpaRoot}/dist";
            });

            aspCoreKernel = new AspCoreKernel(services);

            aspCoreKernel.Load(
                new EventFreeInfrastructureModule(),
                new InfrastructureModule(),
                new DesignerBoundedContextModule(),
                new QuestionnaireVerificationModule(),
                new FileInfrastructureModule(),
                new DesignerRegistry(),
                new DesignerWebModule(),
                new WebCommonModule()
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseExceptional();

            if (!env.IsDevelopment())
            {
                app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"),
                    appBuilder =>
                    {
                        appBuilder.UseStatusCodePagesWithReExecute("/error/{0}");
                        appBuilder.UseExceptionHandler("/error/500");
                    });
            }

            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseResponseCompression();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (!env.IsDevelopment())
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
                    }
                }
            });
            app.UseSpaStaticFiles();
            
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();
            app.UseCors(WebTesterCorsPolicy);

            app.UseRequestLocalization(opt =>
            {
                opt.DefaultRequestCulture = new RequestCulture("en-US");
                opt.SupportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en")
                };
                opt.SupportedUICultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("ru"),
                    new CultureInfo("fr"),
                    new CultureInfo("es"),
                    new CultureInfo("ar"),
                    new CultureInfo("zh"),
                    new CultureInfo("sq"),
                    new CultureInfo("ro")
                };
            });

            app.UseHealthChecks("/.hc");

            app.UseRouting();
            app.UseAuthorization();
            
            app.Use((context, next) =>
            {
                var endpoint = context.GetEndpoint();
                if (endpoint != null && endpoint.Metadata.All(f => f.GetType() != typeof(VuePageAttribute)))
                {
                    return next();
                }

                // if it is client vue page, set null to transfer control to SPA middleware
                context.SetEndpoint(null);
                return next();
            });
            
            app.UseEndpoints(routes =>
            {
                routes.MapVersionEndpoint();

                routes.MapControllerRoute(
                    name: "areaRoute",
                    pattern: "{area:exists}/{controller}/{action}/{id?}",
                    defaults: new { action = "Index" });

                routes.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Questionnaire}/{action=Index}/{id?}");
                routes.MapRazorPages();

                if (env.IsDevelopment())
                {
                    routes.MapToVueCliProxy("{*path}", new SpaOptions
                        {
                            SourcePath = SpaRoot
                        },
                        port: 0,
                        npmScript: "serve", //(System.Diagnostics.Debugger.IsAttached) ? "serve" : null,
                        regex: "Compiled successfully",
                        forceKill: true
                    );
                }
            });
            
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = SpaRoot;
            });

            if (aspCoreKernel == null) return;

            var initTask = aspCoreKernel.InitAsync(serviceProvider);
            if (env.IsDevelopment())
                initTask.Wait();
            else
                initTask.Wait(TimeSpan.FromSeconds(10));
        }
    }
}
