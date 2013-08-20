using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class QuestionnairePropagationStructureDenormalizer : IEventHandler, IEventHandler<TemplateImported>
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure> questionnries;

        public QuestionnairePropagationStructureDenormalizer(
            IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure> questionnries)
        {
            this.questionnries = questionnries;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new[] {typeof (QuestionnairePropagationStructure)}; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var questionnarie = evnt.Payload.Source;

            var questionnairePropagationStructure =
                new QuestionnairePropagationStructure()
                    {
                        QuestionnaireId = evnt.EventSourceId,
                        Version = evnt.EventSequence
                    };

            questionnairePropagationStructure.PropagationScopes = new Dictionary<Guid, HashSet<Guid>>();

            var autoPropagatebleQuestions =
                questionnarie.Find<IAutoPropagateQuestion>(
                    question =>
                    question.QuestionType == QuestionType.Numeric || question.QuestionType == QuestionType.AutoPropagate);

            foreach (var autoPropagatebleQuestion in autoPropagatebleQuestions)
            {
                var triggerHashSet = new HashSet<Guid>();

                foreach (var trigger in autoPropagatebleQuestion.Triggers)
                {
                    triggerHashSet.Add(trigger);
                }

                questionnairePropagationStructure.PropagationScopes.Add(autoPropagatebleQuestion.PublicKey,
                                                                        triggerHashSet);
            }
            questionnries.Store(questionnairePropagationStructure,evnt.EventSourceId);
        }
    }
}
