using System;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Interviewer.ViewModel.Dashboard;

namespace WB.UI.Interviewer.Syncronization
{
    //has to be reviewed after interview separation from template
    public class CapiCleanUpService : ICapiCleanUpService
    {
        private readonly IChangeLogManipulator changelog;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly IWriteSideCleanerRegistry writeSideCleanerRegistry;
        private readonly IReadSideRepositoryWriter<QuestionnaireDTO> interviewInfoRepository;

        public CapiCleanUpService(
            IChangeLogManipulator changelog, 
            IPlainInterviewFileStorage plainInterviewFileStorage, 
            IWriteSideCleanerRegistry writeSideCleanerRegistry,
            IReadSideRepositoryWriter<QuestionnaireDTO> interviewInfoRepository)
        {
            this.changelog = changelog;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.writeSideCleanerRegistry = writeSideCleanerRegistry;
            this.interviewInfoRepository = interviewInfoRepository;
        }

        //dangerous operation
        //deletes all information about Interview
        public void DeleteInterview(Guid id)
        {
            this.changelog.CleanUpChangeLogByEventSourceId(id);

            foreach (var writeSideCleaner in writeSideCleanerRegistry.GetAll())
            {
                writeSideCleaner.Clean(id);
            }

            //todo: notify denormalizes
            this.interviewInfoRepository.Remove(id);

            this.plainInterviewFileStorage.RemoveAllBinaryDataForInterview(id);
        }
    }
}
