using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
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

        public LanguageInfo GetLanguageInfo() => new LanguageInfo
        {
            OriginalLanguageName = Headquarters.Resources.WebInterview.Original_Language,
            Languages = this.GetCallerQuestionnaire().GetTranslationLanguages(),
            CurrentLanguage = this.GetCallerInterview().Language
        };

        public InterviewInfo GetInterviewDetails()
        {
            return new InterviewInfo
            {
                QuestionnaireTitle = this.GetCallerQuestionnaire().Title,
                InterviewId = this.statefulInterviewRepository.GetHumanInterviewId(this.CallerInterviewId),
                FirstSectionId = this.GetCallerQuestionnaire().GetFirstSectionId().FormatGuid()
            };
        }

        public bool IsEnabled(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var statefulInterview = this.GetCallerInterview();
            return statefulInterview.IsEnabled(Identity.Parse(id));
        }

        public string GetInterviewStatus()
        {
            var statefulInterview = this.GetCallerInterview();

            if (statefulInterview.CountInvalidEntitiesInInterview() > 0)
                return SimpleGroupStatus.Invalid.ToString();

            if (statefulInterview.CountActiveQuestionsInInterview() == statefulInterview.CountActiveAnsweredQuestionsInInterview())
                return SimpleGroupStatus.Completed.ToString();

            return SimpleGroupStatus.Other.ToString();
        }

        public PrefilledPageData GetPrefilledEntities()
        {
            var interviewEntityWithTypes = this.GetCallerQuestionnaire()
                .GetPrefilledQuestions()
                .Select(x => new InterviewEntityWithType
                {
                    Identity = Identity.Create(x, RosterVector.Empty).ToString(),
                    EntityType = this.GetEntityType(x).ToString()
                })
                .Union(ActionButtonsDefinition)
                .ToArray();
            var result = new PrefilledPageData
            {
                FirstSectionId = this.GetCallerQuestionnaire().GetFirstSectionId().FormatGuid(),
                Entities = interviewEntityWithTypes,
                HasAnyQuestions = interviewEntityWithTypes.Length > 1
            };

            return result;
        }

        public InterviewEntityWithType[] GetSectionEntities(string sectionId)
        {
            if (sectionId == null) throw new ArgumentNullException(nameof(sectionId));

            Identity sectionIdentity = Identity.Parse(sectionId);
            var statefulInterview = this.GetCallerInterview();

            var ids = statefulInterview.GetUnderlyingInterviewerEntities(sectionIdentity);
            var questionarie = this.GetCallerQuestionnaire();
            var entities = ids
                .Where(id => !questionarie.IsVariable(id.Id))
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
                    var treeGroup = statefulInterview.GetGroup(itemIdentity);
                    var breadCrumb = new Breadcrumb
                    {
                        Title = treeGroup.Title.Text,
                        RosterTitle = (treeGroup as InterviewTreeRoster)?.RosterTitle,
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

            var currentTreeGroup = statefulInterview.GetGroup(group);
            return new BreadcrumbInfo
            {
                Title = currentTreeGroup.Title.Text,
                RosterTitle = (currentTreeGroup as InterviewTreeRoster)?.RosterTitle,
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

        private static readonly Regex HtmlRemovalRegex = new Regex(Constants.HtmlRemovalPattern, RegexOptions.Compiled);

        public InterviewEntity GetEntityDetails(string id)
        {
            if (id == "NavigationButton")
            {
                return this.GetNavigationButtonState(id);
            }

            var identity = Identity.Parse(id);
            var callerInterview = this.GetCallerInterview();

            InterviewTreeQuestion question = callerInterview.GetQuestion(identity);
            var questionnaire = this.GetCallerQuestionnaire();
            if (question != null)
            {
                GenericQuestion result = this.Map<StubEntity>(question);

                if (question.IsSingleFixedOption)
                {
                    if (questionnaire.IsQuestionFilteredCombobox(identity.Id) || question.IsCascading)
                    {
                        result = this.Map<InterviewFilteredQuestion>(question);
                    }
                    else
                    {
                        result = this.Map<InterviewSingleOptionQuestion>(question, res =>
                        {
                            res.Options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                        });
                    }
                }
                else if (question.IsText)
                {
                    InterviewTreeQuestion textQuestion = callerInterview.GetQuestion(identity);
                    result = this.autoMapper.Map<InterviewTextQuestion>(textQuestion);
                    var textQuestionMask = questionnaire.GetTextQuestionMask(identity.Id);
                    if (!string.IsNullOrEmpty(textQuestionMask))
                    {
                        ((InterviewTextQuestion)result).Mask = textQuestionMask;
                    }
                }
                else if (question.IsInteger)
                {
                    InterviewTreeQuestion integerQuestion = callerInterview.GetQuestion(identity);
                    var interviewIntegerQuestion = this.autoMapper.Map<InterviewIntegerQuestion>(integerQuestion);
                    var callerQuestionnaire = questionnaire;

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
                    var callerQuestionnaire = questionnaire;
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
                    var callerQuestionnaire = questionnaire;
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
                    var callerQuestionnaire = questionnaire;
                    typedResult.MaxAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                    typedResult.IsRosterSize = callerQuestionnaire.ShouldQuestionSpecifyRosterSize(identity.Id);
                }
                else if (question.IsYesNo)
                {
                    var interviewYesNoQuestion = this.autoMapper.Map<InterviewYesNoQuestion>(question);
                    var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                    interviewYesNoQuestion.Options = options;
                    var callerQuestionnaire = questionnaire;
                    interviewYesNoQuestion.Ordered = callerQuestionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                    interviewYesNoQuestion.MaxSelectedAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                    interviewYesNoQuestion.IsRosterSize = callerQuestionnaire.ShouldQuestionSpecifyRosterSize(identity.Id);

                    result = interviewYesNoQuestion;
                }
                else if (question.IsGps)
                {
                    result = this.autoMapper.Map<InterviewGpsQuestion>(question);
                }
                else if (question.IsSingleLinkedOption || question.IsSingleLinkedToList)
                {
                    var singleLinkedOption = this.autoMapper.Map<InterviewLinkedSingleQuestion>(question);
                    var options = question.IsSingleLinkedToList 
                        ? GetLinkedOptionsForLinkedToListQuestion(callerInterview, identity, question.AsSingleLinkedToList.LinkedSourceId, question.AsSingleLinkedToList.Options) 
                        : GetLinkedOptionsForLinkedQuestion(callerInterview, identity, question.AsLinked.Options);

                    singleLinkedOption.IsLinkedToList = question.IsSingleLinkedToList;
                    singleLinkedOption.Options = options.ToList();
                    result = singleLinkedOption;
                }
                else if (question.IsMultiLinkedOption || question.IsMultiLinkedToList)
                {
                    var multiLinkedOption = this.autoMapper.Map<InterviewLinkedMultiQuestion>(question);
                    var options = question.IsMultiLinkedToList 
                        ? GetLinkedOptionsForLinkedToListQuestion(callerInterview, identity, question.AsMultiLinkedToList.LinkedSourceId, question.AsMultiLinkedToList.Options) 
                        : GetLinkedOptionsForLinkedQuestion(callerInterview, identity, question.AsLinked.Options);

                    multiLinkedOption.IsLinkedToList = question.IsMultiLinkedToList;
                    multiLinkedOption.Ordered = questionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                    multiLinkedOption.MaxSelectedAnswersCount = questionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                    multiLinkedOption.Options = options.ToList();
                    result = multiLinkedOption;
                }
                else if (question.IsMultimedia)
                {
                    result = Map<InterviewMultimediaQuestion>(question);
                }

                this.PutValidationMessages(result.Validity, callerInterview, identity);
                this.PutInstructions(result, identity);
                this.ApplyDisablement(result, identity);

                return result;
            }

            InterviewTreeStaticText staticText = callerInterview.GetStaticText(identity);
            if (staticText != null)
            {
                InterviewStaticText result = new InterviewStaticText() { Id = id };
                result = this.autoMapper.Map<InterviewStaticText>(staticText);

                var callerQuestionnaire = questionnaire;
                var attachment = callerQuestionnaire.GetAttachmentForEntity(identity.Id);
                if (attachment != null)
                {
                    result.AttachmentContent = attachment.ContentId;
                }

                this.ApplyDisablement(result, identity);
                this.PutValidationMessages(result.Validity, callerInterview, identity);

                return result;
            }

            InterviewTreeGroup @group = callerInterview.GetGroup(identity);
            if (@group != null)
            {
                var result = new InterviewGroupOrRosterInstance { Id = id };
                result = this.autoMapper.Map<InterviewGroupOrRosterInstance>(@group);

                this.ApplyDisablement(result, identity);
                return result;
            }

            InterviewTreeRoster @roster = callerInterview.GetRoster(identity);
            if (@roster != null)
            {
                var result = new InterviewGroupOrRosterInstance { Id = id };
                result = this.autoMapper.Map<InterviewGroupOrRosterInstance>(@roster);

                this.ApplyDisablement(result, identity);
                return result;
            }

            return null;
        }

        private T Map<T>(InterviewTreeQuestion question, Action<T> afterMap = null)
        {
            return this.autoMapper.Map<InterviewTreeQuestion, T>(question, opts => opts.AfterMap((treeQuestion, target) => afterMap?.Invoke(target)));
        }

        public List<SidebarPanel> GetSidebarChildSectionsOf(string[] parentIds)
        {
            var sectionId = this.CallerSectionid;
            var interview = this.GetCallerInterview();
            var result = new List<SidebarPanel>();

            HashSet<Identity> visibleSections = new HashSet<Identity>();

            if (sectionId != null)
            {
                var currentOpenSection = interview.GetGroup(Identity.Parse(sectionId));
                var shownPanels = currentOpenSection.Parents.Union(new[] {currentOpenSection});
                visibleSections = new HashSet<Identity>(shownPanels.Select(p => p.Identity));
            }
            foreach (var parentId in parentIds)
            {
                var children = parentId == null
                    ? interview.GetEnabledSections()
                    : interview.GetGroup(Identity.Parse(parentId))?.Children.OfType<InterviewTreeGroup>().Where(g => !g.IsDisabled());

                foreach (var child in children ?? Array.Empty<InterviewTreeGroup>())
                {
                    var sidebar = this.autoMapper.Map<InterviewTreeGroup, SidebarPanel>(child, opts => SidebarMapOptions(opts, visibleSections));
                    result.Add(sidebar);
                }
            }

            return result;
        }

        private void SidebarMapOptions(IMappingOperationOptions<InterviewTreeGroup, SidebarPanel> opts, HashSet<Identity> shownLookup)
        {
            opts.AfterMap((g, sidebarPanel) =>
            {
                sidebarPanel.Collapsed = !shownLookup.Contains(g.Identity);
                sidebarPanel.Current = shownLookup.Contains(g.Identity);
            });
        }

        public DropdownItem[] GetTopFilteredOptionsForQuestion(string id, string filter, int count)
        {
            var questionIdentity = Identity.Parse(id);
            var statefulInterview = this.GetCallerInterview();
            var question = statefulInterview.GetQuestion(questionIdentity);
            var parentCascadingQuestion = question.AsCascading?.GetCascadingParentQuestion();
            var parentCascadingQuestionAnswer = parentCascadingQuestion?.IsAnswered ?? false
                ? parentCascadingQuestion?.GetAnswer()?.SelectedValue
                : null;

            var topFilteredOptionsForQuestion = statefulInterview.GetTopFilteredOptionsForQuestion(questionIdentity, parentCascadingQuestionAnswer, filter, count);
            return topFilteredOptionsForQuestion.Select(x => new DropdownItem(x.Value, x.Title)).ToArray();
        }

        public CompleteInfo GetCompleteInfo()
        {
            var interview = this.GetCallerInterview();

            var questionsCount = interview.CountActiveQuestionsInInterview();
            var answeredQuestionsCount = interview.CountActiveAnsweredQuestionsInInterview();
            var invalidAnswersCount = interview.CountInvalidEntitiesInInterview();
            Identity[] invalidEntityIds = interview.GetInvalidEntitiesInInterview().Take(30).ToArray();
            var invalidEntities = invalidEntityIds.Select(identity =>
            {
                var titleText = interview.GetTitleText(identity);
                var parentId = interview.IsQuestionPrefilled(identity) ? "prefilled" : interview.GetParentGroup(identity).ToString();
                return new EntityWithError
                {
                    Id = identity.ToString(),
                    ParentId = parentId,
                    Title = titleText
                };

            }).ToArray();

            var completeInfo = new CompleteInfo
            {
                AnsweredCount = answeredQuestionsCount,
                ErrorsCount = invalidAnswersCount,
                UnansweredCount = questionsCount - answeredQuestionsCount,

                EntitiesWithError = invalidEntities
            };
            return completeInfo;
        }

        private void PutValidationMessages(Validity validity, IStatefulInterview callerInterview, Identity identity)
        {
            validity.Messages = callerInterview.GetFailedValidationMessages(identity).ToArray();
        }

        private void ApplyDisablement(InterviewEntity result, Identity identity)
        {
            if (result.IsDisabled)
            {
                result.Title = HtmlRemovalRegex.Replace(result.Title, string.Empty);
            }

            result.HideIfDisabled = this.GetCallerQuestionnaire().ShouldBeHiddenIfDisabled(identity.Id);
        }

        private void PutInstructions(GenericQuestion result, Identity id)
        {
            var callerQuestionnaire = this.GetCallerQuestionnaire();

            result.Instructions = callerQuestionnaire.GetQuestionInstruction(id.Id);
            result.HideInstructions = callerQuestionnaire.GetHideInstructions(id.Id);
        }

        private static IEnumerable<LinkedOption> GetLinkedOptionsForLinkedQuestion(IStatefulInterview callerInterview,
            Identity identity, List<RosterVector> options)
        {
            return options.Select(x => new LinkedOption
            {
                Value = x.ToString(),
                RosterVector = x.Select(Convert.ToInt32).ToArray(),
                Title = callerInterview.GetLinkedOptionTitle(identity, x)
            });
        }

        private static IEnumerable<LinkedOption> GetLinkedOptionsForLinkedToListQuestion(IStatefulInterview callerInterview,
            Identity identity, Guid linkedSourceId, IReadOnlyCollection<decimal> options)
        {
            var listQuestion = callerInterview.FindQuestionInQuestionBranch(linkedSourceId, identity)?.AsTextList;
            return options.Select(x => new LinkedOption
            {
                Value = x.ToString(),
                RosterVector = Convert.ToInt32(x).ToEnumerable().ToArray(),
                Title = listQuestion?.GetTitleByItemCode(x)
            });
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
                    return InterviewEntityType.Gps;
                case QuestionType.Multimedia:
                    return InterviewEntityType.Multimedia; // InterviewEntityType.Multimedia;
                case QuestionType.MultyOption:
                    if (callerQuestionnaire.IsQuestionLinked(entityId)
                        || callerQuestionnaire.IsLinkedToListQuestion(entityId)
                        || callerQuestionnaire.IsQuestionLinkedToRoster(entityId))
                        return InterviewEntityType.LinkedMulti;
                    return callerQuestionnaire.IsQuestionYesNo(entityId)
                        ? InterviewEntityType.CategoricalYesNo
                        : InterviewEntityType.CategoricalMulti;
                case QuestionType.SingleOption:
                    if (callerQuestionnaire.IsQuestionLinked(entityId)
                        || callerQuestionnaire.IsLinkedToListQuestion(entityId)
                        || callerQuestionnaire.IsQuestionLinkedToRoster(entityId))
                        return InterviewEntityType.LinkedSingle;
                    return callerQuestionnaire.IsQuestionFilteredCombobox(entityId) || callerQuestionnaire.IsQuestionCascading(entityId)
                        ? InterviewEntityType.Combobox
                        : InterviewEntityType.CategoricalSingle;
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