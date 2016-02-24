using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    [Obsolete]
    public class QuestionnaireBrowseItemEventHandler : BaseDenormalizer, IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted>,
        IEventHandler<QuestionnaireDisabled>
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemWriter;
        private readonly IPlainTransactionManager plainTransactionManager;

        public QuestionnaireBrowseItemEventHandler(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemWriter, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IPlainTransactionManager plainTransactionManager)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainTransactionManager = plainTransactionManager;
            this.questionnaireBrowseItemWriter = questionnaireBrowseItemWriter;
        }

        public override object[] Writers => new object[] { this.questionnaireBrowseItemWriter };

        private  QuestionnaireBrowseItem CreateBrowseItem(long version, QuestionnaireDocument questionnaireDocument, bool allowCensusMode, long questionnaireContentVersion)
        {
            return new QuestionnaireBrowseItem(questionnaireDocument, version, allowCensusMode, questionnaireContentVersion);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(evnt.EventSourceId, version);

            var view = this.CreateBrowseItem(version, questionnaireDocument, evnt.Payload.AllowCensusMode, evnt.Payload.ContentVersion??1);
            plainTransactionManager.ExecuteInPlainTransaction(() => this.questionnaireBrowseItemWriter.Store(view, view.Id));
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            var view = this.CreateBrowseItem(version, questionnaireDocument, evnt.Payload.AllowCensusMode, 1);
            plainTransactionManager.ExecuteInPlainTransaction(() => this.questionnaireBrowseItemWriter.Store(view, view.Id));
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            var versionedWrapper = this.questionnaireBrowseItemWriter.AsVersioned();
            var browseItem = plainTransactionManager.ExecuteInPlainTransaction(() => versionedWrapper.Get(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion));
            if (browseItem == null)
                return;

            browseItem.IsDeleted = true;
            plainTransactionManager.ExecuteInPlainTransaction(() => this.questionnaireBrowseItemWriter.Store(browseItem, browseItem.Id));
        }

        public void Handle(IPublishedEvent<QuestionnaireDisabled> evnt)
        {
            var versionedWrapper = this.questionnaireBrowseItemWriter.AsVersioned();
            var browseItem = plainTransactionManager.ExecuteInPlainTransaction(() => versionedWrapper.Get(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion));
            if (browseItem == null)
                return;

            browseItem.Disabled = true;
            plainTransactionManager.ExecuteInPlainTransaction(() => this.questionnaireBrowseItemWriter.Store(browseItem, browseItem.Id));
        }
    }
}
