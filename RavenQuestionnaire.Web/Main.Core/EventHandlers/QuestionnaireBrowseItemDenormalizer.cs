﻿// -----------------------------------------------------------------------
// <copyright file="QuestionnaireBrowseItemDenormalizer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Main.Core.View.Questionnaire;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Restoring.EventStapshoot;

namespace Main.Core.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireBrowseItemDenormalizer : IEventHandler<NewQuestionnaireCreated>,
                                                       IEventHandler<SnapshootLoaded>,
                                                       IEventHandler<QuestionnaireUpdated>
    {
        #region Fields

        /// <summary>
        /// The document storage.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireBrowseItem> documentStorage;

        public QuestionnaireBrowseItemDenormalizer(IDenormalizerStorage<QuestionnaireBrowseItem> documentStorage)
        {
            this.documentStorage = documentStorage;
        }

        #endregion

        #region Implementation of IEventHandler<in NewQuestionnaireCreated>

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            this.documentStorage.Store(
                new QuestionnaireBrowseItem(evnt.EventSourceId, evnt.Payload.Title, evnt.Payload.CreationDate, DateTime.Now),
                evnt.EventSourceId);
        }

        #endregion

        #region Implementation of IEventHandler<in SnapshootLoaded>

        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as QuestionnaireDocument;
            if (document == null)
                return;
            // getting all featured questions
            var browseItem = this.documentStorage.GetByGuid(document.PublicKey);
            if (browseItem == null)
            {
                browseItem = new QuestionnaireBrowseItem(document.PublicKey, document.Title, document.CreationDate, document.LastEntryDate);
                this.documentStorage.Store(browseItem, document.PublicKey);
            }
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireUpdated>

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            var browseItem = this.documentStorage.GetByGuid(evnt.EventSourceId);
            if (browseItem != null)
            {
                browseItem.Title = evnt.Payload.Title;
            }
        }

        #endregion

    }
}
