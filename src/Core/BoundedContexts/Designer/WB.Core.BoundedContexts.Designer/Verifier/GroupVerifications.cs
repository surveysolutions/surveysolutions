using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class GroupVerifications : AbstractVerifier, IPartialVerifier
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Critical<IGroup>(GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion, "WB0009", VerificationMessages.WB0009_GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion),
            Error<IGroup>(RosterSizeSourceQuestionTypeIsIncorrect, "WB0023", VerificationMessages.WB0023_RosterSizeSourceQuestionTypeIsIncorrect),
            Error<IGroup>(GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles, "WB0032", VerificationMessages.WB0032_GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles),
            Error<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion, "WB0033", VerificationMessages.WB0033_GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion),
            Error<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion, "WB0034", VerificationMessages.WB0034_GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion),
            Error<IGroup>(GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion, "WB0035", VerificationMessages.WB0035_GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion),
            Error<IGroup>(GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion, "WB0036", VerificationMessages.WB0036_GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion),
            Error<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles, "WB0037", VerificationMessages.WB0037_GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles),
            Error<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveDuplicateValues, "WB0041", VerificationMessages.WB0041_GroupWhereRosterSizeSourceIsFixedTitlesHaveDuplicateValues),
            Error<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesValuesHaveNonIntegerValues, "WB0115", VerificationMessages.WB0115_FixRosterSupportsOnlyIntegerTitleValues),
            Error<IGroup>(RosterFixedTitlesHaveMoreThanAllowedItems, "WB0038", string.Format(VerificationMessages.WB0038_RosterFixedTitlesHaveMoreThan200Items, Constants.MaxLongRosterRowCount)),
            Error<IGroup, IComposite>(RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster, "WB0054", VerificationMessages.WB0054_RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster),
            Error<IGroup>(RosterHasRosterLevelMoreThan4, "WB0055", string.Format(VerificationMessages.WB0055_RosterHasRosterLevelMoreThan4, MaxNestedRostersCount)),
            Error<IGroup>(RosterHasEmptyVariableName, "WB0067", VerificationMessages.WB0067_RosterHasEmptyVariableName),
            Error<IGroup>(RosterHasInvalidVariableName, "WB0069", VerificationMessages.WB0069_RosterHasInvalidVariableName),
            Error<IGroup>(this.RosterHasVariableNameEqualToQuestionnaireTitle, "WB0070", VerificationMessages.WB0070_RosterHasVariableNameEqualToQuestionnaireTitle),
            Error<IGroup>(GroupHasLevelDepthMoreThan10, "WB0101", string.Format(VerificationMessages.WB0101_GroupHasLevelDepthMoreThan10, MaxNestedSubsectionsCount)),
            ErrorForTranslation<IGroup>(GroupTitleIsTooLong, "WB0260", string.Format(VerificationMessages.WB0260_GroupTitleIsTooLong, MaxTitleLength)),
            ErrorForTranslation<IGroup>(GroupTitleIsEmpty, "WB0120", VerificationMessages.WB0120_GroupTitleIsEmpty),
            Error<IGroup>(LongMultiRosterCannotBeNested, "WB0081", string.Format(VerificationMessages.WB0081_LongRosterCannotBeNested, Constants.MaxRosterRowCount)),
            Error<IGroup>(LongListRosterCannotBeNested, "WB0081", string.Format(VerificationMessages.WB0081_LongRosterCannotBeNested, Constants.MaxRosterRowCount)),
            Error<IGroup>(LongFixedRosterCannotBeNested, "WB0081", string.Format(VerificationMessages.WB0081_LongRosterCannotBeNested, Constants.MaxRosterRowCount)),
            Error<IGroup>(LongFixedRosterCannotHaveNestedRosters, "WB0080", string.Format(VerificationMessages.WB0080_LongRosterCannotHaveNestedRosters,Constants.MaxRosterRowCount)),
            Error<IGroup>(LongMultiRosterCannotHaveNestedRosters, "WB0080", string.Format(VerificationMessages.WB0080_LongRosterCannotHaveNestedRosters,Constants.MaxRosterRowCount)),
            Error<IGroup>(LongListRosterCannotHaveNestedRosters, "WB0080", string.Format(VerificationMessages.WB0080_LongRosterCannotHaveNestedRosters,Constants.MaxRosterRowCount)),
            Error<IGroup>(LongRosterHasMoreThanAllowedChildElements, "WB0068", string.Format(VerificationMessages.WB0068_RosterHasMoreThanAllowedChildElements,Constants.MaxAmountOfItemsInLongRoster)),
            Error<IGroup, IComposite>(QuestionsCannotBeUsedAsRosterTitle, "WB0083", VerificationMessages.WB0083_QuestionCannotBeUsedAsRosterTitle),
            Error<IGroup>(RosterHasPropagationExededLimit, "WB0262", VerificationMessages.WB0262_RosterHasTooBigPropagation),
            Error<IGroup>(FirstChapterHasEnablingCondition, "WB0263", VerificationMessages.WB0263_FirstChapterHasEnablingCondition),
            Error<IGroup>(SectionHasMoreThanAllowedQuestions, "WB0270", string.Format(VerificationMessages.WB0270_SectionContainsTooManyQuestions, 400)),

            Warning(LargeNumberOfRosters, "WB0200", VerificationMessages.WB0200_LargeNumberOfRostersIsCreated),
            Warning<IGroup>(TooManyQuestionsInGroup, "WB0201", string.Format(VerificationMessages.WB0201_LargeNumberOfQuestionsInGroup, MaxQuestionsCountInSubSection)),
            Warning<IGroup>(EmptyGroup, "WB0202", VerificationMessages.WB0202_GroupWithoutQuestions),
            Warning<IGroup>(HasSingleQuestionInRoster, "WB0203", VerificationMessages.WB0203_RosterHasSingleQuestion),
            Warning<IGroup>(EmptyRoster, "WB0204", VerificationMessages.WB0204_EmptyRoster),
            Warning<IGroup>(FixedRosterContains3OrLessItems, "WB0207", string.Format(VerificationMessages.WB0207_FixedRosterContains3OrLessItems, MinFixedRosterItemsCount)),
            Warning<IGroup>(NotSingleSectionWithLessThan5Questions, "WB0223", VerificationMessages.WB0223_SectionWithLessThan5Questions),
            Warning<IGroup>(TooManySubsectionsAtOneLevel, "WB0224", VerificationMessages.WB0224_TooManySubsectionsAtOneLevel),
            Warning<IGroup>(NestedRosterDegree3OrMore, "WB0233", VerificationMessages.WB0233_NestedRosterDegree3OrMore),
            Warning<IGroup>(RosterInRosterWithSameSourceQuestion, "WB0234", VerificationMessages.WB0234_RosterInRosterWithSameSourceQuestion),
        };

        private static readonly HashSet<QuestionType> QuestionTypesValidToBeRosterTitles = new HashSet<QuestionType>
        {
            QuestionType.SingleOption,
            QuestionType.MultyOption,
            QuestionType.Numeric,
            QuestionType.DateTime,
            QuestionType.GpsCoordinates,
            QuestionType.Text,
            QuestionType.TextList,
            QuestionType.QRBarcode
        };

        private static bool RosterInRosterWithSameSourceQuestion(IGroup group)
        {
            if (group.IsRoster && group.RosterSizeQuestionId.HasValue)
            {
                return
                    group.UnwrapReferences(x => x.GetParent() as IGroup)
                        .Any(parent => parent != group && parent.IsRoster && parent.RosterSizeQuestionId == group.RosterSizeQuestionId);
            }

            return false;
        }

        private static bool NestedRosterDegree3OrMore(IGroup group)
            => group.IsRoster
               && group.UnwrapReferences(x => x.GetParent() as IGroup).Count(x => x.IsRoster) >= 3;


        private static bool TooManySubsectionsAtOneLevel(IGroup group)
            => group.Children.Count(Subsection) >= 10;

        private static bool Subsection(IComposite entity)
            => entity is IGroup
               && !IsSection(entity)
               && !((IGroup) entity).IsRoster;

        private static bool NotSingleSectionWithLessThan5Questions(IGroup group)
            => IsSection(group)
               && group.GetParent().Children.Count > 1
               && group.GetDescendants().Count(Question) < 5;

        private static bool Question(IQuestionnaireEntity entity) => entity is IQuestion;

        private static bool HasSingleQuestionInRoster(IGroup rosterGroup)
            => rosterGroup.IsRoster
               && rosterGroup.Children.OfType<IQuestion>().Count() == 1;

        private static bool EmptyGroup(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => !group.IsRoster
               && !group.Children.Any()
               && questionnaire.Find<IGroup>().Count() > 1;

        private static bool FixedRosterContains3OrLessItems(IGroup group)
            => IsFixedRoster(group)
               && group.FixedRosterTitles.Length <= 3;

        private static bool IsFixedRoster(IGroup group) => group.IsRoster && (group.FixedRosterTitles?.Any() ?? false);

        private static bool EmptyRoster(IGroup group)
            => group.IsRoster
               && !group.Children.Any();

        private static bool TooManyQuestionsInGroup(IGroup group)
            => group.Children.OfType<IQuestion>().Count() > 200;
        private static bool LargeNumberOfRosters(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<IGroup>(q => q.IsRoster).Count() > 20;

        public GroupVerifications(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        private static bool SectionHasMoreThanAllowedQuestions(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.Children.OfType<IQuestion>().Count() > MaxQuestionsCountInSection;

        private static bool FirstChapterHasEnablingCondition(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var parentComposite = group.GetParent();
            if (parentComposite.PublicKey != questionnaire.PublicKey) return false;

            if (parentComposite.Children.IndexOf(group) != 0) return false;

            return !string.IsNullOrEmpty(group.ConditionExpression);
        }

        private bool RosterHasPropagationExededLimit(IGroup roster, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRoster(roster))
                return false;

            Dictionary<Guid, long> rosterPropagationCounts = new Dictionary<Guid, long>();

            return CalculateRosterInstancesCountAndUpdateCache(roster, rosterPropagationCounts, questionnaire) > MaxRosterPropagationLimit;
        }

        private static EntityVerificationResult<IComposite> QuestionsCannotBeUsedAsRosterTitle(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var noErrors = new EntityVerificationResult<IComposite> { HasErrors = false };

            if (!group.RosterTitleQuestionId.HasValue)
                return noErrors;

            var rosterTitleQuestion =
                questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == group.RosterTitleQuestionId.Value);
            if (rosterTitleQuestion == null)
                return noErrors;

            if (QuestionTypesValidToBeRosterTitles.Contains(rosterTitleQuestion.QuestionType))
                return noErrors;

            return new EntityVerificationResult<IComposite>
            {
                HasErrors = true,
                ReferencedEntities = new IComposite[] { group, rosterTitleQuestion }
            };
        }

        private static bool GroupTitleIsTooLong(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.Title?.Length > 500;

        private static bool GroupTitleIsEmpty(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => string.IsNullOrWhiteSpace(group.Title);

        private static bool GroupHasLevelDepthMoreThan10(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            int groupLevel = 0;
            IComposite questionnaireItem = group;
            while (questionnaireItem != null)
            {
                groupLevel++;
                questionnaireItem = questionnaireItem.GetParent();
            }

            return groupLevel > 10 + 1/*questionnaire level*/;
        }

        
        private bool RosterHasInvalidVariableName(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!group.IsRoster)
                return false;
            return !IsVariableNameValid(group.VariableName);
        }

        private bool RosterHasVariableNameEqualToQuestionnaireTitle(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!group.IsRoster)
                return false;

            if (string.IsNullOrEmpty(group.VariableName))
                return false;

            var questionnaireVariableName = this.fileSystemAccessor.MakeStataCompatibleFileName(questionnaire.Title);

            return group.VariableName.Equals(questionnaireVariableName, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool RosterHasEmptyVariableName(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!group.IsRoster)
                return false;
            return string.IsNullOrWhiteSpace(group.VariableName);
        }

        private static EntityVerificationResult<IComposite> RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster(IGroup roster, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRoster(roster))
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var rosterSizeQuestion = questionnaire.Questionnaire.GetRosterSizeQuestion(roster);
            if (rosterSizeQuestion == null)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            RosterScope rosterSizeQuestionScope = questionnaire.Questionnaire.GetRosterScope(rosterSizeQuestion);
            if (rosterSizeQuestionScope.Length == 0)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var rosterLevelForRoster = questionnaire.Questionnaire.GetRosterScope(roster);

            if (!rosterSizeQuestionScope.IsParentScopeFor(rosterLevelForRoster))
            {
                return new EntityVerificationResult<IComposite>
                {
                    HasErrors = true,
                    ReferencedEntities = new IComposite[] { roster, rosterSizeQuestion }
                };
            }

            return new EntityVerificationResult<IComposite> { HasErrors = false };
        }

        private static bool GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRosterByQuestion(group))
                return false;
            if (!questionnaire.Questionnaire.IsMultiRoster(group))
                return false;
            return group.RosterTitleQuestionId.HasValue;
        }

        private static bool GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsFixedRoster(group))
                return false;
            if (group.FixedRosterTitles == null)
                return false;

            if (group.FixedRosterTitles.Length < 2)
                return true;

            return group.FixedRosterTitles.Any(title => string.IsNullOrWhiteSpace(title.Title));
        }

        private static bool GroupWhereRosterSizeSourceIsFixedTitlesHaveDuplicateValues(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsFixedRoster(group))
                return false;
            if (group.FixedRosterTitles == null)
                return false;
            if (group.FixedRosterTitles.Length == 0)
                return false;
            return group.FixedRosterTitles.Select(x => x.Value).Distinct().Count() != group.FixedRosterTitles.Length;
        }

        private static bool GroupWhereRosterSizeSourceIsFixedTitlesValuesHaveNonIntegerValues(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsFixedRoster(group))
                return false;

            if (group.FixedRosterTitles == null)
                return false;

            if (group.FixedRosterTitles.Length == 0)
                return false;

            foreach (var rosterTitle in group.FixedRosterTitles)
            {
                if ((rosterTitle.Value % 1) != 0)
                    return true;
            }

            return false;
        }

        private static bool RosterFixedTitlesHaveMoreThanAllowedItems(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsFixedRoster(group))
                return false;

            return group.FixedRosterTitles.Length > Constants.MaxLongRosterRowCount;
        }

        private static bool RosterHasRosterLevelMoreThan4(IGroup roster, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRoster(roster))
                return false;

            return questionnaire.Questionnaire.GetRosterScope(roster).Length > 4;
        }
        private static bool GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsRosterByQuestion(group) && questionnaire.Questionnaire.GetRosterSizeQuestion(group) == null;
        }

        private static bool RosterSizeSourceQuestionTypeIsIncorrect(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var rosterSizeQuestion = questionnaire.Questionnaire.GetRosterSizeQuestion(group);
            if (rosterSizeQuestion == null)
                return false;

            return !questionnaire.Questionnaire.IsQuestionAllowedToBeRosterSizeSource(rosterSizeQuestion);
        }

        private static bool LongMultiRosterCannotBeNested(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsRosterByQuestion(@group) && IsLongRosterNested(group, questionnaire, (g, q) => (questionnaire.Questionnaire.GetRosterSizeQuestion(g) as MultyOptionsQuestion)?.MaxAllowedAnswers);
        }

        private static bool LongListRosterCannotBeNested(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsRosterByQuestion(@group) && IsLongRosterNested(group, questionnaire, (g, q) => (questionnaire.Questionnaire.GetRosterSizeQuestion(g) as TextListQuestion)?.MaxAnswerCount);
        }

        private static bool LongFixedRosterCannotBeNested(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsFixedRoster(group) && IsLongRosterNested(group, questionnaire, (g, q) => group.FixedRosterTitles.Length);
        }

        private static bool LongMultiRosterCannotHaveNestedRosters(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsRosterByQuestion(@group) && IsLongRosterHasNestedRosters(group, questionnaire, (g, q) => (questionnaire.Questionnaire.GetRosterSizeQuestion(g) as MultyOptionsQuestion)?.MaxAllowedAnswers);
        }

        private static bool LongListRosterCannotHaveNestedRosters(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsRosterByQuestion(@group) && IsLongRosterHasNestedRosters(group, questionnaire, (g, q) => (questionnaire.Questionnaire.GetRosterSizeQuestion(g) as TextListQuestion)?.MaxAnswerCount);
        }

        private static bool LongFixedRosterCannotHaveNestedRosters(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsFixedRoster(group) && IsLongRosterHasNestedRosters(group, questionnaire, (g, q) => @group.FixedRosterTitles.Length);
        }

        private static bool LongRosterHasMoreThanAllowedChildElements(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return IsLongRoster(@group, questionnaire)
                   && questionnaire.FindInGroup<IComposite>(@group.PublicKey).Count(c => !(c is IVariable)) > Constants.MaxAmountOfItemsInLongRoster;
        }

        private static bool IsLongRoster(IGroup roster, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return GetRosterMaxPropagationCount(roster, questionnaire) > Constants.MaxRosterRowCount;
        }

        private static bool IsLongRosterNested(IGroup group, MultiLanguageQuestionnaireDocument questionnaire,
            Func<IGroup, MultiLanguageQuestionnaireDocument, int?> getRosterSizeLimitSetByUser)
        {
            var rosterSizeLimitSetByUser = getRosterSizeLimitSetByUser(@group, questionnaire);
            if (rosterSizeLimitSetByUser == null)
                return false;

            return rosterSizeLimitSetByUser > Constants.MaxRosterRowCount && questionnaire.Questionnaire.GetRosterScope(@group).Length > 1;
        }

        private static bool IsLongRosterHasNestedRosters(IGroup group, MultiLanguageQuestionnaireDocument questionnaire,
            Func<IGroup, MultiLanguageQuestionnaireDocument, int?> getRosterSizeLimitSetByUser)
        {
            var rosterSizeLimitSetByUser = getRosterSizeLimitSetByUser(@group, questionnaire);
            if (rosterSizeLimitSetByUser == null)
                return false;

            return rosterSizeLimitSetByUser > Constants.MaxRosterRowCount && GroupHasNestedRosters(@group, questionnaire);
        }

        private static bool GroupHasNestedRosters(IGroup @group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var findInGroup = questionnaire.FindInGroup<IGroup>(@group.PublicKey);
            return findInGroup.Any(x => questionnaire.Questionnaire.IsRoster(x));
        }

        private static bool GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsRosterByQuestion(group) && group.FixedRosterTitles.Any();
        }

        private static bool GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsFixedRoster(group) && group.RosterSizeQuestionId.HasValue;
        }

        private static bool GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsFixedRoster(group) && group.RosterTitleQuestionId.HasValue;
        }

        private static bool GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRosterByQuestion(group))
                return false;
            if (!questionnaire.Questionnaire.IsNumericRoster(group))
                return false;
            if (!group.RosterTitleQuestionId.HasValue)
                return false;

            var rosterTitleQuestion = questionnaire.FirstOrDefault<IQuestion>(x => x.PublicKey == group.RosterTitleQuestionId.Value);
            if (rosterTitleQuestion == null)
                return true;

            if (!questionnaire.Questionnaire.GetRosterScope(rosterTitleQuestion).Any())
                return true;

            RosterScope rosterScopeForGroup = questionnaire.Questionnaire.GetRosterScope(group);
            RosterScope rosterScopeForTitleQuestion = questionnaire.Questionnaire.GetRosterScope(rosterTitleQuestion);

            if (!rosterScopeForGroup.Equals(rosterScopeForTitleQuestion))
                return true;

            return false;
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Critical<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Critical(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Error(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TEntity, TReferencedEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, EntityVerificationResult<TReferencedEntity>> verifyEntity, string code, string message)
            where TEntity : class, IComposite
            where TReferencedEntity : class, IComposite
        {
            return questionnaire =>
                from entity in questionnaire.Find<TEntity>(_ => true)
                let verificationResult = verifyEntity(entity, questionnaire)
                where verificationResult.HasErrors
                select QuestionnaireVerificationMessage.Error(code, message, verificationResult.ReferencedEntities.Select(x => CreateReference(x)).ToArray());
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> ErrorForTranslation<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .FindWithTranslations<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(translatedEntity =>
                    {
                        var translationMessage = translatedEntity.TranslationName == null
                            ? message
                            : translatedEntity.TranslationName + ": " + message;
                        var questionnaireVerificationReference = CreateReference(translatedEntity.Entity);
                        return QuestionnaireVerificationMessage.Error(code, translationMessage, questionnaireVerificationReference);
                    });
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IQuestionnaireEntity
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(x => hasError(x, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity>(
            Func<TEntity, bool> hasError, string code, string message)
            where TEntity : class, IQuestionnaireEntity
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(hasError)
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                hasError(questionnaire)
                    ? new[] { QuestionnaireVerificationMessage.Warning(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }

        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument)
        {
            var verificationMessagesByQuestionnaire = new List<QuestionnaireVerificationMessage>();
            foreach (var verifier in this.ErrorsVerifiers)
            {
                verificationMessagesByQuestionnaire.AddRange(verifier.Invoke(multiLanguageQuestionnaireDocument));
            }
            return verificationMessagesByQuestionnaire;
        }
    }
}