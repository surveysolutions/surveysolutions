namespace WB.Core.BoundedContexts.Designer.Exceptions
{
    public enum DomainExceptionType
    {
        /// <summary>
        /// This should be used when there are no business logic depending on exception type.
        /// </summary>
        Undefined,

        TriggerLinksToNotExistingGroup, 

        TriggerLinksToNotPropagatedGroup, 

        QuestionIsHeaderOfRosterButNotInsideRoster, 

        GroupTitleRequired, 

        QuestionnaireTitleRequired, 

        QuestionNotFound, 

        GroupNotFound, 

        QuestionTitleRequired, 

        VariableNameRequired, 

        VariableNameMaxLength, 

        VariableNameSpecialCharacters, 

        VariableNameStartWithDigit, 

        VarialbeNameNotUnique, 

        SelectorEmpty, 

        SelectorValueRequired, 

        SelectorValueSpecialCharacters, 

        SelectorValueNotUnique, 

        SelectorTextRequired, 

        SelectorTextNotUnique,

        NotAllowedQuestionType,

        QuestionWithSuchIdAlreadyExists,
        
        GroupWithSuchIdAlreadyExists,

        MoreThanOneQuestionsWithSuchIdExists,

        MoreThanOneGroupsWithSuchIdExists,

        TemplateIsInvalid,

        VariableNameShouldNotMatchWithKeywords,

        TooFewOptionsInCategoryQuestion,

        CouldNotDeleteInterview,

        DoesNotHavePermissionsForEdit,

        UserDoesNotExistInShareList,

        UserExistInShareList,

        OwnerCannotBeInShareList,

        LinkedQuestionDoesNotExist,

        QuestionCanNotBeFeatured,

        QuestionCanNotContainValidation,

        ConflictBetweenLinkedQuestionAndOptions,

        NotSupportedQuestionForLinkedQuestion,

        LinkedQuestionIsNotInPropagateGroup,

        NotCategoricalQuestionLinkedToAnoterQuestion,

        QuestionWithLinkedQuestionCanNotBeFeatured,

        QuestionWithLinkedQuestionCanNotBeHead,

        AutoPropagateQuestionCantBeReal,
        QuestionTitleContainsSubstitutionReferenceQuestionOfInvalidType,

        QuestionTitleContainsInvalidSubstitutionReference,

        QuestionTypeIsReroutedOnQuestionTypeSpecificCommand, 
           
        CountOfDecimalPlacesValueIsIncorrect,
        
        IntegerQuestionCantHaveDecimalPlacesSettings,

        QuestionTitleContainsUnknownSubstitutionReference,

        QuestionTitleContainsSubstitutionReferenceToSelf,

        FeaturedQuestionTitleContainsSubstitutionReference,

        MaxAllowedAnswersIsNotPositive,

        MaxAllowedAnswersMoreThanOptions,

        MultiOptionQuestionCanNotBeFeatured,

        ExpressionContainsNotExistingQuestionReference,

        QuestionOrGroupDependOnAnotherQuestion
    }
}