﻿using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.Versions;
using WB.UI.Designer.Code;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Versions;

namespace WB.UI.Designer1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
          
            services.AddDbContext<DesignerDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IPasswordHasher<DesignerIdentityUser>, PasswordHasher>();
            services.AddDefaultIdentity<DesignerIdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<DesignerDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
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
            services.AddScoped<IViewRenderingService, ViewRenderingService>();
            services.AddScoped<IQuestionnaireHelper, QuestionnaireHelper>();
            services.AddScoped<IQuestionnaireListViewFactory, QuestionnaireListViewFactory>();
            services.AddScoped<IAccountListViewFactory, AccountListViewFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

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
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            
        }
    }
}
