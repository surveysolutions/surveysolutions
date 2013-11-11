using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.BrowseItem
{
    internal class QuestionnaireBrowseItemDenormalizer :
        IEventHandler<NewQuestionnaireCreated>,
        IEventHandler<TemplateImported>,
        IEventHandler<QuestionnaireUpdated>,
        IEventHandler<QuestionnaireDeleted>,
        IEventHandler<QuestionnaireCloned>, IEventHandler
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireBrowseItem> documentStorage;

        public QuestionnaireBrowseItemDenormalizer(IReadSideRepositoryWriter<QuestionnaireBrowseItem> documentStorage)
        {
            this.documentStorage = documentStorage;
        }

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            this.documentStorage.Store(
                new QuestionnaireBrowseItem(
                    evnt.EventSourceId,
                    evnt.Payload.Title,
                    evnt.Payload.CreationDate,
                    DateTime.Now,
                    evnt.Payload.CreatedBy,
                    evnt.Payload.IsPublic),
                evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            var browseItem = this.documentStorage.GetById(evnt.EventSourceId);
            if (browseItem != null)
            {
                browseItem.Title = evnt.Payload.Title;
                browseItem.IsPublic = evnt.Payload.IsPublic;
            }
            this.documentStorage.Store(browseItem, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            var browseItem = this.documentStorage.GetById(evnt.EventSourceId);
            if (browseItem != null)
            {
                browseItem.IsDeleted = true;
            }
            this.documentStorage.Store(browseItem, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.StoreNewBrowseItemFromQuestionnaireDocument(evnt.Payload.Source);
        }

        public void Handle(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            this.StoreNewBrowseItemFromQuestionnaireDocument(evnt.Payload.QuestionnaireDocument);
        }

        private void StoreNewBrowseItemFromQuestionnaireDocument(QuestionnaireDocument document)
        {
            var browseItem = new QuestionnaireBrowseItem(
                document.PublicKey,
                document.Title,
                document.CreationDate,
                document.LastEntryDate,
                document.CreatedBy,
                document.IsPublic);

            this.documentStorage.Store(browseItem, document.PublicKey);
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
            get { return new Type[] { typeof(QuestionnaireBrowseItem) }; }
        }
    }
}
