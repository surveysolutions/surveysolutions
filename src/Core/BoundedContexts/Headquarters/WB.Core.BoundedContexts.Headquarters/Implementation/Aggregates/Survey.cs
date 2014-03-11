using System;
using Ncqrs.Domain;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Exceptions;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates
{
    internal class Survey : AggregateRootMappedByConvention
    {
        #region State

        private void Apply(NewSurveyStarted @event) {}

        #endregion

        /// <remarks>Is used to restore aggregate from event stream.</remarks>
        public Survey() { }

        public Survey(Guid id, string name)
            : base(id)
        {
            this.ThrowIfSurveyNameIsEmpty(name);

            this.ApplyEvent(new NewSurveyStarted(name));
        }

        #region Invariants

        private void ThrowIfSurveyNameIsEmpty(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new SurveyException("Survey name cannot be empty.");
        }

        #endregion
    }
}