// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventPipeCollector.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient
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

        /// <summary>
        /// Chunked list of event streams
        /// </summary>
        private readonly List<IEnumerable<AggregateRootEvent>> chunkList;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPipeCollector"/> class.
        /// </summary>
        public EventPipeCollector()
        {
            this.list = new List<AggregateRootEvent>();
            this.chunkList = new List<IEnumerable<AggregateRootEvent>>();
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
                this.chunkList.Add(request.Command);
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

        /// <summary>
        /// Gets chunked list of events
        /// </summary>
        /// <returns>
        /// Return chunked list of events
        /// </returns>
        public IEnumerable<IEnumerable<AggregateRootEvent>> GetChunkedList()
        {
            return this.chunkList;
        }
    }
}