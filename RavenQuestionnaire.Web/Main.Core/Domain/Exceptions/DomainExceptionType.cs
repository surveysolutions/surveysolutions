// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainExceptionType.cs" company="">
//   
// </copyright>
// <summary>
//   The domain exception type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Domain
{
    /// <summary>
    /// The domain exception type.
    /// </summary>
    public enum DomainExceptionType
    {
        AutoPropagateGroupCantHaveChildGroups,

        /// <summary>
        /// The trigger links to not existing group.
        /// </summary>
        TriggerLinksToNotExistingGroup, 

        /// <summary>
        /// The trigger links to not propagated group.
        /// </summary>
        TriggerLinksToNotPropagatedGroup, 

        /// <summary>
        /// The question is featured but not inside non propagate group.
        /// </summary>
        QuestionIsFeaturedButNotInsideNonPropagateGroup, 

        /// <summary>
        /// The question is head of group but not inside propagate group.
        /// </summary>
        QuestionIsHeadOfGroupButNotInsidePropagateGroup, 

        /// <summary>
        /// The not supported propagation group.
        /// </summary>
        NotSupportedPropagationGroup, 

        /// <summary>
        /// The group title_ required.
        /// </summary>
        GroupTitleRequired, 

        /// <summary>
        /// The questionnaire title_ required.
        /// </summary>
        QuestionnaireTitleRequired, 

        /// <summary>
        /// The question_ not found.
        /// </summary>
        QuestionNotFound, 

        /// <summary>
        /// The group_ not found.
        /// </summary>
        GroupNotFound, 

        /// <summary>
        /// The question title_ required.
        /// </summary>
        QuestionTitleRequired, 

        /// <summary>
        /// The variable name_ required.
        /// </summary>
        VariableNameRequired, 

        /// <summary>
        /// The variable name_ max length.
        /// </summary>
        VariableNameMaxLength, 

        /// <summary>
        /// The variable name_ special characters.
        /// </summary>
        VariableNameSpecialCharacters, 

        /// <summary>
        /// The variable name_ start with digit.
        /// </summary>
        VariableNameStartWithDigit, 

        /// <summary>
        /// The varialbe name_ not unique.
        /// </summary>
        VarialbeNameNotUnique, 

        /// <summary>
        /// The selector_ empty.
        /// </summary>
        SelectorEmpty, 

        /// <summary>
        /// The selector value_ required.
        /// </summary>
        SelectorValueRequired, 

        /// <summary>
        /// The selector value_ special characters.
        /// </summary>
        SelectorValueSpecialCharacters, 

        /// <summary>
        /// The selector value_ not unique.
        /// </summary>
        SelectorValueNotUnique, 

        /// <summary>
        /// The selector text_ required.
        /// </summary>
        SelectorTextRequired, 

        /// <summary>
        /// The selector text_ not unique.
        /// </summary>
        SelectorTextNotUnique,

        NotAllowedQuestionType,

        QuestionWithSuchIdAlreadyExists,
        
        GroupCantBecomeAutoPropagateIfHasAnyChildGroup,

        GroupWithSuchIdAlreadyExists,

        MoreThanOneQuestionsWithSuchIdExists,

        MoreThanOneGroupsWithSuchIdExists,
    }
}