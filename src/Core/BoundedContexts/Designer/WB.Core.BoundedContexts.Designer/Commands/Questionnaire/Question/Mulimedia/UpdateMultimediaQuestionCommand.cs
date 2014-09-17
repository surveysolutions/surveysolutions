using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Mulimedia
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "UpdateMultimediaQuestion")]
    public class UpdateMultimediaQuestionCommand: AbstractUpdateQuestionCommand
    {
        public UpdateMultimediaQuestionCommand(Guid questionnaireId, Guid questionId, string title, string variableName, string variableLabel, bool isMandatory,
            string enablementCondition, string instructions, Guid responsibleId)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions, variableLabel:variableLabel) {}
    }
}
