using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Headquarters.Commands.Survey.Base;

namespace WB.Core.BoundedContexts.Headquarters.Commands.Survey
{
    [MapsToAggregateRootConstructor(typeof(Implementation.Aggregates.Survey))]
    public class StartNewSurvey : SurveyCommand
    {
        public string Name { get; private set; }

        public StartNewSurvey(Guid surveyId, string name)
            : base(surveyId)
        {
            this.Name = name;
        }
    }
}