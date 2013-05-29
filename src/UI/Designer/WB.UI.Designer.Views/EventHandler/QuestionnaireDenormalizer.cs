// -----------------------------------------------------------------------
// <copyright file="QuestionnaireBrowseItemDenormalizer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.Views.EventHandler
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Events.Questionnaire;
    using Main.DenormalizerStorage;

    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Restoring.EventStapshoot;


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireDenormalizer : IEventHandler<NewQuestionnaireCreated>,
                                             IEventHandler<SnapshootLoaded>,
                                             IEventHandler<QuestionnaireUpdated>,
                                             IEventHandler<QuestionnaireDeleted>,IEventHandler<TemplateImported>
    {
        #region Fields

        /// <summary>
        /// The document storage.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireListViewItem> documentStorage;

        /// <summary>
        /// The account storage.
        /// </summary>
        private readonly IDenormalizerStorage<AccountDocument> accountStorage; 

        public QuestionnaireDenormalizer(IDenormalizerStorage<QuestionnaireListViewItem> documentStorage, IDenormalizerStorage<AccountDocument> accountStorage)
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
                evnt.Payload.CreatedBy);
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

        #region Implementation of IEventHandler<in SnapshootLoaded>

        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as QuestionnaireDocument;
            if (document == null)
            {
                return;
            }

            /* // getting all featured questions
             var item = this.documentStorage.GetByGuid(document.PublicKey);
             if (item == null)
             {*/
            var item = new QuestionnaireListViewItem(
                document.PublicKey,
                document.Title,
                document.CreationDate,
                document.LastEntryDate,
                document.CreatedBy);
            if (document.CreatedBy.HasValue)
            {
                var user = this.accountStorage.GetById(document.CreatedBy.Value);
                if (user != null)
                {
                    item.CreatorName = user.UserName;
                }
            }
            this.documentStorage.Store(item, document.PublicKey);
            // }
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireUpdated>

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            var browseItem = this.documentStorage.GetById(evnt.EventSourceId);
            if (browseItem != null)
            {
                browseItem.Title = evnt.Payload.Title;
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
            var document = evnt.Payload.Source;
            var item = new QuestionnaireListViewItem(
                document.PublicKey,
                document.Title,
                document.CreationDate,
                document.LastEntryDate,
                document.CreatedBy);
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
    }
}
