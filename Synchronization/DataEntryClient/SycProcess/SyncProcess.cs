// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcess.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.SycProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
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
    public class SyncProcess
    {
        #region Constants and Fields

        /// <summary>
        /// Gets or sets statistics.
        /// </summary>
        private readonly SyncProcessStatisticsDocument statistics;

        /// <summary>
        /// Gets or sets users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        /// <summary>
        /// The event store.
        /// </summary>
        private readonly IEventStore eventStore;

        /// <summary>
        /// The surveys
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcess"/> class.
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
        public SyncProcess(
            SyncProcessStatisticsDocument statistics,
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys,
            IDenormalizerStorage<UserDocument> users)
        {
            this.statistics = statistics;
            this.surveys = surveys;
            this.users = users;
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
        public UncommittedEventStream[] IncomeEvents { get; set; }

        /// <summary>
        /// Gets SynckProcessId.
        /// </summary>
        public Guid SynckProcessKey { get; private set; }

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
            foreach (UncommittedEventStream uncommittedEventStream in this.IncomeEvents)
            {
                if (!uncommittedEventStream.Any())
                {
                    continue;
                }

                foreach (var uncommittedEvent in uncommittedEventStream)
                {
                    this.ProcessEvent(uncommittedEvent);
                }
            }

            return this.statistics.Statistics;
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
                var document = (uncommittedEvent.Payload as SnapshootLoaded).Template.Payload as CompleteQuestionnaireDocument;
                if (document != null)
                {
                    var item = this.surveys.GetByGuid(document.PublicKey);
                    this.MeasureDifference(item, document);
                }
            }

            if (uncommittedEvent.Payload is NewUserCreated)
            {
                var e = uncommittedEvent.Payload as NewUserCreated;
                var stat = new UserSyncProcessStatistics
                {
                    Type = SynchronizationStatisticType.NewUser,
                    User = new UserLight(e.PublicKey, e.Name),
                };
                this.statistics.Statistics.Add(stat);
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
                if (!uncommittedEventStream.Any())
                {
                    continue;
                }

                this.eventStore.Store(uncommittedEventStream);
                myEventBus.Publish(uncommittedEventStream);
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
            IEnumerable<UncommittedEventStream> uncommitedStreams = this.BuildEventStreams(stream);

            this.IncomeEvents = uncommitedStreams as UncommittedEventStream[] ?? uncommitedStreams.ToArray();
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
            Func<CompleteQuestionnaireDocument, UserSyncProcessStatistics> newStatItem = document => new UserSyncProcessStatistics
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
                var statItem = newStatItem(cq);
                statItem.Type = SynchronizationStatisticType.NewSurvey;
                this.statistics.Statistics.Add(statItem);
                return;
            }

            if (cq.Responsible != null)
            {
                if (item.Responsible == null)
                {
                    var statItem = newStatItem(cq);
                    statItem.Type = SynchronizationStatisticType.NewAssignment;
                    this.statistics.Statistics.Add(statItem);
                }
                else if (item.Responsible != null && item.Responsible.Id != cq.Responsible.Id)
                {
                    var statItem = newStatItem(cq);
                    statItem.Type = SynchronizationStatisticType.AssignmentChanged;
                    statItem.PrevUser = item.Responsible;
                    this.statistics.Statistics.Add(statItem);
                }
            }

            if (item.Status.PublicId != cq.Status.PublicId)
            {
                var statItem = newStatItem(cq);
                statItem.Type = SynchronizationStatisticType.StatusChanged;
                statItem.PrevStatus = item.Status;
                this.statistics.Statistics.Add(statItem);
            }
        }


        #endregion
    }
}