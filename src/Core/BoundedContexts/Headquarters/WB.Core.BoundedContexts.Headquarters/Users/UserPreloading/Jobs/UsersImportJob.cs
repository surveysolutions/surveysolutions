using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.Infrastructure.Domain;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    [RetryFailedJob]
    [DisplayName("Users import"), Category("Import")]
    public class UsersImportJob : IJob<Unit>
    {
        private readonly IInScopeExecutor<IMediator> inScopeExecutor;
        private readonly ILogger<UsersImportJob> logger;

        public UsersImportJob(IInScopeExecutor<IMediator> inScopeExecutor,
            ILogger<UsersImportJob> logger)
        {
            this.inScopeExecutor = inScopeExecutor;
            this.logger = logger;
        }

        public async Task Execute(Unit @void, IJobExecutionContext context)
        {
            var sw = new Stopwatch();
            sw.Start();

            await ImportUsersAsync();

            sw.Stop();
            logger.LogInformation("User import job: Finished. Elapsed time: {elapsed}", sw.Elapsed);
        }

        private async Task ImportUsersAsync()
        {
            try
            {
                UserToImport userToImport = null;

                do
                {
                    userToImport = await this.inScopeExecutor.ExecuteAsync(
                        mediator => mediator.Send(new CreateOrUnArchiveUserRequest()));
                } while (userToImport != null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User import job: FAILED");
                throw;
            }
        }
    }
}
