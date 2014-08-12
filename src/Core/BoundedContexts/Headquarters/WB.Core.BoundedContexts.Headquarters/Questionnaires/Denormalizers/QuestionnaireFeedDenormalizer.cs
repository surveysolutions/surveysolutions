using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers
{
    internal class QuestionnaireFeedDenormalizer : BaseDenormalizer,
                                    IEventHandler<TemplateImported>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireFeedEntry> questionnaireFeed;

        public override Type[] BuildsViews
        {
            get { return new Type[] { typeof(QuestionnaireFeedEntry) }; }
        }

        public QuestionnaireFeedDenormalizer(IReadSideRepositoryWriter<QuestionnaireFeedEntry> questionnaireFeed)
        {
            this.questionnaireFeed = questionnaireFeed;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var eventId = evnt.EventIdentifier.FormatGuid();
            questionnaireFeed.Store(new QuestionnaireFeedEntry(evnt.EventSourceId, evnt.EventSequence, eventId, evnt.Payload.AllowCensusMode, evnt.EventTimeStamp), eventId);
        }
    }
}
