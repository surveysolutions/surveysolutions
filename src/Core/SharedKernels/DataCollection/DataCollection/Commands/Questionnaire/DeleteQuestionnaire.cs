using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Questionnaire), "DeleteQuestionnaire")]
    public class DeleteQuestionnaire : CommandBase
    {
        public DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? responsibleId)
            : base(questionnaireId)
        {
            this.ResponsibleId = responsibleId;
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
    }
}
