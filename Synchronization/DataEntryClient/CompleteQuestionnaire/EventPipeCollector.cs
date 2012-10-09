// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventPipeCollector.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Events;

    using SynchronizationMessages.CompleteQuestionnaire;

    /// <summary>
    /// Event collector for usb syncronization
    /// </summary>
    public class EventPipeCollector : IEventPipe
    {
        #region Constants and Fields

        /// <summary>
        /// List of event streams
        /// </summary>
        private readonly List<AggregateRootEvent> list;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPipeCollector"/> class.
        /// </summary>
        public EventPipeCollector()
        {
            this.list = new List<AggregateRootEvent>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// Error code
        /// </returns>
        public ErrorCodes Process(EventSyncMessage request)
        {
            try
            {
                this.list.AddRange(request.Command);
                return ErrorCodes.None;
            }
            catch (Exception)
            {
                return ErrorCodes.Fail;
            }
        }

        #endregion

        /// <summary>
        /// Gets list of events
        /// </summary>
        /// <returns>
        /// Return list of events
        /// </returns>
        public IEnumerable<AggregateRootEvent> GetEventList()
        {
            return this.list;
        }
    }
}