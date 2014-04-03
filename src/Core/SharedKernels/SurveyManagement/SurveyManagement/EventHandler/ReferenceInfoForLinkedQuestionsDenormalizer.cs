using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class ReferenceInfoForLinkedQuestionsDenormalizer : IEventHandler, IEventHandler<TemplateImported>
    {
        private readonly IVersionedReadSideRepositoryWriter<ReferenceInfoForLinkedQuestions> questionnaires;
        private readonly IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory;

        public ReferenceInfoForLinkedQuestionsDenormalizer(
            IVersionedReadSideRepositoryWriter<ReferenceInfoForLinkedQuestions> questionnaires, IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory)
        {
            this.questionnaires = questionnaires;
            this.referenceInfoForLinkedQuestionsFactory = referenceInfoForLinkedQuestionsFactory;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new[] {typeof (ReferenceInfoForLinkedQuestions)}; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            evnt.Payload.Source.ConnectChildrenWithParent();

            this.questionnaires.Store(this.referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(evnt.Payload.Source, evnt.EventSequence), evnt.EventSourceId);
        }
    }
}
