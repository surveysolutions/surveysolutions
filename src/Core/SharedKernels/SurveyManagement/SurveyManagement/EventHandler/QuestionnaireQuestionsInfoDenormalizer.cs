using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class QuestionnaireQuestionsInfoDenormalizer : BaseDenormalizer, IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireQuestionsInfo> questionnaires;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public QuestionnaireQuestionsInfoDenormalizer(IReadSideKeyValueStorage<QuestionnaireQuestionsInfo> questionnaires, IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.questionnaires = questionnaires;
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

            this.StoreQuestionsInfo(id, version, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreQuestionsInfo(id, version, questionnaireDocument);
        }

        private void StoreQuestionsInfo(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            var map = new QuestionnaireQuestionsInfo
            {
                QuestionIdToVariableMap =
                    questionnaireDocument.Find<IQuestion>(question => true).ToDictionary(x => x.PublicKey, x => x.StataExportCaption)
            };

            this.questionnaires.Store(map, RepositoryKeysHelper.GetVersionedKey(id, version));
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.questionnaires.Remove(RepositoryKeysHelper.GetVersionedKey(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion));
        }
    }
}