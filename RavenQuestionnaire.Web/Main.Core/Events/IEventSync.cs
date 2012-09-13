// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEventSync.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IEventSync type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events
{
    using System.Collections.Generic;

    /// <summary>
    /// The EventSync interface.
    /// </summary>
    public interface IEventSync
    {
        #region Public Methods and Operators

        /// <summary>
        /// Returns list of ALL events grouped by aggregate root, please use very carefully
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; RavenQuestionnaire.Core.Events.AggregateRootEvent].
        /// </returns>
        IEnumerable<AggregateRootEvent> ReadEvents();

        /// <summary>
        /// The write events.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        void WriteEvents(IEnumerable<AggregateRootEvent> stream);

        #endregion

        //// AggregateRootEventStream ReadEventStream(Guid eventSurceId);
        //// IEnumerable<AggregateRootEventStream> ReadCompleteQuestionare();
    }
}