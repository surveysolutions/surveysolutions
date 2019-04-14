﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Serialization;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.Versions;
using WB.Infrastructure.Native.Files;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.DependencyInjection;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Models;
using WB.UI.Designer.Modules;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Versions;

namespace WB.UI.Designer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private AspCoreKernel aspCoreKernel;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
            });

            services.AddDbContext<DesignerDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IPasswordHasher<DesignerIdentityUser>, PasswordHasher>();
            services.AddDefaultIdentity<DesignerIdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<DesignerDbContext>();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddWebApiConventions()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            services.AddTransient<ICaptchaService, WebCacheBasedCaptchaService>();
            services.AddTransient<ICaptchaProtectedAuthenticationService, CaptchaProtectedAuthenticationService>();
            services.AddSingleton<IProductVersion, ProductVersion>();

            services.Configure<CaptchaConfig>(Configuration.GetSection("Captcha"));
            services.AddTransient<IRecaptchaService, RecaptchaService>();
            services.AddTransient<IRecipientNotifier, MailNotifier>();
            services.Configure<UiConfig>(Configuration.GetSection("UI"));

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
            services.AddTransient<IViewRenderingService, ViewRenderingService>();
            services.AddTransient<IQuestionnaireHelper, QuestionnaireHelper>();
            services.AddScoped<ILoggedInUser, LoggedInUser>();

            services.Configure<CompilerSettings>(Configuration.GetSection("CompilerSettings"));
            services.Configure<PdfSettings>(Configuration.GetSection("Pdf"));
            services.Configure<DeskSettings>(Configuration.GetSection("Desk"));
            services.Configure<QuestionnaireHistorySettings>(Configuration.GetSection("QuestionnaireHistorySettings"));
            services.Configure<WebTesterSettings>(Configuration.GetSection("WebTester"));

            aspCoreKernel = new AspCoreKernel(services);

            aspCoreKernel.Load(
                new EventFreeInfrastructureModule(),
                new InfrastructureModule(),
                new NcqrsModule(),
                //new WebConfigurationModule(membershipSettings),
                //new CaptchaModule(settingsProvider.AppSettings.Get("CaptchaService")),
                //new NLogLoggingModule(),
                //new OrmModule(ormSettings),
                //new DesignerCommandDeserializationModule(),
                new DesignerBoundedContextModule(),
                new QuestionnaireVerificationModule(),
                //new MembershipModule(),
                new FileInfrastructureModule(),
                //new ProductVersionModule(typeof(Startup).Assembly),
                new DesignerRegistry(),
                new DesignerWebModule(),
                new WebCommonModule()
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseResponseCompression();
            app.UseStaticFiles();
            
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Areas", "Pdf", "Content")),
                RequestPath = new PathString("/pdf/content")
            });

            app.UseRequestLocalization(opt =>
            {
                opt.DefaultRequestCulture = new RequestCulture("en-US");
                opt.SupportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("ru"),
                    new CultureInfo("es"),
                    new CultureInfo("ar"),
                    new CultureInfo("zh")
                };
                opt.SupportedUICultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("ru"),
                    new CultureInfo("es"),
                    new CultureInfo("ar"),
                    new CultureInfo("zh")
                };
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areas",
                    template: "{area:exists}/{controller=Questionnaire}/{action=My}/{id?}"
                );

                routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Questionnaire}/{action=My}/{id?}");
            });

            var initTask = aspCoreKernel.InitAsync(serviceProvider);
            if (env.IsDevelopment())
                initTask.Wait();
            else
                initTask.Wait(TimeSpan.FromSeconds(10));
        }
    }
}
