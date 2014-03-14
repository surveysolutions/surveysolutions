using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.BoundedContexts.Headquarters.Commands.Survey.Base
{
    public abstract class SurveyCommand : CommandBase
    {
        [AggregateRootId]
        public Guid SurveyId { get; private set; }

        protected SurveyCommand(Guid surveyId)
        {
            this.SurveyId = surveyId;
        }
    }
}