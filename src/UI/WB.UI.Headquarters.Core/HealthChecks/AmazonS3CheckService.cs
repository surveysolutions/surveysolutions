using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.HealthChecks
{
    /// <summary>
    /// This is simple hosted service that will prevent HQ startup if S3 configuration is incorrect
    /// </summary>
    public class AmazonS3CheckService : IHostedService
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ILogger<AmazonS3CheckService> logger;

        public AmazonS3CheckService(IServiceLocator serviceLocator,
            ILogger<AmazonS3CheckService> logger)
        {
            this.serviceLocator = serviceLocator;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await serviceLocator.ExecuteInScopeAsync(Workspace.Default.AsContext(), async sl =>
            {
                var externalFileStorage = sl.GetInstance<IExternalFileStorage>();

                if (!externalFileStorage.IsEnabled()) return;

                try
                {
                    externalFileStorage.Store(".hc", Encoding.UTF8.GetBytes("Check"), "text/plain");
                    await externalFileStorage.GetBinaryAsync(".hc");
                    await externalFileStorage.ListAsync(".hc");
                    await externalFileStorage.RemoveAsync(".hc");
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Cannot start application with current Amazon S3 configuration: ",
                        e);
                }

                logger.LogInformation("Amazon S3 file storage configuration check completed.");
            });
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
