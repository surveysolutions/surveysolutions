using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    [MapsToAggregateRootMethodOrConstructor(typeof(Implementation.Aggregates.Questionnaire), "RegisterPlainQuestionnaire")]
    public class RegisterPlainQuestionnaire : CommandBase
    {
        [AggregateRootId]
        public Guid QuestionnaireId { get; private set; }

        public Guid Id { get; private set; }
        public long Version { get; private set; }

        public RegisterPlainQuestionnaire(Guid questionnaireId, long version)
            : base(questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
            this.Id = questionnaireId;
            this.Version = version;
        }
    }
}