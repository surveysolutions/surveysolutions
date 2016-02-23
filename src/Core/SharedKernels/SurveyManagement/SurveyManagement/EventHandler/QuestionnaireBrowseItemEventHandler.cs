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
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> readsideRepositoryWriter;
        private readonly IPlainTransactionManager plainTransactionManager;

        public QuestionnaireBrowseItemEventHandler(
            IPlainStorageAccessor<QuestionnaireBrowseItem> readsideRepositoryWriter, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IPlainTransactionManager plainTransactionManager)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainTransactionManager = plainTransactionManager;
            this.readsideRepositoryWriter = readsideRepositoryWriter;
        }

        public override object[] Writers => new object[0];

        private  QuestionnaireBrowseItem CreateBrowseItem(long version, QuestionnaireDocument questionnaireDocument, bool allowCensusMode)
        {
            return new QuestionnaireBrowseItem(questionnaireDocument, version, allowCensusMode);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(evnt.EventSourceId, version);

            var view = this.CreateBrowseItem(version, questionnaireDocument, evnt.Payload.AllowCensusMode);
            plainTransactionManager.ExecuteInPlainTransaction(() => this.readsideRepositoryWriter.Store(view, view.Id));
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            var view = this.CreateBrowseItem(version, questionnaireDocument, evnt.Payload.AllowCensusMode);
            plainTransactionManager.ExecuteInPlainTransaction(() => this.readsideRepositoryWriter.Store(view, view.Id));
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            var versionedWrapper = this.readsideRepositoryWriter.AsVersioned();
            var browseItem = plainTransactionManager.ExecuteInPlainTransaction(() => versionedWrapper.Get(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion));
            if (browseItem == null)
                return;

            browseItem.IsDeleted = true;
            plainTransactionManager.ExecuteInPlainTransaction(() => this.readsideRepositoryWriter.Store(browseItem, browseItem.Id));
        }

        public void Handle(IPublishedEvent<QuestionnaireDisabled> evnt)
        {
            var versionedWrapper = this.readsideRepositoryWriter.AsVersioned();
            var browseItem = plainTransactionManager.ExecuteInPlainTransaction(() => versionedWrapper.Get(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion));
            if (browseItem == null)
                return;

            browseItem.Disabled = true;
            plainTransactionManager.ExecuteInPlainTransaction(() => this.readsideRepositoryWriter.Store(browseItem, browseItem.Id));
        }
    }
}
