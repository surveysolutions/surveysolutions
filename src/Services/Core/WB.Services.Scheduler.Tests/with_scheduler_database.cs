﻿using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Services.Implementation;
using WB.Services.Scheduler.Storage;

namespace WB.Services.Scheduler.Tests
{
    public abstract class with_scheduler : with_scheduler_database
    {
        protected ServiceProvider serviceProvider;

        protected async Task CreateNewJobs(params JobItem[] jobs)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var jobService = scope.ServiceProvider.GetService<JobService>();

                foreach (var job in jobs)
                {
                    await jobService.AddNewJobAsync(job);
                }
            }
        }
    }

    public abstract class with_scheduler_database
    {
        protected Mock<IOptions<JobSettings>> jobSettingsMock;
        protected string SchemaName;

        protected IServiceCollection NewServiceCollection()
        {
            var services = new ServiceCollection()
                .AddDbContext<JobContext>(ops =>
                    ops.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
            
            jobSettingsMock = new Mock<IOptions<JobSettings>>();

            jobSettingsMock.Setup(m => m.Value).Returns(() => new JobSettings()
            {
                SchemaName = SchemaName
            });

            services.AddSingleton(jobSettingsMock.Object);
            services.AddTransient(typeof(ILogger<>), typeof(NullLogger<>));
            return services;
        }

        [SetUp]
        public async Task Init()
        {
            var person = new Bogus.Person();

            this.SchemaName = "test_schema_";// + person.UserName.ToLowerInvariant().Replace(".", "");

            Console.WriteLine(SchemaName);
            using (var scope = PrepareOneTime().CreateScope())
            {
                var db = scope.ServiceProvider.GetService<JobContext>();

                try { await db.Database.ExecuteSqlRawAsync($"DROP SCHEMA IF EXISTS " + SchemaName + " CASCADE"); }
                catch { }

                await EnsurePublicSchemaExists(db.Database);
                await db.Database.MigrateAsync();
                await db.Database.ExecuteSqlRawAsync("ALTER SCHEMA scheduler RENAME TO " + SchemaName);
            }
        }

          
        private static async Task EnsurePublicSchemaExists(DatabaseFacade db)
        {
            try
            {
                await db.GetDbConnection().ExecuteAsync("create schema if not exists public");
            }
            catch { /* 
                    If DB is not created, then db.Database.MigrateAsync will create it with public schema
                    but if there is already created DB without public schema, them MigrateAsync will fail.
                    So it's OK to fail here and om om om exception and fail later on Migrate if there is a 
                    problem with migrations or DB access
                 */ }
         }
 

        [TearDown]
        public async Task Down()
        {
            using (var scope = PrepareOneTime().CreateScope())
            {
                var db = scope.ServiceProvider.GetService<JobContext>();
                
                await db.Database.ExecuteSqlRawAsync($"DROP SCHEMA " + SchemaName +" CASCADE");
                await db.Database.ExecuteSqlRawAsync($"DROP Table public.\"__EFMigrationsHistory\"");
               // db.Database.CommitTransaction();
            }
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            using (var scope = PrepareOneTime().CreateScope())
            {
                var db = scope.ServiceProvider.GetService<JobContext>();
                await db.Database.EnsureDeletedAsync();
            }
        }

        protected IConfiguration Configuration => new ConfigurationBuilder()
            .AddJsonFile($@"appsettings.json", true)
            .Build();

        private IServiceProvider PrepareOneTime()
        {
            var services = NewServiceCollection();

            return services.BuildServiceProvider();
        }
    }
}
