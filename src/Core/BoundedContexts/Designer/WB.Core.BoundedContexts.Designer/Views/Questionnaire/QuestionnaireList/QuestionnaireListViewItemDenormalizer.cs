﻿using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    internal class QuestionnaireListViewItemDenormalizer : BaseDenormalizer,IAtomicEventHandler, IEventHandler<NewQuestionnaireCreated>,
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

        private readonly IQuestionnaireDocumentUpgrader upgrader;

        public QuestionnaireListViewItemDenormalizer(IReadSideRepositoryWriter<QuestionnaireListViewItem> documentStorage,
            IReadSideRepositoryWriter<AccountDocument> accountStorage, IQuestionnaireDocumentUpgrader upgrader)
        {
            this.documentStorage = documentStorage;
            this.accountStorage = accountStorage;
            this.upgrader = upgrader;
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
            this.CreateAndStoreQuestionnaireListViewItemFromQuestionnaireDocument(evnt.Payload.Source, true);
        }

        public void Handle(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            this.CreateAndStoreQuestionnaireListViewItemFromQuestionnaireDocument(evnt.Payload.QuestionnaireDocument, false);
        }

        private void CreateAndStoreQuestionnaireListViewItemFromQuestionnaireDocument(QuestionnaireDocument document, bool shouldPreserveSharedPersons)
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

            item.SharedPersons.AddRange(document.SharedPersons);

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

                this.documentStorage.Store(browseItem, evnt.EventSourceId);
            }
        }
    }
}
