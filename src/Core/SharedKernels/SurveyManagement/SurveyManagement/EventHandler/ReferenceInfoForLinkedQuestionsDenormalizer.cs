using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class ReferenceInfoForLinkedQuestionsDenormalizer : BaseDenormalizer, IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted>
    {
        private readonly IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaires;
        private readonly IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public ReferenceInfoForLinkedQuestionsDenormalizer(
            IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaires,
            IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory,
            IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.questionnaires = questionnaires;
            this.referenceInfoForLinkedQuestionsFactory = referenceInfoForLinkedQuestionsFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public override object[] Writers
        {
            get { return new[] { questionnaires }; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            this.StoreReferenceInfo(id, version, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreReferenceInfo(id, version, questionnaireDocument);
        }

        private void StoreReferenceInfo(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            questionnaireDocument.ConnectChildrenWithParent();

            ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions =
                this.referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(questionnaireDocument, version);

            this.questionnaires.AsVersioned().Store(referenceInfoForLinkedQuestions, id.FormatGuid(), version);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.questionnaires.AsVersioned().Remove(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion);
        }
    }
}
