using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Markdig;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
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
            Critical<IGroup>(WB0009_RosterSizeQuestionIsMissing, "WB0009", VerificationMessages.WB0009_RosterSizeQuestionIsMissing),
            Error<IGroup>(RosterSizeSourceQuestionTypeIsIncorrect, "WB0023", VerificationMessages.WB0023_RosterSizeSourceQuestionTypeIsIncorrect),
            Error<IGroup>(QuestionTriggeredRosterHasFixedTitles, "WB0032", VerificationMessages.WB0032_GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles),
            Error<IGroup>(FixedRosterHasRosterSizeQuestion, "WB0033", VerificationMessages.WB0033_GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion),
            Error<IGroup>(FixedRosterHasRosterTitleQuestion, "WB0034", VerificationMessages.WB0034_GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion),
            Error<IGroup>(QuestionTriggeredRosterHasInvalidRosterTitleQuestion, "WB0035", VerificationMessages.WB0035_GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion),
            Error<IGroup>(ListOrMultiRostersHaveRosterTitleQuestion, "WB0036", VerificationMessages.WB0036_ListAndMultiRostersCantHaveRosterTitleQuestion),
            Error<IGroup>(FixedRosterTitlesHasEmptyTitles, "WB0037", VerificationMessages.WB0037_GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles),
            Error<IGroup>(FixedRosterHasDuplicateValues, "WB0041", VerificationMessages.WB0041_GroupWhereRosterSizeSourceIsFixedTitlesHaveDuplicateValues),
            Error<IGroup>(FixedRosterHasNonIntegerValues, "WB0115", VerificationMessages.WB0115_FixRosterSupportsOnlyIntegerTitleValues),
            Error<IGroup>(FixedRosterHasMoreThanAllowedItems, "WB0038", string.Format(VerificationMessages.WB0038_RosterFixedTitlesHaveMoreThan200Items, Constants.MaxLongRosterRowCount)),
            Error<IGroup, IComposite>(RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster, "WB0054", VerificationMessages.WB0054_RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster),
            Error<IGroup>(RosterLevelIsMoreThan4, "WB0055", string.Format(VerificationMessages.WB0055_RosterHasRosterLevelMoreThan4, MaxNestedRostersCount)),
            Error<IGroup>(RostersVariableEqualsToQuestionnaireTitle, "WB0070", VerificationMessages.WB0070_RosterHasVariableNameEqualToQuestionnaireTitle),
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
            Error<IGroup>(FirstChapterHasEnablingCondition, "WB0263", VerificationMessages.WB0263_FirstChapterHasEnablingCondition),
            Error<IGroup>(SectionHasMoreThanAllowedQuestions, "WB0270", string.Format(VerificationMessages.WB0270_SectionContainsTooManyQuestions, 400)),
            Error<IGroup>(FlatModeGroupContainsNestedGroup, "WB0279", VerificationMessages.WB0279_PlainModeGroupContainsNestedGroup),
            Error<IGroup>(FlatModeGroupHasMoreThanAllowedEntities, "WB0278", string.Format(VerificationMessages.WB0278_PlainModeAllowedOnlyForGroupWithNoMoreThanElements, MaxUIEntitiesInPlainModeGroup)),
            Error<IGroup>(LinksAreProhibitedOnNavigationElements, "WB0057", VerificationMessages.WB0057_LinksAreProhibitedOnNavigationElements),
            Error<IGroup>(TableRosterContainsNestedGroup, "WB0282", VerificationMessages.WB0282_TableRosterContainsNestedGroup),
            Error<IGroup>(TableRosterHasMoreThanAllowedEntities, "WB0283", string.Format(VerificationMessages.WB0283_TableRosterAllowedOnlyForGroupWithNoMoreThanElements, MaxEntitiesInTableRoster)),
            Error<IGroup>(TableRosterCantContainsSupervisorQuestions, "WB0284", VerificationMessages.WB0284_TableRosterCantContainsSupervisorQuestions),
            Error<IGroup>(TableRosterContainsOnlyAllowedQuestionTypes, "WB0285", VerificationMessages.WB0285_TableRosterContainsOnlyAllowedQuestionTypes),
            Error<IGroup>(MatrixRosterContainsOnlyAllowedQuestionTypes, "WB0297", VerificationMessages.WB0297_MatrixRosterContainsOnlyAllowedQuestionTypes),
            Error<IGroup>(MatrixRosterHasMoreThanAllowedEntities, "WB0298", string.Format(VerificationMessages.WB0298_MatrixRosterAllowedOnlyForGroupWithNoMoreThanElements, MaxEntitiesInMatrixRoster)),
            Error<IGroup>(MatrixRosterHasToContainNoSupervisorOrIdentifyingQuestions, "WB0299", VerificationMessages. WB0299_MatrixRosterHasToContainNoSupervisorOrIdentifyingQuestions),
            Error<IGroup>(MatrixRosterHasToContainNoLinkedQuestions, "WB0301", VerificationMessages. WB0301_MatrixRosterHasToContainNoLinkedQuestions),
            Error<IGroup>(MatrixRosterCannotHaveCustomTitle, "WB0303", VerificationMessages.WB0303_MatrixRosterCannotHaveCustomTitle),
            Error<IGroup>(TableRosterCannotHaveCustomTitle, "WB0304", VerificationMessages.WB0304_TableRosterCannotHaveCustomTitle),
            
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
            Warning<IGroup>(RosterHasPropagationExededLimit, "WB0262", VerificationMessages.WB0262_RosterHasTooBigPropagation),
            Warning<IGroup>(TableAndMatrixRosterWorksOnlyInWebMode, "WB0286", VerificationMessages.WB0286_TableAndMatixRosterWorksOnlyInWebMode),
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

        private static bool LinksAreProhibitedOnNavigationElements(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var links = GetMarkdownLinksFromText(group.Title).ToList();
            return links.Any();
        }

        public static IEnumerable<string> GetMarkdownLinksFromText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) yield break;

            var builder = new MarkdownPipelineBuilder();

            builder.BlockParsers.Clear();
            builder.InlineParsers.Clear();

            builder.BlockParsers.AddIfNotAlready<ParagraphBlockParser>();
            builder.InlineParsers.AddIfNotAlready<LinkInlineParser>();
            builder.InlineParsers.AddIfNotAlready<AutolinkInlineParser>();

            var pipeline = builder.Build();

            foreach (var node in Markdown.Parse(text, pipeline).AsEnumerable())
            {
                if(!(node is LeafBlock leafBlock)) continue;
                foreach (var inline in leafBlock.Inline)
                {
                    if(!(inline is LinkInline link) || link.Url==null) continue;
                    
                    yield return link.Url.ToLower();
                }
            }
        }

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
               && (group.GetParent() ?? throw new InvalidOperationException("Parent was not found.")).Children.Count > 1
               && group.GetDescendants().Count(Question) < 5;

        private static bool Question(IQuestionnaireEntity entity) => entity is IQuestion;

        private static bool HasSingleQuestionInRoster(IGroup rosterGroup)
            => rosterGroup.IsRoster
               && rosterGroup.DisplayMode != RosterDisplayMode.Matrix
               && rosterGroup.Children.Count == 1
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

        private static bool FlatModeGroupHasMoreThanAllowedEntities(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Flat 
               && group.Children.Count(x => x is IStaticText || x is IQuestion) > MaxUIEntitiesInPlainModeGroup;

        private static bool FlatModeGroupContainsNestedGroup(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Flat && group.Children.Any(composite =>
            {
                if (composite is IGroup childGroup)
                    return childGroup.DisplayMode == RosterDisplayMode.Flat || childGroup.IsRoster;
                return false;
            });

        private static bool FirstChapterHasEnablingCondition(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var parentComposite = group.GetParent();
            if (parentComposite?.PublicKey != questionnaire.PublicKey) return false;

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
            {
                if (!questionnaire.Questionnaire.IsLinked(rosterTitleQuestion))
                    return noErrors;
            }

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
            IComposite? questionnaireItem = group;
            while (questionnaireItem != null)
            {
                groupLevel++;
                questionnaireItem = questionnaireItem.GetParent();
            }

            return groupLevel > 10 + 1/*questionnaire level*/;
        }

        private bool RostersVariableEqualsToQuestionnaireTitle(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!group.IsRoster)
                return false;

            if (string.IsNullOrEmpty(group.VariableName))
                return false;

            var questionnaireVariableName = this.fileSystemAccessor.MakeStataCompatibleFileName(questionnaire.Title);

            return group.VariableName.Equals(questionnaireVariableName, StringComparison.InvariantCultureIgnoreCase);
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

        private static bool ListOrMultiRostersHaveRosterTitleQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRosterByQuestion(group))
                return false;

            if (questionnaire.Questionnaire.IsMultiRoster(group))
                return group.RosterTitleQuestionId.HasValue;

            if (questionnaire.Questionnaire.IsListRoster(group))
                return group.RosterTitleQuestionId.HasValue;

            return false;
        }

        private static bool FixedRosterTitlesHasEmptyTitles(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsFixedRoster(group))
                return false;
            if (group.FixedRosterTitles == null)
                return false;

            if (group.FixedRosterTitles.Length < 2)
                return true;

            return group.FixedRosterTitles.Any(title => string.IsNullOrWhiteSpace(title.Title));
        }

        private static bool FixedRosterHasDuplicateValues(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsFixedRoster(group))
                return false;
            if (group.FixedRosterTitles == null)
                return false;
            if (group.FixedRosterTitles.Length == 0)
                return false;
            return group.FixedRosterTitles.Select(x => x.Value).Distinct().Count() != group.FixedRosterTitles.Length;
        }

        private static bool FixedRosterHasNonIntegerValues(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
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

        private static bool FixedRosterHasMoreThanAllowedItems(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsFixedRoster(group))
                return false;

            return group.FixedRosterTitles.Length > Constants.MaxLongRosterRowCount;
        }

        private static bool RosterLevelIsMoreThan4(IGroup roster, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRoster(roster))
                return false;

            return questionnaire.Questionnaire.GetRosterScope(roster).Length > 4;
        }
        private static bool WB0009_RosterSizeQuestionIsMissing(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
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
            return questionnaire.Questionnaire.IsRosterByQuestion(@group) 
                   && IsLongRosterHasNestedRosters(group, questionnaire, 
                       (g, q) => (questionnaire.Questionnaire.GetRosterSizeQuestion(g) as TextListQuestion)?.MaxAnswerCount);
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

        private static bool QuestionTriggeredRosterHasFixedTitles(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsRosterByQuestion(group) && group.FixedRosterTitles.Any();
        }

        private static bool FixedRosterHasRosterSizeQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsFixedRoster(group) && group.RosterSizeQuestionId.HasValue;
        }

        private static bool FixedRosterHasRosterTitleQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsFixedRoster(group) && group.RosterTitleQuestionId.HasValue;
        }

        private static bool QuestionTriggeredRosterHasInvalidRosterTitleQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
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

        private static bool MatrixRosterHasMoreThanAllowedEntities(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Matrix && group.Children.Count() > MaxEntitiesInMatrixRoster;

        private static bool MatrixRosterContainsOnlyAllowedQuestionTypes(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Matrix && group.Children.Any(composite =>
            {
                if (composite is IQuestion question)
                    switch (question.QuestionType)
                    {
                        case QuestionType.SingleOption:
                        case QuestionType.MultyOption:
                            return question.IsFilteredCombobox == true 
                                   || question.CascadeFromQuestionId != null 
                                   || (question as MultyOptionsQuestion)?.YesNoView == true;
                        default: return true;
                    }
                return true;
            });

        private static bool MatrixRosterHasToContainNoSupervisorOrIdentifyingQuestions(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Matrix && group.Children.Any(composite =>
            {
                if (composite is IQuestion question)
                    return question.QuestionScope == QuestionScope.Supervisor 
                           || question.QuestionScope == QuestionScope.Headquarter;
                return false;
            });

        private static bool MatrixRosterCannotHaveCustomTitle(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
          => group.DisplayMode == RosterDisplayMode.Matrix && group.CustomRosterTitle;
        private static bool TableRosterCannotHaveCustomTitle(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
          => group.DisplayMode == RosterDisplayMode.Table && group.CustomRosterTitle;

        private static bool MatrixRosterHasToContainNoLinkedQuestions(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Matrix && group.Children.Any(composite =>
            {
                if (composite is IQuestion question)
                    return question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue;
                return false;
            });

        private static bool TableRosterHasMoreThanAllowedEntities(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Table && group.Children.Count() > MaxEntitiesInTableRoster;

        private static bool TableRosterContainsNestedGroup(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Table && group.Children.Any(composite =>
            {
                if (composite is IGroup childGroup)
                    return true;
                return false;
            });

        private static bool TableRosterCantContainsSupervisorQuestions(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Table && group.Children.Any(composite =>
            {
                if (composite is IQuestion question)
                    return question.QuestionScope == QuestionScope.Supervisor;
                return false;
            });

        private static bool TableRosterContainsOnlyAllowedQuestionTypes(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Table && group.Children.Any(composite =>
            {
                if (composite is IQuestion question)
                    switch (question.QuestionType)
                    {
                        case QuestionType.Text:
                        case QuestionType.Numeric:
                            return false;
                        default: return true;
                    }

                if (composite is IStaticText staticText)
                    return true;

                return false;
            });

        private static bool TableAndMatrixRosterWorksOnlyInWebMode(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => group.DisplayMode == RosterDisplayMode.Table || group.DisplayMode == RosterDisplayMode.Matrix;


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
