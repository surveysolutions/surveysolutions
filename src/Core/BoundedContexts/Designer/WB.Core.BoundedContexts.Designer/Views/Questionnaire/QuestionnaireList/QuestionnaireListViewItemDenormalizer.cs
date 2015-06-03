using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    internal class QuestionnaireListViewItemDenormalizer : BaseDenormalizer, IAtomicEventHandler, 
        IEventHandler<NewQuestionnaireCreated>,
        IEventHandler<QuestionnaireUpdated>,
        IEventHandler<QuestionnaireDeleted>,
        IEventHandler<TemplateImported>,
        IEventHandler<QuestionnaireCloned>,
        IEventHandler<SharedPersonToQuestionnaireAdded>,
        IEventHandler<SharedPersonFromQuestionnaireRemoved>,

        IEventHandler<NewGroupAdded>,
        IEventHandler<GroupCloned>,
        IEventHandler<QuestionnaireItemMoved>,
        IEventHandler<QuestionDeleted>,
        IEventHandler<NewQuestionAdded>,
        IEventHandler<QuestionCloned>,
        IEventHandler<QuestionChanged>,
        IEventHandler<NumericQuestionAdded>,
        IEventHandler<NumericQuestionCloned>,
        IEventHandler<NumericQuestionChanged>,
        IEventHandler<GroupDeleted>,
        IEventHandler<GroupUpdated>,
        IEventHandler<GroupBecameARoster>,
        IEventHandler<RosterChanged>,
        IEventHandler<GroupStoppedBeingARoster>,
        IEventHandler<TextListQuestionAdded>,
        IEventHandler<TextListQuestionCloned>,
        IEventHandler<TextListQuestionChanged>,
        IEventHandler<QRBarcodeQuestionAdded>,
        IEventHandler<QRBarcodeQuestionUpdated>,
        IEventHandler<QRBarcodeQuestionCloned>,
        IEventHandler<MultimediaQuestionUpdated>,
        IEventHandler<StaticTextAdded>,
        IEventHandler<StaticTextUpdated>,
        IEventHandler<StaticTextCloned>,
        IEventHandler<StaticTextDeleted>
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

        public QuestionnaireListViewItemDenormalizer(IReadSideRepositoryWriter<QuestionnaireListViewItem> documentStorage,
            IReadSideRepositoryWriter<AccountDocument> accountStorage)
        {
            this.documentStorage = documentStorage;
            this.accountStorage = accountStorage;
        }

        #endregion

        public override object[] Writers
        {
            get { return new object[] { documentStorage}; }
        }

        public void CleanWritersByEventSource(Guid eventSourceId)
        {
            documentStorage.Remove(eventSourceId);
        }

        public override object[] Readers
        {
            get { return new object[] { accountStorage}; }
        }

        #region Implementation of IEventHandler<in NewQuestionnaireCreated>

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            var item = new QuestionnaireListViewItem(
                evnt.EventSourceId,
                evnt.Payload.Title,
                evnt.Payload.CreationDate,
                evnt.EventTimeStamp,
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
                browseItem.LastEntryDate = evnt.EventTimeStamp;
                this.documentStorage.Store(browseItem, evnt.EventSourceId);
            }
        }

        #endregion

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            var browseItem = this.documentStorage.GetById(evnt.EventSourceId);
            if (browseItem != null)
            {
                browseItem.IsDeleted = true;
                this.documentStorage.Store(browseItem, evnt.EventSourceId);
            }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.CreateAndStoreQuestionnaireListViewItemFromQuestionnaireDocument(evnt.Payload.Source, true, evnt.Payload.Source.CreationDate, evnt.Payload.Source.LastEntryDate);
        }

        public void Handle(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            this.CreateAndStoreQuestionnaireListViewItemFromQuestionnaireDocument(evnt.Payload.QuestionnaireDocument, false, evnt.EventTimeStamp, evnt.EventTimeStamp);
        }

        private void CreateAndStoreQuestionnaireListViewItemFromQuestionnaireDocument(QuestionnaireDocument document, bool shouldPreserveSharedPersons, DateTime creationTime, DateTime updateTime)
        {
            var item = new QuestionnaireListViewItem(
             document.PublicKey,
             document.Title,
             creationTime,
             updateTime,
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

            foreach (var sharedPerson in document.SharedPersons)
            {
                item.SharedPersons.Add(sharedPerson);
            }

            if (shouldPreserveSharedPersons)
            {
                var browseItem = this.documentStorage.GetById(document.PublicKey);
                if (browseItem != null)
                {
                    foreach (var sharedPerson in browseItem.SharedPersons)
                    {
                        if (!item.SharedPersons.Contains(sharedPerson))
                        {
                            item.SharedPersons.Add(sharedPerson);
                        }
                    }
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
                browseItem.LastEntryDate = evnt.EventTimeStamp;
                this.documentStorage.Store(browseItem, evnt.EventSourceId);
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
                browseItem.LastEntryDate = evnt.EventTimeStamp;
                this.documentStorage.Store(browseItem, evnt.EventSourceId);
            }
        }

        private void UpdateLastEntryDate(Guid questionnaireId, DateTime updateDate)
        {
            var browseItem = this.documentStorage.GetById(questionnaireId);
            if (browseItem != null)
            {
                browseItem.LastEntryDate = updateDate;
                this.documentStorage.Store(browseItem, questionnaireId);
            }
        }

        public void Handle(IPublishedEvent<NewGroupAdded> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<GroupCloned> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QuestionDeleted> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<NewQuestionAdded> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QuestionCloned> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<NumericQuestionAdded> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<NumericQuestionCloned> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<NumericQuestionChanged> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<GroupDeleted> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<GroupUpdated> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<GroupBecameARoster> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<RosterChanged> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<GroupStoppedBeingARoster> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<TextListQuestionAdded> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<TextListQuestionCloned> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<TextListQuestionChanged> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<MultimediaQuestionUpdated> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<StaticTextAdded> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<StaticTextUpdated> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<StaticTextCloned> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<StaticTextDeleted> evnt)
        {
            UpdateLastEntryDate(evnt.EventSourceId, evnt.EventTimeStamp);
        }
    }
}
