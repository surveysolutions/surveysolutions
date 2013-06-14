// -----------------------------------------------------------------------
// <copyright file="QuestionnaireBrowseItemDenormalizer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.Views.EventHandler
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Events.Questionnaire;
    using Main.DenormalizerStorage;

    using Ncqrs.Eventing.ServiceModel.Bus;


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireDenormalizer : IEventHandler<NewQuestionnaireCreated>,
                                             IEventHandler<QuestionnaireUpdated>,
                                             IEventHandler<QuestionnaireDeleted>,IEventHandler<TemplateImported>
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
            var document = evnt.Payload.Source;
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
    }
}
