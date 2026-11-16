using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using Quartz;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Persistence.Headquarters.Migrations.Quartz;

namespace WB.Tests.Integration.PostgreSQLTests;

public class QuartzMigrationTests : with_postgres_db
{
    [Test]
    public async Task when_upgrading_legacy_quartz_should_preserve_jobs_and_start_scheduler()
    {
        var connectionString = ConnectionStringBuilder.ConnectionString;
        DatabaseManagement.InitDatabase(connectionString, "quartz");
        var migrationConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            SearchPath = "quartz"
        }.ConnectionString;

        using (var services = new ServiceCollection()
            .AddLogging()
            .Configure<TypeFilterOptions>(options =>
                options.Namespace = typeof(M201905151013_AddQuartzTables).Namespace)
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddPostgres("quartz")
                .WithGlobalConnectionString(migrationConnectionString)
                .ScanIn(typeof(M201905151013_AddQuartzTables).Assembly)
                .For.Migrations().For.EmbeddedResources())
            .BuildServiceProvider())
        using (var scope = services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp(202012151633);
        }

        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.ExecuteAsync(@"
                ALTER TABLE quartz.triggers ADD COLUMN execution_group varchar(200) NULL;
                INSERT INTO quartz.job_details
                    (sched_name, job_name, job_group, job_class_name, is_durable, is_nonconcurrent,
                     is_update_data, requests_recovery)
                VALUES ('Quartz migration test', 'legacy', 'DEFAULT', @JobType, true, false, false, false);
                INSERT INTO quartz.triggers
                    (sched_name, trigger_name, trigger_group, job_name, job_group, trigger_state,
                     trigger_type, start_time, priority, misfire_instr)
                VALUES ('Quartz migration test', 'legacy-trigger', 'legacy-group', 'legacy', 'DEFAULT',
                        'PAUSED', 'SIMPLE', 638000000000000000, 5, 0);
                INSERT INTO quartz.simple_triggers
                    (sched_name, trigger_name, trigger_group, repeat_count, repeat_interval, times_triggered)
                VALUES ('Quartz migration test', 'legacy-trigger', 'legacy-group', 0, 0, 0);
                INSERT INTO quartz.paused_trigger_grps (sched_name, trigger_group)
                VALUES ('Quartz migration test', 'legacy-group');",
                new { JobType = typeof(TestJob).AssemblyQualifiedName });
        }

        var settings = DbUpgradeSettings.FromFirstMigration<M201905151013_AddQuartzTables>();
        DbMigrationsRunner.MigrateToLatest(connectionString, "quartz", settings);

        var servicesCollection = new ServiceCollection().AddLogging();
        servicesCollection.AddQuartz(q =>
        {
            q.ConfigureScheduler(options => options.InstanceName = "Quartz migration test");
            q.UsePersistentStore(store =>
            {
                store.UsePostgres(options => options.ConnectionString = connectionString);
                store.ConfigureStore(options =>
                {
                    options.TablePrefix = "quartz.";
                    options.StoreJobDataAsStrings = true;
                });
                store.UseClustering();
                store.UseNewtonsoftJsonSerializer();
            });
        });

        using var schedulerServices = servicesCollection.BuildServiceProvider();
        var scheduler = await schedulerServices.GetRequiredService<ISchedulerFactory>().GetScheduler();
        try
        {
            Assert.That(await scheduler.GetJobDetail(new JobKey("legacy")), Is.Not.Null);
            Assert.That(await scheduler.GetTrigger(new TriggerKey("legacy-trigger", "legacy-group")), Is.Not.Null);
            Assert.That(await scheduler.IsTriggerGroupPaused("legacy-group"), Is.True);

            var job = JobBuilder.Create<TestJob>().WithIdentity("new-job").Build();
            var trigger = TriggerBuilder.Create().WithIdentity("new-trigger")
                .StartAt(DateTimeOffset.UtcNow.AddMinutes(10)).Build();
            await scheduler.ScheduleJob(job, trigger);
            Assert.That(await scheduler.GetTrigger(trigger.Key), Is.Not.Null);
            await scheduler.Start();
        }
        finally
        {
            await scheduler.Shutdown();
        }
    }

    public class TestJob : IJob
    {
        public ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
