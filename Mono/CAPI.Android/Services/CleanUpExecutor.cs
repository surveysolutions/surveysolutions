using System;
using AndroidNcqrs.Eventing.Storage.SQLite;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.SnapshotStore;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Services
{

    //has to be reviewed after interview separation from template
    public class CleanUpExecutor
    {
        private readonly IChangeLogManipulator changelog;

        public CleanUpExecutor(IChangeLogManipulator changelog)
        {
            this.changelog = changelog;
        }

        //dengerous operation
        //deletes all information about Interview
        public void DeleteInterveiw(Guid id)
        {
            changelog.CleanUpChangeLogByEventSourceId(id);

            //delete from snapshot store
            var storage = NcqrsEnvironment.Get<ISnapshotStore>() as AndroidSnapshotStore;
            if (storage != null)
                storage.DeleteSnapshot(id);

            //delete from read model
            var documentStorage = CapiApplication.Kernel.Get<IReadSideRepositoryWriter<CompleteQuestionnaireView>>();
            documentStorage.Remove(id);

            //delete from event store
            #warning invent some better way of doing that
            var eventStore = NcqrsEnvironment.Get<IEventStore>() as MvvmCrossSqliteEventStore;
            if (eventStore != null)
                eventStore.CleanStream(id);
            
            //todo: notify denormalizes
            
            //think about more elegant solution
            CapiApplication.Kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>().Remove(id);
            CapiApplication.Kernel.Get<IReadSideRepositoryWriter<CompleteQuestionnaireView>>().Remove(id);

        }

        //frees up memory dedicated to this interview  
        public void CleanUpInterviewCaches(Guid id)
        {
            var storage = NcqrsEnvironment.Get<ISnapshotStore>() as AndroidSnapshotStore;
            if (storage != null)
                storage.FlushSnapshot(id);

            var documentStorage = CapiApplication.Kernel.Get<IReadSideRepositoryWriter<CompleteQuestionnaireView>>();
            documentStorage.Remove(id);
        }

    }
}
