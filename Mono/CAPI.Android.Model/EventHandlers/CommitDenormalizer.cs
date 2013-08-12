using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire.Completed;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class CommitDenormalizer : IEventHandler<InterviewCompleted>,
                                      IEventHandler<InterviewRestarted>, 
                                      IEventHandler<InterviewSynchronized>
    {
        public CommitDenormalizer(IChangeLogManipulator changeLog)
        {
            this.changeLog = changeLog;
        }

        private readonly IChangeLogManipulator changeLog;

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            changeLog.CloseDraftRecord(evnt.EventSourceId, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            changeLog.ReopenDraftRecord(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            changeLog.OpenDraftRecord(evnt.EventSourceId, evnt.EventSequence + 1);
        }
    }
}