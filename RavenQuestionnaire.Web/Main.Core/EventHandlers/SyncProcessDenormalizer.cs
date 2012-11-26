// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.EventHandlers
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Questionnaire;
    using Main.Core.Events.Questionnaire.Completed;
    using Main.Core.Events.Synchronization;
    using Main.Core.Events.User;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.SyncProcess;
    using Main.DenormalizerStorage;

    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Restoring.EventStapshoot;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessDenormalizer : IEventHandler<NewSynchronizationProcessCreated>,
                                           IEventHandler<ProcessEnded>,
                                           IEventHandler<NewCompleteQuestionnaireCreated>,
                                           IEventHandler<SnapshootLoaded>,
                                           IEventHandler<CompleteQuestionnaireDeleted>,
                                           IEventHandler<QuestionnaireAssignmentChanged>,
                                           IEventHandler<QuestionnaireStatusChanged>,
                                           IEventHandler<NewQuestionnaireCreated>,
                                           IEventHandler<NewUserCreated>
    {
        #region Constants and Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<SyncProcessStatisticsDocument> docs;

        /// <summary>
        /// The surveys
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessDenormalizer"/> class.
        /// </summary>
        /// <param name="docs">
        /// The docs.
        /// </param>
        /// <param name="documentItemStore">
        /// The document Item Store.
        /// </param>
        public SyncProcessDenormalizer(IDenormalizerStorage<SyncProcessStatisticsDocument> docs, IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore)
        {
            this.docs = docs;
            this.documentItemStore = documentItemStore;
        }

        #endregion

        #region Public Methods and Operators


        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            SyncProcessStatisticsDocument item = this.docs.GetByGuid(Guid.Empty);
            if (item == null)
            {
                return;
            }
            var stat = new UserSyncProcessStatistics
            {
                Type = SynchronizationStatisticType.NewUser,
                User = new UserLight(evnt.Payload.PublicKey, evnt.Payload.Name),
            };
            item.Statistics.Add(stat);
        }

        /// <summary>
        /// Start of sync process
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewSynchronizationProcessCreated> evnt)
        {
            this.docs.Store(
                new SyncProcessStatisticsDocument
                    {
                        SyncKey = evnt.Payload.ProcessGuid,
                        SyncType = evnt.Payload.SynckType
                    },
                Guid.Empty);
        }

        /// <summary>
        /// Ends sync process
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<ProcessEnded> evnt)
        {
            SyncProcessStatisticsDocument item = this.docs.GetByGuid(Guid.Empty);
            if (item == null)
            {
                return;
            }

            item.IsEnded = true;
            item.EndDate = DateTime.Now;
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            SyncProcessStatisticsDocument item = this.docs.GetByGuid(Guid.Empty);
            if (item == null || item.IsEnded)
            {
                return;
            }

            var cq = evnt.Payload.Questionnaire;
            var stat = new UserSyncProcessStatistics
                {
                    Type = SynchronizationStatisticType.NewSurvey,
                    User = cq.Responsible,
                    TemplateId = cq.TemplateId,
                    Title = cq.Title,
                    SurveyId = cq.PublicKey,
                    Status = cq.Status
                };
            item.Statistics.Add(stat);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            SyncProcessStatisticsDocument item = this.docs.GetByGuid(Guid.Empty);
            if (item == null || item.IsEnded)
            {
                return;
            }

            var q = evnt.Payload;
            var stat = new UserSyncProcessStatistics
            {
                Type = SynchronizationStatisticType.NewQuestionnaire,
                TemplateId = q.PublicKey,
                Title = q.Title
            };
            item.Statistics.Add(stat);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            SyncProcessStatisticsDocument item = this.docs.GetByGuid(Guid.Empty);
            if (item == null || item.IsEnded)
            {
                return;
            }

            var document = evnt.Payload.Template.Payload as QuestionnaireDocument;
            if (document != null)
            {
                var stat = new UserSyncProcessStatistics
                {
                    Type = SynchronizationStatisticType.NewQuestionnaire,
                    TemplateId = document.PublicKey,
                    Title = document.Title
                };
                item.Statistics.Add(stat);
                return;
            }

            var cq = evnt.Payload.Template.Payload as CompleteQuestionnaireDocument;
            if (cq == null)
            {
                return;
            }

            {
                var doc = this.documentItemStore.GetByGuid(cq.PublicKey);

                var stat = new UserSyncProcessStatistics
                    {
                        User = cq.Responsible,
                        TemplateId = cq.TemplateId,
                        Title = cq.Title,
                        SurveyId = cq.PublicKey,
                        Status = cq.Status
                    };

                if (doc == null)
                {
                    stat.Type = SynchronizationStatisticType.NewSurvey;
                }
                else if (cq.Status.PublicId != doc.Status.PublicId)
                {
                    stat.Type = SynchronizationStatisticType.StatusChanged;
                }
                else
                {
                    if (doc.Responsible == null)
                    {
                        stat.Type = SynchronizationStatisticType.NewAssignment;
                    }
                    else if (cq.Responsible.Id != doc.Responsible.Id)
                    {
                        stat.Type = SynchronizationStatisticType.AssignmentChanged;
                    }
                }

                item.Statistics.Add(stat);
            }
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            SyncProcessStatisticsDocument item = this.docs.GetByGuid(Guid.Empty);
            if (item == null || item.IsEnded)
            {
                return;
            }
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            SyncProcessStatisticsDocument item = this.docs.GetByGuid(Guid.Empty);
            var doc = this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            if (item == null || item.IsEnded)
            {
                return;
            }

            var stat = new UserSyncProcessStatistics
                {
                    Type = SynchronizationStatisticType.AssignmentChanged,
                    User = evnt.Payload.Responsible,
                    SurveyId = evnt.Payload.CompletedQuestionnaireId,
                    PrevUser = evnt.Payload.PreviousResponsible,
                    Status = doc.Status,
                    Title = doc.QuestionnaireTitle,
                    TemplateId = doc.TemplateId
                };

            item.Statistics.Add(stat);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            SyncProcessStatisticsDocument item = this.docs.GetByGuid(Guid.Empty);
            var doc = this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            if (item == null || item.IsEnded)
            {
                return;
            }

            var stat = new UserSyncProcessStatistics
            {
                Type = SynchronizationStatisticType.StatusChanged,
                User = doc.Responsible,
                SurveyId = evnt.Payload.CompletedQuestionnaireId,
                PrevStatus = evnt.Payload.PreviousStatus,
                Status = evnt.Payload.Status,
                Title = doc.QuestionnaireTitle,
                TemplateId = doc.TemplateId
            };

            item.Statistics.Add(stat);
        }

        #endregion
    }
}