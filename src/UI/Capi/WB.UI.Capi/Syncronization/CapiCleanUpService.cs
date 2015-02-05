﻿using System;
using System.Collections.Generic;
using System.Linq;
using AndroidNcqrs.Eventing.Storage.SQLite;

using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Capi.SnapshotStore;

namespace WB.UI.Capi.Syncronization
{
    //has to be reviewed after interview separation from template
    public class CapiCleanUpService : ICapiCleanUpService
    {
        private readonly IChangeLogManipulator changelog;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;

        public CapiCleanUpService(IChangeLogManipulator changelog, IPlainInterviewFileStorage plainInterviewFileStorage)
        {
            this.changelog = changelog;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
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
            this.plainInterviewFileStorage.RemoveAllBinaryDataForInterview(id);
        }
    }
}
