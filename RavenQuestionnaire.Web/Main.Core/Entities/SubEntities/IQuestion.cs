using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities
{
    /// <summary>
    /// The Question interface.
    /// </summary>
    public interface IQuestion : IComposite, IConditional
    {
        List<IAnswer> Answers { get; set; }
        Order AnswerOrder { get; set; }
        bool Capital { get; set; }
        List<Image> Cards { get; set; }
        bool Featured { get; set; }
        string Instructions { get; set; }
        bool Mandatory { get; set; }
        string QuestionText { get; set; }
        QuestionType QuestionType { get; set; }
        QuestionScope QuestionScope { get; set; } 
        string StataExportCaption { get; set; }
        string ValidationExpression { get; set; }
        string ValidationMessage { get; set; }

        List<Guid> QuestionsWithDependentEnablementConditions { get; set; }
        List<Guid> GroupsWithDependentEnablementConditions { get; set; }
        List<Guid> QuestionsWhichCustomValidationDependsOnQuestion { get; set; }

        List<Guid> QuestionIdsInvolvedInCustomEnablementConditionOfQuestion { get; set; }

        List<Guid> QuestionIdsInvolvedInCustomValidationOfQuestion { get; set; }

        Guid? LinkedToQuestionId { get; set; }

        void AddAnswer(IAnswer answer);
        IEnumerable<string> GetVariablesUsedInTitle();

        [Obsolete("please use QuestionIdsInvolvedInCustomEnablementConditionOfQuestion instead")]
        List<QuestionIdAndVariableName> QuestionsInvolvedInCustomEnablementConditionOfQuestion { get; set; }
        [Obsolete("please use QuestionsWhichCustomValidationDependsOnQuestion instead")]
        List<QuestionIdAndVariableName> QuestionsInvolvedInCustomValidationOfQuestion { get; set; }
    }
}