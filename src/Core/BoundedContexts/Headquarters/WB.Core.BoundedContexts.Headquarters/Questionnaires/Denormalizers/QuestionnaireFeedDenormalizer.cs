using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers
{
    internal class QuestionnaireFeedDenormalizer : BaseDenormalizer,
                                    IEventHandler<TemplateImported>,
                                    IEventHandler<QuestionnaireDeleted>,
                                    IEventHandler
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireFeedEntry> questionnaireFeed;

        public override object[] Writers
        {
            get { return new object[] { questionnaireFeed}; }
        }

        public QuestionnaireFeedDenormalizer(IReadSideRepositoryWriter<QuestionnaireFeedEntry> questionnaireFeed)
        {
            this.questionnaireFeed = questionnaireFeed;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var eventId = evnt.EventIdentifier.FormatGuid();
            var entityType = evnt.Payload.AllowCensusMode
                ? QuestionnaireEntryType.QuestionnaireCreatedInCensusMode
                : QuestionnaireEntryType.QuestionnaireCreated;
            questionnaireFeed.Store(new QuestionnaireFeedEntry(evnt.EventSourceId, evnt.Payload.Version ?? evnt.EventSequence, eventId, entityType, evnt.EventTimeStamp), eventId);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            var eventId = evnt.EventIdentifier.FormatGuid();
            questionnaireFeed.Store(new QuestionnaireFeedEntry(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion, eventId, QuestionnaireEntryType.QuestionnaireDeleted, evnt.EventTimeStamp), eventId);
        }
    }
}
