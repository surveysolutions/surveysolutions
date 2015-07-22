using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteSupervisor;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteSupervisor
{
    internal class DeleteSupervisorService : IDeleteSupervisorService
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> userDocumentReader;
        private readonly ICommandService commandService;
        private readonly ILogger logger;

        public DeleteSupervisorService(
            IQueryableReadSideRepositoryReader<UserDocument> userDocumentReader,
            ICommandService commandService,
            ILogger logger, IQueryableReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.userDocumentReader = userDocumentReader;
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
                    userDocumentReader.Query(
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
