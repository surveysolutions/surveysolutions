namespace WB.Core.BoundedContexts.Designer.Aggregates
{
    public enum DomainExceptionType
    {
        /// <summary>
        /// This should be used when there are no business logic depending on exception type.
        /// </summary>
        Undefined = 0,

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

        LinkedEntityDoesNotExist,

        QuestionCanNotBeFeatured,

        QuestionCanNotContainValidation,

        ConflictBetweenLinkedQuestionAndOptions,

        NotSupportedQuestionForLinkedQuestion,

        LinkedQuestionIsNotInPropagateGroup,

        LinkedCategoricalQuestionCanNotBeFilledBySupervisor,

        NotCategoricalQuestionLinkedToAnotherQuestion,

        QuestionWithLinkedQuestionCanNotBeFeatured,

        AutoPropagateQuestionCantBeReal,
        TextContainsSubstitutionReferenceQuestionOfInvalidType,

        TextContainsInvalidSubstitutionReference,

        QuestionTypeIsReroutedOnQuestionTypeSpecificCommand, 
           
        CountOfDecimalPlacesValueIsIncorrect,
        
        IntegerQuestionCantHaveDecimalPlacesSettings,

        TextContainsUnknownSubstitutionReference,

        TextContainsSubstitutionReferenceToSelf,

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

        CategoricalQuestionHasMoreThan200Options,
        
        CategoricalCascadingQuestionOptionsMaxLength,

        MacroIsAbsent,

        MacroAlreadyExist,

        MacroContentIsEmpty,

        LookupTableAlreadyExist,

        LookupTableIsAbsent,

        EmptyLookupTable,

        VariableNameEndsWithUnderscore,

        VariableNameHasConsecutiveUnderscores,

        GroupYouAreLinkedToIsNotRoster,

        VariableLabelContainsSubstitutionReference,
        CategoricalCascadingOptionsContainsNotUniqueTitleAndParentValuePair,

        TryToDeleteCoverPage,
        CoverSectionMustBeFirst,
        CanNotAddElementToCoverPage,
        CanNotEditElementIntoCoverPage,
        MigrateToNewVersion,
        InvalidUserInfo
    }
}
