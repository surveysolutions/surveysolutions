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

        GroupTitleRequired, 

        QuestionnaireTitleRequired,

        StaticTextNotFound,

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

        StaticTextWithSuchIdAlreadyExists,

        QuestionWithSuchIdAlreadyExists,
        
        GroupWithSuchIdAlreadyExists,

        MoreThanOneStaticTextsWithSuchIdExists,

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

        AutoPropagateQuestionCantBeReal,
        QuestionTitleContainsSubstitutionReferenceQuestionOfInvalidType,

        QuestionTitleContainsInvalidSubstitutionReference,

        QuestionTypeIsReroutedOnQuestionTypeSpecificCommand, 
           
        CountOfDecimalPlacesValueIsIncorrect,
        
        IntegerQuestionCantHaveDecimalPlacesSettings,

        QuestionTitleContainsUnknownSubstitutionReference,

        QuestionTitleContainsSubstitutionReferenceToSelf,

        FeaturedQuestionTitleContainsSubstitutionReference,

        MaxAllowedAnswersLessThan2,

        MaxAllowedAnswersMoreThanOptions,

        MultiOptionQuestionCanNotBeFeatured,

        ExpressionContainsNotExistingQuestionReference,

        QuestionOrGroupDependOnAnotherQuestion,

        QuestionUsedAsRosterTitleOfOtherGroup,

        MaxAnswerCountNotInRange,

        StaticTextIsEmpty,
    }
}