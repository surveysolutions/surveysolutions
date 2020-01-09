using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Jobs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Headquarters.Code
{
    public class QuartzHostedService : IHostedService
    {
        private readonly IScheduler scheduler;
        private readonly IServiceLocator serviceLocator;

        public QuartzHostedService(
            IScheduler scheduler,
            IServiceLocator serviceLocator)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.serviceLocator = serviceLocator ?? throw new ArgumentNullException(nameof(serviceLocator));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await serviceLocator.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            await serviceLocator.GetInstance<UsersImportTask>().ScheduleRunAsync();
            await serviceLocator.GetInstance<AssignmentsImportTask>().Schedule(repeatIntervalInSeconds: 300);
            await serviceLocator.GetInstance<AssignmentsVerificationTask>().Schedule(repeatIntervalInSeconds: 300);
            await serviceLocator.GetInstance<DeleteQuestionnaireJobScheduler>().Schedule(repeatIntervalInSeconds: 10);
            await serviceLocator.GetInstance<PauseResumeJobScheduler>().Configure();
            await serviceLocator.GetInstance<UpgradeAssignmentJobScheduler>().Configure();
            await serviceLocator.GetInstance<SendInvitationsTask>().ScheduleRunAsync();
            await serviceLocator.GetInstance<SendRemindersTask>().Schedule(repeatIntervalInSeconds: 60 * 60);

            await scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await scheduler.Shutdown(cancellationToken);
        }
    }
}
