using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class UserImportRequestHandler :
        IRequestHandler<UserImportRequest, UserImportVerificationError[]>
    {
        private readonly IScheduledTask<UsersImportJob, Unit> usersImportTask;
        private readonly IUserImportService userImportService;

        public UserImportRequestHandler(
            IScheduledTask<UsersImportJob, Unit> usersImportTask, 
            IUserImportService userImportService)
        {
            this.usersImportTask = usersImportTask;
            this.userImportService = userImportService;
        }

        public async Task<UserImportVerificationError[]> Handle(UserImportRequest request, CancellationToken cancellationToken)
        {
            bool isRunning = await this.usersImportTask.IsJobRunning(cancellationToken);

            if (isRunning)
            {
                throw new PreloadingException(UserPreloadingServiceMessages.HasUsersToImport);
            }

            var importUserErrors = this.userImportService.VerifyAndSaveIfNoErrors(request.FileStream, request.Filename)
                .Take(8).ToArray();

            if (!importUserErrors.Any())
            {
                if (!await this.usersImportTask.IsJobRunning(cancellationToken))
                    await this.usersImportTask.Schedule(Unit.Value);
            }

            return importUserErrors;
        }
    }
}