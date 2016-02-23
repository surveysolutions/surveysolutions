using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class QuestionnaireDenormalizer : BaseDenormalizer, 
        IEventHandler<TemplateImported>, 
        IEventHandler<QuestionnaireDeleted>
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IPlainTransactionManager plainTransactionManager;

        public QuestionnaireDenormalizer(
            IPlainQuestionnaireRepository plainQuestionnaireRepository, IPlainTransactionManager plainTransactionManager)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainTransactionManager = plainTransactionManager;
        }

        public override object[] Writers => new object[0];

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version ?? evnt.EventSequence;

            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);
            if (questionnaireDocument == null)
            {
                questionnaireDocument = evnt.Payload.Source;
                plainTransactionManager.ExecuteInPlainTransaction(
                    () => this.plainQuestionnaireRepository.StoreQuestionnaire(id, version, questionnaireDocument));
            }
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.QuestionnaireVersion;

            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);
            if (questionnaireDocument != null)
            {
                questionnaireDocument.IsDeleted = true;
                plainTransactionManager.ExecuteInPlainTransaction(
                    () => this.plainQuestionnaireRepository.StoreQuestionnaire(id, version, questionnaireDocument));
            }
        }
    }
}