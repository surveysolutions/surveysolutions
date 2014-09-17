using System;
using AndroidNcqrs.Eventing.Storage.SQLite;
using CAPI.Android.Core.Model.SnapshotStore;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Services;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Capi.Syncronization
{

    //has to be reviewed after interview separation from template
    public class CapiCleanUpService : ICapiCleanUpService
    {
        private readonly IChangeLogManipulator changelog;
        private readonly IPlainFileRepository plainFileRepository;

        public CapiCleanUpService(IChangeLogManipulator changelog, IPlainFileRepository plainFileRepository)
        {
            this.changelog = changelog;
            this.plainFileRepository = plainFileRepository;
        }


        //dangerous operation
        //deletes all information about Interview
        public void DeleteInterview(Guid id)
        {
            this.changelog.CleanUpChangeLogByEventSourceId(id);

            //delete from event store
#warning invent some better way of doing that
            var eventStore = NcqrsEnvironment.Get<IEventStore>() as MvvmCrossSqliteEventStore;
            if (eventStore != null)
                eventStore.CleanStream(id);

            var snapshotStore = NcqrsEnvironment.Get<ISnapshotStore>() as FileBasedSnapshotStore;
            if (snapshotStore != null)
                snapshotStore.DeleteSnapshot(id);

            //todo: notify denormalizes

            //think about more elegant solution
            CapiApplication.Kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>().Remove(id);
            CapiApplication.Kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>().Remove(id);
            plainFileRepository.RemoveAllBinaryDataForInterview(id);
        }
    }
}
