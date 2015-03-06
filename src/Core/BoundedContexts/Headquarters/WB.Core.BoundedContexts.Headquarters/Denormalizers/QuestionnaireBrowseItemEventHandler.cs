using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Denormalizers
{
    public class QuestionnaireBrowseItemEventHandler : BaseDenormalizer, IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted>,
        IEventHandler<QuestionnaireDisabled>
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IReadSideRepositoryWriter<QuestionnaireBrowseItem> readsideRepositoryWriter;

        public QuestionnaireBrowseItemEventHandler(PostgreReadSideRepository<QuestionnaireBrowseItem> readsideRepositoryWriter, IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.readsideRepositoryWriter = readsideRepositoryWriter;
        }

        public override object[] Writers
        {
            get { return new[] { this.readsideRepositoryWriter }; }
        }

        private  QuestionnaireBrowseItem CreateBrowseItem(long version, QuestionnaireDocument questionnaireDocument, bool allowCensusMode)
        {
            return new QuestionnaireBrowseItem(questionnaireDocument, version, allowCensusMode);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            var view = this.CreateBrowseItem(version, questionnaireDocument, evnt.Payload.AllowCensusMode);
            this.readsideRepositoryWriter.Store(view, view.Id);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            var view = this.CreateBrowseItem(version, questionnaireDocument, evnt.Payload.AllowCensusMode);
            this.readsideRepositoryWriter.Store(view, view.Id);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.readsideRepositoryWriter.AsVersioned().Remove(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion);
        }

        public void Handle(IPublishedEvent<QuestionnaireDisabled> evnt)
        {
            var versionedWrapper = this.readsideRepositoryWriter.AsVersioned();
            var versionedId = versionedWrapper.GetVersionedId(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion);
            var browseItem = this.readsideRepositoryWriter.GetById(versionedId);
            if (browseItem == null)
                return;

            browseItem.Disabled = true;
            this.readsideRepositoryWriter.Store(browseItem, browseItem.Id);
        }
    }
}
