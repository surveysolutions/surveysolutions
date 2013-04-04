namespace Main.Core.Domain
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception which represents domain logic and is usually thrown by aggregate root command handlers.
    /// </summary>
    [Serializable]
    public class DomainException : Exception
    {
        public readonly DomainExceptionType ErrorType;

        public DomainException(DomainExceptionType errorType, string message)
            : base(message)
        {
            ErrorType = errorType;
        }
    }

    public enum DomainExceptionType
    {
        TriggerLinksToNotExistingGroup,

        TriggerLinksToNotPropagatedGroup,

        QuestionIsFeaturedButNotInsideNonPropagateGroup,

        QuestionIsHeadOfGroupButNotInsidePropagateGroup,

        NotSupportedPropagationGroup,

        GroupTitle_Required,

        QuestionnaireTitle_Required,

        Question_NotFound,

        Group_NotFound,

        QuestionTitle_Required,

        VariableName_Required,

        VariableName_MaxLength,

        VariableName_SpecialCharacters,

        VariableName_StartWithDigit,

        VarialbeName_NotUnique,

        Selector_Empty,

        SelectorValue_Required,

        SelectorValue_SpecialCharacters,

        SelectorValue_NotUnique,

        SelectorText_Required,

        SelectorText_NotUnique
    }
}