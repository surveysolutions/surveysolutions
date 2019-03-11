using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.Infrastructure.Versions;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Migrations.PlainStore;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Captcha;
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

            services.AddFluentMigratorCore()
                .ConfigureRunner(b =>
                {
                    b.AddPostgres()
                        .WithGlobalConnectionString("DefaultConnection")
                        .ScanIn(typeof(M001_Init).Assembly).For.Migrations();
                });

            services.AddDbContext<DesignerDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection")));
            
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

            services.Configure<UiConfig>(Configuration.GetSection("UI"));
            services.Configure<CaptchaConfig>(Configuration.GetSection("Captcha"));
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
