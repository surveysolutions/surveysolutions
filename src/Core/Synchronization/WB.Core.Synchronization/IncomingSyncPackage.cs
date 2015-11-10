using System;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.Synchronization
{
    public class IncomingSyncPackage
    {
        public IncomingSyncPackage(Guid interviewId, 
            Guid responsibleId,
            Guid questionnaireId, 
            long questionnaireVersion, 
            InterviewStatus interviewStatus, 
            ILiteEvent[] eventsToSynchronize, 
            bool createdOnClient, 
            string origin, 
            string pathToPackage)
        {
            InterviewId = interviewId;
            QuestionnaireId = questionnaireId;
            QuestionnaireVersion = questionnaireVersion;
            InterviewStatus = interviewStatus;
            EventsToSynchronize = eventsToSynchronize;
            CreatedOnClient = createdOnClient;
            Origin = origin;
            PathToPackage = pathToPackage;
            ResponsibleId = responsibleId;
        }

        public Guid InterviewId { get; private set; }
        public Guid ResponsibleId { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public InterviewStatus InterviewStatus { get; private set; }
        public ILiteEvent[] EventsToSynchronize { get; private set; }
        public bool CreatedOnClient { get; private set; }
        public string Origin { get; private set; }
        public string PathToPackage { get; private set; }
    }
}