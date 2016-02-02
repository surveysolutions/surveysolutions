using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace Main.Core.Entities.SubEntities
{
    public interface IQuestion : IComposite, IConditional
    {
        List<Answer> Answers { get; set; }
        Order? AnswerOrder { get; set; }
        bool Capital { get; set; }
        bool Featured { get; set; }
        string Instructions { get; set; }
        string QuestionText { get; set; }
        QuestionType QuestionType { get; set; }
        QuestionScope QuestionScope { get; set; }
        string StataExportCaption { get; set; }
        string VariableLabel { get; set; }
        string ValidationExpression { get; set; }
        string ValidationMessage { get; set; }
        Guid? LinkedToQuestionId { get; set; }
        Guid? CascadeFromQuestionId { get; set; }
        bool? IsFilteredCombobox { get; set; }
        List<ValidationCondition> ValidationConditions { get; set; }
        void AddAnswer(Answer answer);
        IEnumerable<string> GetVariablesUsedInTitle();
    }
}