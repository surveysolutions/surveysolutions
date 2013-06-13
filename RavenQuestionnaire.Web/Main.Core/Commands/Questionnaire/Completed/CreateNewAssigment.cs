using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Questionnaire.Completed
{
    [MapsToAggregateRootMethodOrConstructor(typeof(CompleteQuestionnaireAR), "CreateNewAssigment")]
    public class CreateNewAssigment : CommandBase
    {
        public CreateNewAssigment(CompleteQuestionnaireDocument source)
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
