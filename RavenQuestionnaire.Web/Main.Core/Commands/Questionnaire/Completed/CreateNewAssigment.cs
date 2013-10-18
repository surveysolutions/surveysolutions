using System;
using Main.Core.Documents;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Questionnaire.Completed
{
    [MapsToAggregateRootMethod(typeof(_CompleteQuestionnaireAR), "CreateNewAssigment")]
    public class _CreateNewAssigment : CommandBase
    {
        public _CreateNewAssigment(CompleteQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            Source = source;
            QuestionnaireId = source.PublicKey;
        }
        public CompleteQuestionnaireDocument Source { get; private set; }

        [AggregateRootId]
        public Guid QuestionnaireId { get; private set; }
    }
}
