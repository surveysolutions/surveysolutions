// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcesor.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Commands.Synchronization;
using Ncqrs.Commanding.ServiceModel;

namespace Main.Synchronization.SycProcessRepository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.Events.Questionnaire.Completed;
    using Main.Core.Events.User;
    using Main.Core.Utility;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.SyncProcess;
    using Main.DenormalizerStorage;

    using Ncqrs;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Eventing.Storage;
    using Ncqrs.Restoring.EventStapshoot;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessor : ISyncProcessor
    {
        #region Fields

        /// <summary>
        /// The event store.
        /// </summary>
        private readonly IEventStore eventStore;

        /// <summary>
        /// Gets or sets statistics.
        /// </summary>
        private readonly SyncProcessStatisticsDocument statistics;

        /// <summary>
        /// The surveys
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        /// <summary>
        /// Gets or sets users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessor"/> class.
        /// </summary>
        /// <param name="statistics">
        /// The statistics.
        /// </param>
        /// <param name="surveys">
        /// The surveys.
        /// </param>
        /// <param name="users">
        /// The users.
        /// </param>
        /// <exception cref="Exception">
        /// Some exception
        /// </exception>
        public SyncProcessor(
            SyncProcessStatisticsDocument statistics, 
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys, 
            IDenormalizerStorage<UserDocument> users)
        {
            this.statistics = statistics;
            this.surveys = surveys;
            this.users = users;

            this.IncomeEvents = new List<UncommittedEventStream>();

            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
            if (this.eventStore == null)
            {
                throw new Exception("IEventStore is not properly initialized.");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets IncomeEvents.
        /// </summary>
        private List<UncommittedEventStream> IncomeEvents { get; set; }

        /// <summary>
        /// Gets the sync process key.
        /// </summary>
        public Guid SyncProcessKey { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Calculate statistics
        /// </summary>
        /// <returns>
        /// List of UserSyncProcessStatistics
        /// </returns>
        public List<UserSyncProcessStatistics> CalculateStatistics()
        {
            foreach (UncommittedEvent uncommittedEvent in this.IncomeEvents.SelectMany(uncommittedEventStream => uncommittedEventStream))
            {
                this.ProcessEvent(uncommittedEvent);
            }

            return this.statistics.Statistics;
        }

        public void PostProcess()
        {
            if (this.statistics != null)
            {
                var statisticValues = CalculateStatistics();

                NcqrsEnvironment.Get<ICommandService>()
                                .Execute(new PushStatisticsCommand(this.statistics.PublicKey, statisticValues));
            }

        }

        /// <summary>
        /// The commit.
        /// </summary>
        public void Commit()
        {
            var myEventBus = NcqrsEnvironment.Get<IEventBus>();
            if (myEventBus == null)
            {
                throw new Exception("IEventBus is not properly initialized.");
            }

            foreach (UncommittedEventStream uncommittedEventStream in this.IncomeEvents)
            {
                this.eventStore.Store(uncommittedEventStream);
                myEventBus.Publish(uncommittedEventStream.Select(e => e as IPublishableEvent));
            }
        }

        /// <summary>
        /// The merge events.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        public void Merge(IEnumerable<AggregateRootEvent> stream)
        {
            if (stream != null)
            {
                this.IncomeEvents.AddRange(this.BuildEventStreams(stream));
            }

            // check for events with null Payload
            if (this.IncomeEvents.SelectMany(eventStream => eventStream).Any(c => c.Payload == null))
            {
                throw new Exception("Event is wrong");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The build event streams.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// List of UncommittedEventStream
        /// </returns>
        protected IEnumerable<UncommittedEventStream> BuildEventStreams(IEnumerable<AggregateRootEvent> stream)
        {
            return
                stream.GroupBy(x => x.EventSourceId).Select(
                    g => g.CreateUncommittedEventStream(this.eventStore.ReadFrom(g.Key, long.MinValue, long.MaxValue)));

            // foreach (IGrouping<Guid, AggregateRootEvent> g in stream.GroupBy(x => x.EventSourceId))
            // {
            // yield return g.CreateUncommittedEventStream(this.eventStore.ReadFrom(g.Key, long.MinValue, long.MaxValue));
            // }
        }

       

        /// <summary>
        /// Get difference between already stored item and new-come CQ document and generates statistics
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="cq">
        /// The cq.
        /// </param>
        private void MeasureDifference(CompleteQuestionnaireBrowseItem item, CompleteQuestionnaireDocument cq)
        {
            Func<CompleteQuestionnaireDocument, UserSyncProcessStatistics> newStatItem =
                document =>
                new UserSyncProcessStatistics
                    {
                        Type = SynchronizationStatisticType.NewSurvey, 
                        User = document.Responsible, 
                        TemplateId = document.TemplateId, 
                        Title = document.Title, 
                        SurveyId = document.PublicKey, 
                        Status = document.Status
                    };

            if (item == null)
            {
                UserSyncProcessStatistics statItem = newStatItem(cq);
                statItem.Type = SynchronizationStatisticType.NewSurvey;
                this.statistics.Statistics.Add(statItem);
                return;
            }

            if (cq.Responsible != null)
            {
                if (item.Responsible == null)
                {
                    UserSyncProcessStatistics statItem = newStatItem(cq);
                    statItem.Type = SynchronizationStatisticType.NewAssignment;
                    this.statistics.Statistics.Add(statItem);
                }
                else if (item.Responsible != null && item.Responsible.Id != cq.Responsible.Id)
                {
                    UserSyncProcessStatistics statItem = newStatItem(cq);
                    statItem.Type = SynchronizationStatisticType.AssignmentChanged;
                    statItem.PrevUser = item.Responsible;
                    this.statistics.Statistics.Add(statItem);
                }
            }

            if (item.Status.PublicId != cq.Status.PublicId)
            {
                UserSyncProcessStatistics statItem = newStatItem(cq);
                statItem.Type = SynchronizationStatisticType.StatusChanged;
                statItem.PrevStatus = item.Status;
                this.statistics.Statistics.Add(statItem);
            }
        }

        /// <summary>
        /// Process one uncommited event
        /// </summary>
        /// <param name="uncommittedEvent">
        /// The uncommitted event.
        /// </param>
        private void ProcessEvent(UncommittedEvent uncommittedEvent)
        {
            if (uncommittedEvent == null || uncommittedEvent.Payload == null)
            {
                return;
            }

            if (uncommittedEvent.Payload is SnapshootLoaded)
            {
                var document =
                    (uncommittedEvent.Payload as SnapshootLoaded).Template.Payload as CompleteQuestionnaireDocument;
                if (document != null)
                {
                    CompleteQuestionnaireBrowseItem item = this.surveys.GetById(document.PublicKey);
                    this.MeasureDifference(item, document);
                }
            }

            if (uncommittedEvent.Payload is NewUserCreated)
            {
                var e = uncommittedEvent.Payload as NewUserCreated;
                var stat = new UserSyncProcessStatistics
                    {
                       Type = SynchronizationStatisticType.NewUser, User = new UserLight(e.PublicKey, e.Name), 
                    };
                this.statistics.Statistics.Add(stat);
            }

            if (uncommittedEvent.Payload is QuestionnaireStatusChanged)
            {
                var e = uncommittedEvent.Payload as QuestionnaireStatusChanged;
                CompleteQuestionnaireBrowseItem document = this.surveys.GetById(e.CompletedQuestionnaireId);

                var stat = new UserSyncProcessStatistics
                    {
                        Type = SynchronizationStatisticType.StatusChanged, 
                        User = e.Responsible, 
                        SurveyId = e.CompletedQuestionnaireId, 
                        Status = e.Status, 
                        PrevStatus = e.PreviousStatus
                    };

                if (document != null)
                {
                    stat.TemplateId = document.TemplateId;
                    stat.Title = document.QuestionnaireTitle;
                }

                this.statistics.Statistics.Add(stat);
            }
        }

        #endregion
    }
}