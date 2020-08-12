using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using WB.Infrastructure.Native;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Migrations.Logs;
using WB.UI.Designer.Migrations.PlainStore;

namespace WB.UI.Designer.SupportTool
{
    public class MigrateCommand : Command
    {
        private readonly IHost host;
        private IConfiguration Configuration => host.Services.GetRequiredService<IConfiguration>();

        public MigrateCommand(IHost host) : base("migrate", "Migrate database to latest version")
        {
            this.host = host;

            this.AddOption(new Option("--wait-for-db", "Wait for DB to be available. Default is False")
            {
                Required = false,
                Argument = new Argument<bool>(() => false)
            });

            this.AddOption(new Option("--timeout", "Limit wait time for DB in seconds. Default is 0. Used in conjunction with --wait-for-db option")
            {
                Required = false,
                Argument = new Argument<long>(() => 0)
            });


            this.Handler = CommandHandler.Create<bool, long>(Migrate);
        }

        private async Task Migrate(bool waitForDb, long timeout)
        {
            var logger = host.Services.GetRequiredService<ILogger<MigrateCommand>>();

            if (waitForDb)
            {
                var sw = Stopwatch.StartNew();
                await Policy.Handle<Exception>(e =>
                {
                    if (timeout > 0 && sw.Elapsed.TotalSeconds > timeout)
                    {
                        logger.LogWarning("Wait timeout reached. Migration failed");
                        return false;
                    }
                    if (e is InitializationException ie)
                    {
                        if (ie.InnerException is NpgsqlException npge)
                        {
                            logger.LogError("Error: {message}", npge.Message);

                            if (npge.InnerException != null)
                            {
                                logger.LogError("Inner: {inner}", npge.InnerException.Message);
                            }

                            return true;
                        }

                    }

                    logger.LogError("Failed to migrate. Stopping application. {exception}", e);
                    return false;
                })
                .WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(1),
                    (e, i, time) => { logger.LogWarning("Attempt #{attempt} to run migrations", i); })
                .ExecuteAsync(() =>
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    host
                        .RunMigrations(typeof(M001_Init), "plainstore")
                        .RunMigrations(typeof(M201904221727_AddErrorsTable), "logs");

                    logger.LogInformation($"All migrations completed in {sw.Elapsed.TotalSeconds.Seconds()}");
                    return Task.CompletedTask;
                });
            }
            else
            {
                host
                    .RunMigrations(typeof(M001_Init), "plainstore")
                    .RunMigrations(typeof(M201904221727_AddErrorsTable), "logs");
            }

            logger.LogInformation("Headquarters migrated to latest ");
        }
    }
}
