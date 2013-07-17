using System;
using Main.Core.Events.Questionnaire;
using Main.Core.View.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.BrowseItem
{
    public class QuestionnaireBrowseItemDenormalizer :
        IEventHandler<NewQuestionnaireCreated>,
        IEventHandler<TemplateImported>,
        IEventHandler<QuestionnaireUpdated>,
        IEventHandler<QuestionnaireDeleted>
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
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            var browseItem = this.documentStorage.GetById(evnt.EventSourceId);
            if (browseItem != null)
            {
                browseItem.IsDeleted = true;
            }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var document = evnt.Payload.Source;
            var browseItem = new QuestionnaireBrowseItem(
                document.PublicKey,
                document.Title,
                document.CreationDate,
                document.LastEntryDate,
                document.CreatedBy,
                document.IsPublic);

            //}
            this.documentStorage.Store(browseItem, document.PublicKey);
        }
    }
}
