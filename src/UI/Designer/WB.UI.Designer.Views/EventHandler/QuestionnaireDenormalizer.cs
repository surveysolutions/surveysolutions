using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.Views.EventHandler
{
    using System;

    using Main.Core.Events.Questionnaire;

    using Ncqrs.Eventing.ServiceModel.Bus;

    public class QuestionnaireDenormalizer : IEventHandler<NewQuestionnaireCreated>,
                                             IEventHandler<QuestionnaireUpdated>,
                                             IEventHandler<QuestionnaireDeleted>,
                                             IEventHandler<TemplateImported>,
                                             IEventHandler<QuestionnaireCloned>,
                                             IEventHandler<SharedPersonToQuestionnaireAdded>,
                                             IEventHandler<SharedPersonFromQuestionnaireRemoved>
    {
        #region Fields

        /// <summary>
        /// The document storage.
        /// </summary>
        private readonly IReadSideRepositoryWriter<QuestionnaireListViewItem> documentStorage;

        /// <summary>
        /// The account storage.
        /// </summary>
        private readonly IReadSideRepositoryWriter<AccountDocument> accountStorage;

        public QuestionnaireDenormalizer(IReadSideRepositoryWriter<QuestionnaireListViewItem> documentStorage, IReadSideRepositoryWriter<AccountDocument> accountStorage)
        {
            this.documentStorage = documentStorage;
            this.accountStorage = accountStorage;
        }

        #endregion

        #region Implementation of IEventHandler<in NewQuestionnaireCreated>

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            var item = new QuestionnaireListViewItem(
                evnt.EventSourceId,
                evnt.Payload.Title,
                evnt.Payload.CreationDate,
                DateTime.Now,
                evnt.Payload.CreatedBy,
                evnt.Payload.IsPublic);
            if (evnt.Payload.CreatedBy.HasValue)
            {
                var user = this.accountStorage.GetById(evnt.Payload.CreatedBy.Value);
                if (user != null)
                {
                    item.CreatorName = user.UserName;
                }
            }
            this.documentStorage.Store(item, evnt.EventSourceId);
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireUpdated>

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            var browseItem = this.documentStorage.GetById(evnt.EventSourceId);
            if (browseItem != null)
            {
                browseItem.Title = evnt.Payload.Title;
                browseItem.IsPublic = evnt.Payload.IsPublic;
            }
        }

        #endregion

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
            this.CreateAndStoreQuestionnaireListViewItemFromQuestionnaireDocument(evnt.Payload.Source);
        }

        public void Handle(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            this.CreateAndStoreQuestionnaireListViewItemFromQuestionnaireDocument(evnt.Payload.QuestionnaireDocument);
        }

        private void CreateAndStoreQuestionnaireListViewItemFromQuestionnaireDocument(QuestionnaireDocument document)
        {
            var item = new QuestionnaireListViewItem(
                document.PublicKey,
                document.Title,
                document.CreationDate,
                document.LastEntryDate,
                document.CreatedBy,
                document.IsPublic);
            if (document.CreatedBy.HasValue)
            {
                var user = this.accountStorage.GetById(document.CreatedBy.Value);
                if (user != null)
                {
                    item.CreatorName = user.UserName;
                }
            }
            this.documentStorage.Store(item, document.PublicKey);
        }

        public void Handle(IPublishedEvent<SharedPersonToQuestionnaireAdded> evnt)
        {
            var browseItem = this.documentStorage.GetById(evnt.EventSourceId);
            if (browseItem != null)
            {
                if (!browseItem.SharedPersons.Contains(evnt.Payload.PersonId))
                {
                    browseItem.SharedPersons.Add(evnt.Payload.PersonId);
                }
            }
        }

        public void Handle(IPublishedEvent<SharedPersonFromQuestionnaireRemoved> evnt)
        {
            var browseItem = this.documentStorage.GetById(evnt.EventSourceId);
            if (browseItem != null)
            {
                if (browseItem.SharedPersons.Contains(evnt.Payload.PersonId))
                {
                    browseItem.SharedPersons.Remove(evnt.Payload.PersonId);
                }
            }
        }

    }
}
