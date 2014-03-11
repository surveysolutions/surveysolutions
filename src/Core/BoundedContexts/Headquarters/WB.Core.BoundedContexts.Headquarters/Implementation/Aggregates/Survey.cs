using System;
using Ncqrs.Domain;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates
{
    internal class Survey : AggregateRootMappedByConvention
    {
        #region State

        private void Apply(NewSurveyStarted @event) {}

        #endregion

        /// <remarks>Is used to restore aggregate from event stream.</remarks>
        public Survey() { }

        public Survey(Guid id)
            : base(id)
        {
            this.ApplyEvent(new NewSurveyStarted());
        }
    }
}