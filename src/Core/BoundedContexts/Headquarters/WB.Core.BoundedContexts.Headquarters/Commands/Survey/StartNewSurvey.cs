using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Headquarters.Commands.Survey.Base;

namespace WB.Core.BoundedContexts.Headquarters.Commands.Survey
{
    [MapsToAggregateRootConstructor(typeof(Implementation.Aggregates.Survey))]
    public class StartNewSurvey : SurveyCommand
    {
        /// <remarks>NCQRS will not resolve ctor if this parameter is nor supplied :\.</remarks>
        public Guid Id { get { return this.SurveyId; } }

        public string Name { get; private set; }

        public StartNewSurvey(Guid surveyId, string name)
            : base(surveyId)
        {
            this.Name = name;
        }
    }
}