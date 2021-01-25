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
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.SupportTool
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
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var logger = host.Services.GetRequiredService<ILogger<MigrateCommand>>();

            UnitOfWorkConnectionSettings unitOfWorkConnectionSettings = Startup.BuildUnitOfWorkSettings(
                connectionString, Configuration);

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
                .ExecuteAsync(async () =>
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    var uinfo = host.Services.GetRequiredService<UnderConstructionInfo>();

                    await new OrmModule(unitOfWorkConnectionSettings)
                        .Init(host.Services.GetRequiredService<IServiceLocator>(), uinfo);
                    
                    logger.LogInformation($"All migrations completed in {sw.Elapsed.TotalSeconds.Seconds()}");
                });
            }
            else
            {
                var uinfo = host.Services.GetRequiredService<UnderConstructionInfo>();
                await new OrmModule(unitOfWorkConnectionSettings)
                    .Init(host.Services.GetRequiredService<IServiceLocator>(), uinfo);
            }

            logger.LogInformation("Headquarters migrated to latest ");
        }
    }
}
