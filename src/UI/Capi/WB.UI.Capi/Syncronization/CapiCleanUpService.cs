using System;
using System.Collections.Generic;
using System.Linq;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Capi.ViewModel.Dashboard;

namespace WB.UI.Capi.Syncronization
{
    //has to be reviewed after interview separation from template
    public class CapiCleanUpService : ICapiCleanUpService
    {
        private readonly IChangeLogManipulator changelog;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;

        private readonly ISyncPackageIdsStorage syncPackageIdsStorage;

        public CapiCleanUpService(
            IChangeLogManipulator changelog, 
            IPlainInterviewFileStorage plainInterviewFileStorage, 
            ISyncPackageIdsStorage syncPackageIdsStorage)
        {
            this.changelog = changelog;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.syncPackageIdsStorage = syncPackageIdsStorage;
        }

        public void DeleteAllInterviewsForUser(Guid userIdAsGuid)
        {
            string userId = userIdAsGuid.FormatGuid();
            var interviewsForDashboard = CapiApplication.Kernel.Get<IFilterableReadSideRepositoryReader<QuestionnaireDTO>>();

            List<Guid> interviewIds = interviewsForDashboard
                .Filter(q => q.Responsible == userId)
                .Select(x => Guid.Parse(x.Id))
                .ToList();

            foreach (var interviewId in interviewIds)
            {
                this.DeleteInterview(interviewId);
            }

            syncPackageIdsStorage.CleanAllInterviewIdsForUser(userIdAsGuid);
        }

        //dangerous operation
        //deletes all information about Interview
        public void DeleteInterview(Guid id)
        {
            this.changelog.CleanUpChangeLogByEventSourceId(id);

            var writeSideCleanerRegistry = CapiApplication.Kernel.Get<IWriteSideCleanerRegistry>();

            foreach (var writeSideCleaner in writeSideCleanerRegistry.GetAll())
            {
                writeSideCleaner.Clean(id);
            }

            //todo: notify denormalizes

            //think about more elegant solution
            var questionnaireDtoWriter = CapiApplication.Kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>();
            if (questionnaireDtoWriter!=null)
                questionnaireDtoWriter.Remove(id);

            this.plainInterviewFileStorage.RemoveAllBinaryDataForInterview(id);
        }
    }
}
