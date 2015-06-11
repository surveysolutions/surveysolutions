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

        EntityNotFound,

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

        EntityWithSuchIdAlreadyExists,

        QuestionWithSuchIdAlreadyExists,
        
        GroupWithSuchIdAlreadyExists,

        MoreThanOneEntityWithSuchIdExists,

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

        LinkedCategoricalQuestionCanNotBeFilledBySupervisor,

        NotCategoricalQuestionLinkedToAnoterQuestion,

        QuestionWithLinkedQuestionCanNotBeFeatured,

        AutoPropagateQuestionCantBeReal,
        QuestionTitleContainsSubstitutionReferenceQuestionOfInvalidType,

        QuestionTitleContainsInvalidSubstitutionReference,

        QuestionTypeIsReroutedOnQuestionTypeSpecificCommand, 
           
        CountOfDecimalPlacesValueIsIncorrect,
        
        IntegerQuestionCantHaveDecimalPlacesSettings,

        DecimalQuestionCantHaveMaxValueSettings,

        QuestionTitleContainsUnknownSubstitutionReference,

        QuestionTitleContainsSubstitutionReferenceToSelf,

        FeaturedQuestionTitleContainsSubstitutionReference,

        MaxAllowedAnswersLessThan2,

        MaxAllowedAnswersMoreThanOptions,

        MultiOptionQuestionCanNotBeFeatured,

        ExpressionContainsNotExistingQuestionOrRosterReference,

        QuestionOrGroupDependOnAnotherQuestion,

        QuestionUsedAsRosterTitleOfOtherGroup,

        MaxAnswerCountNotInRange,

        StaticTextIsEmpty,

        TitleIsTooLarge,

        FilteredComboboxQuestionNotFound,

        QuestionIsNotAFilteredCombobox,

        FilteredComboboxQuestionOptionsMaxLength,

        CategoricalSingleOptionHasMoreThan200Options,

        CategoricalCascadingOptionsCantContainsEmptyParentValueField,

        CategoricalCascadingOptionsCantContainsNotDecimalParentValueField,

        CategoricalCascadingOptionsContainsNotUniqueTitleAndParentValuePair
    }
}