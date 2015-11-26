using System;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Interviewer.Syncronization
{
    //has to be reviewed after interview separation from template
    public class CapiCleanUpService : ICapiCleanUpService
    {
        private readonly IChangeLogManipulator changelog;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly IWriteSideCleanerRegistry writeSideCleanerRegistry;
        private readonly IAsyncPlainStorage<InterviewView> interviewInfoRepository;

        public CapiCleanUpService(
            IChangeLogManipulator changelog, 
            IPlainInterviewFileStorage plainInterviewFileStorage, 
            IWriteSideCleanerRegistry writeSideCleanerRegistry,
            IAsyncPlainStorage<InterviewView> interviewInfoRepository)
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
            this.interviewInfoRepository.RemoveAsync(id.FormatGuid());

            this.plainInterviewFileStorage.RemoveAllBinaryDataForInterview(id);
        }
    }
}
