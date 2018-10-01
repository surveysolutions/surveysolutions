using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class DownloadDeletedQuestionnairesList : SynchronizationStep
    {
        private readonly ISupervisorSynchronizationService supervisorSynchronization;
        private readonly IPlainStorage<DeletedQuestionnaire> deletedQuestionnairesStorage;

        public DownloadDeletedQuestionnairesList(int sortOrder, ISynchronizationService synchronizationService, ILogger logger, ISupervisorSynchronizationService supervisorSynchronization, IPlainStorage<DeletedQuestionnaire> deletedQuestionnairesStorage) : base(sortOrder, synchronizationService, logger)
        {
            this.supervisorSynchronization = supervisorSynchronization;
            this.deletedQuestionnairesStorage = deletedQuestionnairesStorage;
        }

        public override async Task ExecuteAsync()
        {
            Context.Progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires,
                Statistics = Context.Statistics,
                Status = SynchronizationStatus.Download,
                Stage = SyncStage.CheckObsoleteQuestionnaires
            });

            var deletedQuestionnairesList = await this.supervisorSynchronization.GetListOfDeletedQuestionnairesIds(Context.CancellationToken);

            this.deletedQuestionnairesStorage
                .Store(deletedQuestionnairesList.Select(id => new DeletedQuestionnaire{ Id = id }));
        }
    }
}
