using System;
using System.Collections.Generic;
using System.Diagnostics;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{Title} ({Id})")]
    public class InterviewQuestionView : InterviewEntityView
    {
        public List<ValidationCondition> FailedValidationMessages { get; set; }

        public string AnswerString { get; set; }

        public decimal[] RosterVector { get; set; }

        public string Variable { get; set; }

        public List<QuestionOptionView> Options { get; set; }

        public QuestionScope Scope { get; set; }

        public bool IsAnswered { get; set; }

        public string Title { get; set; }

        public QuestionType QuestionType { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsValid { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsFlagged { get; set; }
        public bool IsFilteredCategorical { get; set; }

        public int[] PropagationVector { get; set; }

        public List<InterviewQuestionCommentView> Comments { get; set; }

        public object Answer { get; set; }

        public dynamic Settings { get; set; }
    }

    public class TextQuestionSettings
    {
        public string Mask { get; set; }
    }

    public class DateTimeQuestionSettings
    {
        public bool IsTimestamp { get; set; }
    }

    public class NumericQuestionSettings
    {
        public bool IsInteger { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
        public bool UseFormating { get; set; }
    }

    public class MultiQuestionSettings
    {
        public bool YesNoQuestion { get; set; }
        public bool AreAnswersOrdered { get; set; }

        public int? MaxAllowedAnswers { get; set; }

        public bool IsLinkedToRoster { get; set; }

        public bool IsLinkedToListQuestion { get; set; }
    }

    public class SingleQuestionSettings
    {
        public bool IsFilteredCombobox { get; set; }

        public bool IsCascade { get; set; }

        public bool IsLinkedToRoster { get; set; }

        public bool IsLinkedToListQuestion { get; set; }
    }

    public class InterviewQuestionCommentView
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public Guid CommenterId { get; set; }
        public string CommenterName { get; set; }
        public UserRoles? CommenterRole { get; set; }
    }

    public class QuestionOptionView
    {
        public object Value { get; set; }
        public string Label { get; set; }
    }
}