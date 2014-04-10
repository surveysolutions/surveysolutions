using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.QRBarcode
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "AddQRBarcodeQuestion")]
    public class AddQRBarcodeQuestionCommand : AbstractAddQuestionCommand
    {
        public AddQRBarcodeQuestionCommand(Guid questionnaireId, Guid questionId, string title, string variableName, bool isMandatory,
            string condition, string instructions, Guid responsibleId, Guid parentGroupId)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, condition: condition, instructions: instructions,
                parentGroupId: parentGroupId) {}
    }
}
