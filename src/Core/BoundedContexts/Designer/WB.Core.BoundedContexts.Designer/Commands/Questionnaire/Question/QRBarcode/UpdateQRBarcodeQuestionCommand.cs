using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.QRBarcode
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "UpdateQRBarcodeQuestion")]
    public class UpdateQRBarcodeQuestionCommand : AbstractUpdateQuestionCommand
    {
        public UpdateQRBarcodeQuestionCommand(Guid questionnaireId, Guid questionId, string title, string variableName, string variableLabel, bool isMandatory,
            string enablementCondition, string instructions, Guid responsibleId)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions, variableLabel:variableLabel) {}
    }
}
