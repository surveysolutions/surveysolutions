using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Questionnaire), "DeleteQuestionnaire")]
    public class DeleteQuestionnaire : CommandBase
    {
        public DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion)
            : base(questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
    }
}
