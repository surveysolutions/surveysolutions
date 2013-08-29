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

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class CommitDenormalizer : IEventHandler<QuestionnaireStatusChanged>, IEventHandler<NewAssigmentCreated>
    {
        public CommitDenormalizer(IChangeLogManipulator changeLog)
        {
            this.changeLog = changeLog;
        }

        private readonly IChangeLogManipulator changeLog;

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            if (evnt.Payload.Status == SurveyStatus.Complete || evnt.Payload.Status == SurveyStatus.Error)
            {
                changeLog.CloseDraftRecord(evnt.EventSourceId, evnt.EventSequence);
                return;
            }
            if (evnt.Payload.Status == SurveyStatus.Reinit)
            {
                changeLog.ReopenDraftRecord(evnt.EventSourceId);
                return;
            }
        }

        public void Handle(IPublishedEvent<NewAssigmentCreated> evnt)
        {
            changeLog.OpenDraftRecord(evnt.EventSourceId, evnt.EventSequence + 1);
        }
    }
}