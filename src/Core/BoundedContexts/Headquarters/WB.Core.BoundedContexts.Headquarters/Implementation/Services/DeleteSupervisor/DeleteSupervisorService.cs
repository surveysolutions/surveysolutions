using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteSupervisor;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteSupervisor
{
    internal class DeleteSupervisorService : IDeleteSupervisorService
    {
        private readonly IPlainStorageAccessor<UserDocument> userDocumentStorage;
        private readonly ICommandService commandService;
        private readonly ILogger logger;

        public DeleteSupervisorService(
            IPlainStorageAccessor<UserDocument> userDocumentStorage,
            ICommandService commandService,
            ILogger logger, IQueryableReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.userDocumentStorage = userDocumentStorage;
            this.commandService = commandService;
            this.logger = logger;
        }

        public void DeleteSupervisor(Guid supervisorId)
        {
            var deletedInterviewers = new List<Guid>();
            try
            {
                this.commandService.Execute(new ArchiveUserCommad(supervisorId));
                var interviewerIds =
                    this.userDocumentStorage.Query(
                        _ =>
                            _.Where(u => u.Supervisor.Id == supervisorId && !u.IsArchived)
                                .Select(u => u.PublicKey)
                                .ToList());

                foreach (var interviewerId in interviewerIds)
                {
                    this.commandService.Execute(new ArchiveUserCommad(interviewerId));
                    deletedInterviewers.Add(interviewerId);
                }
            }
            catch (Exception)
            {
                this.commandService.Execute(new UnarchiveUserCommand(supervisorId));
                foreach (var deletedInterviewer in deletedInterviewers)
                {
                    this.commandService.Execute(new UnarchiveUserCommand(deletedInterviewer));
                }
                throw;
            }
        }
    }
}
