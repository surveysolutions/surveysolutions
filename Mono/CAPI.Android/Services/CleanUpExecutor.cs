﻿using System;
using AndroidNcqrs.Eventing.Storage.SQLite;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.SnapshotStore;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
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

        //dangerous operation
        //deletes all information about Interview
        public void DeleteInterveiw(Guid id)
        {
            changelog.CleanUpChangeLogByEventSourceId(id);

            //delete from event store
            #warning invent some better way of doing that
            var eventStore = NcqrsEnvironment.Get<IEventStore>() as MvvmCrossSqliteEventStore;
            if (eventStore != null)
                eventStore.CleanStream(id);

            var snapshotStore = NcqrsEnvironment.Get<ISnapshotStore>() as AndroidSnapshotStore;
            if (snapshotStore != null)
                snapshotStore.DeleteSnapshot(id);
            
            //todo: notify denormalizes
            
            //think about more elegant solution
            CapiApplication.Kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>().Remove(id);
            CapiApplication.Kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>().Remove(id);

        }
    }
}
