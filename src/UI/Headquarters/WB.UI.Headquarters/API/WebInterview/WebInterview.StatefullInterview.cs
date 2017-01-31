using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.UI.Headquarters.Models.WebInterview;
using InterviewStaticText = WB.UI.Headquarters.Models.WebInterview.InterviewStaticText;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        private static InterviewEntityWithType[] ActionButtonsDefinition { get; } = {
            new InterviewEntityWithType
            {
                Identity = "NavigationButton",
                EntityType = InterviewEntityType.NavigationButton.ToString()
            }
        };

        public InterviewEntityWithType[] GetPrefilledEntities()
        {
            return this.GetCallerQuestionnaire()
                .GetPrefilledQuestions()
                .Select(x => new InterviewEntityWithType
                {
                    Identity = Identity.Create(x, RosterVector.Empty).ToString(),
                    EntityType = this.GetEntityType(x).ToString()
                })
                .Union(ActionButtonsDefinition)
                .ToArray();
        }

        public InterviewEntityWithType[] GetSectionEntities(string sectionId)
        {
            if (sectionId == null) throw new ArgumentNullException(nameof(sectionId));

            Identity sectionIdentity = Identity.Parse(sectionId);
            var statefulInterview = this.GetCallerInterview();

            var ids = statefulInterview.GetUnderlyingInterviewerEntities(sectionIdentity);

            var entities = ids
                .Select(x => new InterviewEntityWithType
                {
                    Identity = x.ToString(),
                    EntityType = this.GetEntityType(x.Id).ToString()
                })
                .Union(ActionButtonsDefinition)
                .ToArray();

            return entities;
        }

        public ButtonState GetNavigationButtonState(string id)
        {
            var sectionId = CallerSectionid;

            var statefulInterview = this.GetCallerInterview();

            var sections = this.GetCallerQuestionnaire().GetAllSections()
                .Where(sec => statefulInterview.IsEnabled(Identity.Create(sec, RosterVector.Empty)))
                .ToArray();

            if (sectionId == null)
            {
                var firstSection = statefulInterview.GetGroup(Identity.Create(sections[0], RosterVector.Empty));

                return new ButtonState
                {
                    Id = id,
                    Status = CalculateSimpleStatus(firstSection.Identity, statefulInterview),
                    Title = firstSection.Title.Text,
                    Target = firstSection.Identity.ToString(),
                    Type = ButtonType.Start
                };
            }

            Identity sectionIdentity = Identity.Parse(sectionId);

            var parent = statefulInterview.GetParentGroup(sectionIdentity);
            if (parent != null)
            {
                var parentGroup = statefulInterview.GetGroup(parent);

                return new ButtonState
                {
                    Id = id,
                    Status = CalculateSimpleStatus(parent, statefulInterview),
                    Title = parentGroup.Title.Text,
                    Target = parent.ToString(),
                    Type = ButtonType.Parent
                };
            }

            var currentSectionIdx = Array.IndexOf(sections, sectionIdentity.Id);

            if (currentSectionIdx + 1 >= sections.Length)
            {
                return new ButtonState
                {
                    Id = id,
                    Title = "Complete interview",
                    Status = SimpleGroupStatus.Other,
                    Target = sectionIdentity.ToString(),
                    Type = ButtonType.Complete
                };
            }
            else
            {
                var nextSectionId = Identity.Create(sections[currentSectionIdx + 1], RosterVector.Empty);

                return new ButtonState
                {
                    Id = id,
                    Title = statefulInterview.GetGroup(nextSectionId).Title.Text,
                    Status = CalculateSimpleStatus(nextSectionId, statefulInterview),
                    Target = nextSectionId.ToString(),
                    Type = ButtonType.Next
                };
            }
        }

        public BreadcrumbInfo GetBreadcrumbs()
        {
            var sectionId = CallerSectionid;

            if (sectionId == null) return new BreadcrumbInfo();

            Identity group = Identity.Parse(sectionId);

            var statefulInterview = this.GetCallerInterview();
            var callerQuestionnaire = this.GetCallerQuestionnaire();
            ReadOnlyCollection<Guid> parentIds = callerQuestionnaire.GetParentsStartingFromTop(group.Id);

            var breadCrumbs = new List<Breadcrumb>();
            int metRosters = 0;

            foreach (Guid parentId in parentIds)
            {
                if (callerQuestionnaire.IsRosterGroup(parentId))
                {
                    metRosters++;
                    var itemRosterVector = group.RosterVector.Shrink(metRosters);
                    var itemIdentity = new Identity(parentId, itemRosterVector);
                    var breadCrumb = new Breadcrumb
                    {
                        Title = statefulInterview.GetGroup(itemIdentity).Title.Text,
                        Target = itemIdentity.ToString()
                    };

                    if (breadCrumbs.Any())
                    {
                        breadCrumbs.Last().ScrollTo = breadCrumb.Target;
                    }

                    breadCrumbs.Add(breadCrumb);
                }
                else
                {
                    var itemIdentity = new Identity(parentId, group.RosterVector.Shrink(metRosters));
                    var breadCrumb = new Breadcrumb
                    {
                        Title = statefulInterview.GetGroup(itemIdentity).Title.Text,
                        Target = itemIdentity.ToString()
                    };

                    if (breadCrumbs.Any())
                    {
                        breadCrumbs.Last().ScrollTo = breadCrumb.Target;
                    }

                    breadCrumbs.Add(breadCrumb);
                }
            }

            if (breadCrumbs.Any())
            {
                breadCrumbs.Last().ScrollTo = sectionId;
            }

            return new BreadcrumbInfo
            {
                Title = statefulInterview.GetGroup(group).Title.Text,
                Breadcrumbs = breadCrumbs.ToArray(),
                Status = CalculateSimpleStatus(group, statefulInterview).ToString()
            };
        }

        private static SimpleGroupStatus CalculateSimpleStatus(Identity group, IStatefulInterview interview)
        {
            if (interview.HasEnabledInvalidQuestionsAndStaticTexts(group))
                return SimpleGroupStatus.Invalid;

            if (interview.HasUnansweredQuestions(group))
                return SimpleGroupStatus.Other;

            bool isSomeSubgroupNotCompleted = interview
                .GetEnabledSubgroups(group)
                .Select(subgroup => CalculateSimpleStatus(subgroup, interview))
                .Any(status => status != SimpleGroupStatus.Completed);

            if (isSomeSubgroupNotCompleted)
                return SimpleGroupStatus.Other;

            return SimpleGroupStatus.Completed;
        }

        public InterviewEntity[] GetEntitiesDetails(string[] ids)
        {
            return ids.Select(GetEntityDetails).ToArray();
        }

        public InterviewEntity GetEntityDetails(string id)
        {
            if (id == "NavigationButton")
            {
                return this.GetNavigationButtonState(id);
            }

            var identity = Identity.Parse(id);
            var callerInterview = this.GetCallerInterview();

            InterviewTreeQuestion question = callerInterview.GetQuestion(identity);
            if (question != null)
            {
                GenericQuestion result = new StubEntity { Id = id, Title = question.Title.Text};

                if (question.IsSingleFixedOption)
                {
                    result = this.autoMapper.Map<InterviewSingleOptionQuestion>(question);

                    var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                    ((InterviewSingleOptionQuestion)result).Options = options;
                }
                else if (question.IsText)
                {
                    InterviewTreeQuestion textQuestion = callerInterview.GetQuestion(identity);
                    result = this.autoMapper.Map<InterviewTextQuestion>(textQuestion);
                    var textQuestionMask = this.GetCallerQuestionnaire().GetTextQuestionMask(identity.Id);
                    if (!string.IsNullOrEmpty(textQuestionMask))
                    {
                        ((InterviewTextQuestion)result).Mask = textQuestionMask;
                    }
                }
                else if (question.IsInteger)
                {
                    InterviewTreeQuestion integerQuestion = callerInterview.GetQuestion(identity);
                    var interviewIntegerQuestion = this.autoMapper.Map<InterviewIntegerQuestion>(integerQuestion);
                    var callerQuestionnaire = this.GetCallerQuestionnaire();

                    interviewIntegerQuestion.UseFormatting = callerQuestionnaire.ShouldUseFormatting(identity.Id);
                    var isRosterSize = callerQuestionnaire.ShouldQuestionSpecifyRosterSize(identity.Id);
                    interviewIntegerQuestion.IsRosterSize = isRosterSize;

                    if (isRosterSize)
                    {
                        var isRosterSizeOfLongRoster = callerQuestionnaire.IsQuestionIsRosterSizeForLongRoster(identity.Id);
                        interviewIntegerQuestion.AnswerMaxValue = isRosterSizeOfLongRoster ? Constants.MaxLongRosterRowCount : Constants.MaxRosterRowCount;
                    }

                    result = interviewIntegerQuestion;
                }
                else if (question.IsDouble)
                {
                    InterviewTreeQuestion textQuestion = callerInterview.GetQuestion(identity);
                    var interviewDoubleQuestion = this.autoMapper.Map<InterviewDoubleQuestion>(textQuestion);
                    var callerQuestionnaire = this.GetCallerQuestionnaire();
                    interviewDoubleQuestion.CountOfDecimalPlaces = callerQuestionnaire.GetCountOfDecimalPlacesAllowedByQuestion(identity.Id);
                    interviewDoubleQuestion.UseFormatting = callerQuestionnaire.ShouldUseFormatting(identity.Id);
                    result = interviewDoubleQuestion;
                }
                else if (question.IsMultiFixedOption)
                {
                    result = this.autoMapper.Map<InterviewMutliOptionQuestion>(question);

                    var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                    var typedResult = (InterviewMutliOptionQuestion)result;
                    typedResult.Options = options;
                    var callerQuestionnaire = this.GetCallerQuestionnaire();
                    typedResult.Ordered = callerQuestionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                    typedResult.MaxSelectedAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                    typedResult.IsRosterSize = callerQuestionnaire.ShouldQuestionSpecifyRosterSize(identity.Id);
                }
                else if (question.IsDateTime)
                {
                    result = this.autoMapper.Map<InterviewDateQuestion>(question);
                }                
                else if (question.IsTextList)
                {
                    result = this.autoMapper.Map<InterviewTextListQuestion>(question);
                    var typedResult = (InterviewTextListQuestion)result;
                    var callerQuestionnaire = this.GetCallerQuestionnaire();
                    typedResult.MaxAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                    typedResult.IsRosterSize = callerQuestionnaire.ShouldQuestionSpecifyRosterSize(identity.Id);
                }                
                else if (question.IsYesNo)
                {
                    var interviewYesNoQuestion = this.autoMapper.Map<InterviewYesNoQuestion>(question);
                    var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                    interviewYesNoQuestion.Options = options;
                    var callerQuestionnaire = this.GetCallerQuestionnaire();
                    interviewYesNoQuestion.Ordered = callerQuestionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                    interviewYesNoQuestion.MaxSelectedAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                    interviewYesNoQuestion.IsRosterSize = callerQuestionnaire.ShouldQuestionSpecifyRosterSize(identity.Id);

                    result = interviewYesNoQuestion;
                }

                this.PutValidationMessages(result.Validity, callerInterview, identity);
                this.PutInstructions(result, identity);
                this.PutHideIfDisabled(result, identity);

                return result;
            }

            InterviewTreeStaticText staticText = callerInterview.GetStaticText(identity);
            if (staticText != null)
            {
                InterviewStaticText result = new InterviewStaticText() { Id = id };
                result = this.autoMapper.Map<InterviewStaticText>(staticText);

                var callerQuestionnaire = this.GetCallerQuestionnaire();
                var attachment = callerQuestionnaire.GetAttachmentForEntity(identity.Id);
                if (attachment != null)
                {
                    result.AttachmentContent = attachment.ContentId;
                }

                this.PutHideIfDisabled(result, identity);
                this.PutValidationMessages(result.Validity, callerInterview, identity);

                return result;
            }

            InterviewTreeGroup @group = callerInterview.GetGroup(identity);
            if (@group != null)
            {
                var result = new InterviewGroupOrRosterInstance { Id = id };
                result = this.autoMapper.Map<InterviewGroupOrRosterInstance>(@group);

                this.PutHideIfDisabled(result, identity);

                return result;
            }

            InterviewTreeRoster @roster = callerInterview.GetRoster(identity);
            if (@roster != null)
            {
                var result = new InterviewGroupOrRosterInstance { Id = id };
                result = this.autoMapper.Map<InterviewGroupOrRosterInstance>(@roster);

                this.PutHideIfDisabled(result, identity);

                return result;
            }

            return null;
        }

        private void SidebarMapOptions(IMappingOperationOptions<InterviewTreeGroup, SidebarPanel> opts, HashSet<Identity> shownLookup)
        {
            opts.AfterMap((InterviewTreeGroup g, SidebarPanel sidebarPanel) =>
            {
                sidebarPanel.Collapsed = !shownLookup.Contains(g.Identity);
            });
        }

        public List<SidebarPanel> GetSidebarChildSectionsOf(string[] parentIds)
        {
            var sectionId = this.CallerSectionid;
            if (sectionId == null)
            {
                return null;
            }

            var interview = this.GetCallerInterview();
            var currentOpenSection = interview.GetGroup(Identity.Parse(sectionId));
            var shownPanels = currentOpenSection.Parents.Union(new[] { currentOpenSection });
            var visibleSections = new HashSet<Identity>(shownPanels.Select(p => p.Identity));

            var result = new List<SidebarPanel>();

            foreach (var parentId in parentIds)
            {
                var childs = parentId == null
                    ? interview.GetEnabledSections()
                    : interview.GetGroup(Identity.Parse(parentId))?.Children.OfType<InterviewTreeGroup>().Where(g => !g.IsDisabled());

                foreach (var child in childs ?? Array.Empty<InterviewTreeGroup>())
                {
                    var sidebar = this.autoMapper.Map<InterviewTreeGroup, SidebarPanel>(child, opts => SidebarMapOptions(opts, visibleSections));
                    result.Add(sidebar);
                }
            }

            return result;
        }

        public DropdownItem[] GetTopFilteredOptionsForQuestion(string id, string filter, int count)
        {
            var statefulInterview = this.GetCallerInterview();
            var topFilteredOptionsForQuestion = statefulInterview.GetTopFilteredOptionsForQuestion(Identity.Parse(id), null, filter, count);
            return topFilteredOptionsForQuestion.Select(x => new DropdownItem(x.Value, x.Title)).ToArray();
        }

        private void PutValidationMessages(Validity validity, IStatefulInterview callerInterview, Identity identity)
        {
            validity.Messages = callerInterview.GetFailedValidationMessages(identity).ToArray();
        }

        private void PutHideIfDisabled(InterviewEntity result, Identity identity)
        {
            result.HideIfDisabled = this.GetCallerQuestionnaire().ShouldBeHiddenIfDisabled(identity.Id);
        }

        private void PutInstructions(GenericQuestion result, Identity id)
        {
            var callerQuestionnaire = this.GetCallerQuestionnaire();

            result.Instructions = callerQuestionnaire.GetQuestionInstruction(id.Id);
            result.HideInstructions = callerQuestionnaire.GetHideInstructions(id.Id);
        }

        private InterviewEntityType GetEntityType(Guid entityId)
        {
            var callerQuestionnaire = this.GetCallerQuestionnaire();

            if (callerQuestionnaire.IsVariable(entityId)) return InterviewEntityType.Unsupported;
            if (callerQuestionnaire.HasGroup(entityId) || callerQuestionnaire.IsRosterGroup(entityId))
                return InterviewEntityType.Group;
            if (callerQuestionnaire.IsStaticText(entityId)) return InterviewEntityType.StaticText;

            switch (callerQuestionnaire.GetQuestionType(entityId))
            {
                case QuestionType.DateTime:
                    return InterviewEntityType.DateTime;
                case QuestionType.GpsCoordinates:
                    return InterviewEntityType.Unsupported; // InterviewEntityType.Gps;
                case QuestionType.Multimedia:
                    return InterviewEntityType.Unsupported; // InterviewEntityType.Multimedia;
                case QuestionType.MultyOption:
                    return callerQuestionnaire.IsQuestionYesNo(entityId)
                        ? InterviewEntityType.CategoricalYesNo
                        : InterviewEntityType.CategoricalMulti;
                case QuestionType.SingleOption:
                    return callerQuestionnaire.IsQuestionFilteredCombobox(entityId) ? InterviewEntityType.Combobox : InterviewEntityType.CategoricalSingle;
                case QuestionType.Numeric:
                    return callerQuestionnaire.IsQuestionInteger(entityId)
                        ? InterviewEntityType.Integer
                        : InterviewEntityType.Double;
                case QuestionType.Text:
                    return InterviewEntityType.TextQuestion;
                case QuestionType.TextList:
                    return InterviewEntityType.TextList;
                default:
                    return InterviewEntityType.Unsupported;
            }
        }
    }
}