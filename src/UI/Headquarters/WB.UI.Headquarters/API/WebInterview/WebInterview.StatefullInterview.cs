using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.UI.Headquarters.Models.WebInterview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Headquarters.Resources;
using GpsAnswer = WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers.GpsAnswer;

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
            var statefulInterview = this.GetCallerInterview();
            return new InterviewInfo
            {
                QuestionnaireTitle = this.GetCallerQuestionnaire().Title,
                FirstSectionId = this.GetCallerQuestionnaire().GetFirstSectionId().FormatGuid(),
                InterviewKey = statefulInterview.GetInterviewKey().ToString()
            };
        }

        public bool IsEnabled(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var statefulInterview = this.GetCallerInterview();
            return statefulInterview.IsEnabled(Identity.Parse(id));
        }

        private SimpleGroupStatus GetInterviewSimpleStatus()
        {
            var statefulInterview = this.GetCallerInterview();

            if (statefulInterview.CountInvalidEntitiesInInterview() > 0)
                return SimpleGroupStatus.Invalid;

            return (statefulInterview.CountActiveQuestionsInInterview() == statefulInterview.CountActiveAnsweredQuestionsInInterview())
                ? SimpleGroupStatus.Completed 
                : SimpleGroupStatus.Other;
        }


        public string GetInterviewStatus()
        {
            return GetInterviewSimpleStatus().ToString();
        }

        private IdentifyingQuestion GetIdentifyingQuestion(Guid questionId, IStatefulInterview interview)
        {
            var result = new IdentifyingQuestion();
            var entityType = this.GetEntityType(questionId);
            result.Type = entityType.ToString();
            var questionIdentity = new Identity(questionId, RosterVector.Empty);
            result.Identity = questionIdentity.ToString();
            var interviewQuestion = interview.GetQuestion(questionIdentity);

            result.IsReadonly = interviewQuestion.IsReadonly;
            result.Title = interviewQuestion.Title.BrowserReadyText;

            if (entityType == InterviewEntityType.Gps)
            {
                GpsAnswer questionAnswer = interviewQuestion.GetAsGpsAnswer();
                string answer = questionAnswer?.Value != null ? $"{questionAnswer.Value.Latitude},{questionAnswer.Value.Longitude}" : null;
                result.Answer = answer;
            }
            if (entityType == InterviewEntityType.DateTime && ((InterviewTreeDateTimeQuestion)interviewQuestion.InterviewQuestion).IsTimestamp)
            {
                DateTimeAnswer questionAnswer = interviewQuestion.GetAsDateTimeAnswer();
                string answer = questionAnswer?.Value != null
                    ? $"<time datetime=\"{questionAnswer.Value:o}\">{interview.GetAnswerAsString(questionIdentity)}</time>"
                    : null;
                result.Answer = answer;
            }
            else
            {
                result.Answer = interview.GetAnswerAsString(questionIdentity, CultureInfo.InvariantCulture);
            }

            return result;
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
                var parentRoster = parentGroup as InterviewTreeRoster;

                return new ButtonState
                {
                    Id = id,
                    Status = CalculateSimpleStatus(parent, statefulInterview),
                    Title = parentGroup.Title.Text,
                    RosterTitle = parentRoster?.RosterTitle,
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
                    Status = GetInterviewSimpleStatus(),
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
                        Target = itemIdentity.ToString(),
                        IsRoster = true
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
            var currentTreeGroupAsRoster = currentTreeGroup as InterviewTreeRoster;

            if (currentTreeGroup == null)
            {
                webInterviewNotificationService.ReloadInterview(Guid.Parse(this.CallerInterviewId));
            }

            return new BreadcrumbInfo
            {
                Title = currentTreeGroup?.Title.Text,
                RosterTitle = (currentTreeGroupAsRoster)?.RosterTitle,
                Breadcrumbs = breadCrumbs.ToArray(),
                Status = CalculateSimpleStatus(group, statefulInterview).ToString(),
                IsRoster = currentTreeGroupAsRoster != null
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
            var callerInterview = this.GetCallerInterview();
            return ids.Select(id => GetEntityDetails(id, callerInterview)).ToArray();
        }

        private static readonly Regex HtmlRemovalRegex = new Regex(Constants.HtmlRemovalPattern, RegexOptions.Compiled);

        private InterviewEntity GetEntityDetails(string id, IStatefulInterview callerInterview)
        {
            if (id == "NavigationButton")
            {
                return this.GetNavigationButtonState(id);
            }

            var identity = Identity.Parse(id);
            
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
                else if(question.IsSingleLinkedToList)
                {
                    result = this.Map<InterviewSingleOptionQuestion>(question, res =>
                    {
                        res.Options = GetOptionsLinkedToListQuestion(callerInterview, identity,
                            ((InterviewTreeSingleOptionLinkedToListQuestion)question.InterviewQuestion).LinkedSourceId).ToList();
                    });
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
                else if (question.IsMultiLinkedToList)
                {
                    result = this.Map<InterviewMutliOptionQuestion>(question, res =>
                    {
                        res.Options = GetOptionsLinkedToListQuestion(callerInterview, identity,
                            ((InterviewTreeMultiOptionLinkedToListQuestion)question.InterviewQuestion).LinkedSourceId).ToList();
                    });
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
                    typedResult.MaxAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id) ?? 200;
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
                else if (question.IsSingleLinkedOption)
                {
                    result = this.Map<InterviewLinkedSingleQuestion>(question, res =>
                    {
                       res.Options = GetLinkedOptionsForLinkedQuestion(callerInterview, identity, question.AsLinked.Options).ToList();
                    });
                }
                else if (question.IsMultiLinkedOption)
                {
                    result = this.Map<InterviewLinkedMultiQuestion>(question, res =>
                    {
                        res.Options = GetLinkedOptionsForLinkedQuestion(callerInterview, identity, question.AsLinked.Options).ToList();
                        res.Ordered = questionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                        res.MaxSelectedAnswersCount = questionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                    });
                }
                else if (question.IsMultimedia)
                {
                    result = Map<InterviewMultimediaQuestion>(question);
                }
                else if (question.IsQRBarcode)
                {
                    InterviewTreeQuestion barcodeQuestion = callerInterview.GetQuestion(identity);
                    result = this.autoMapper.Map<InterviewBarcodeQuestion>(barcodeQuestion);
                }
                else if (question.IsAudio)
                {
                    InterviewTreeQuestion audioQuestion = callerInterview.GetQuestion(identity);
                    result = this.autoMapper.Map<InterviewAudioQuestion>(audioQuestion);
                }

                this.PutValidationMessages(result.Validity, callerInterview, identity);
                this.PutInstructions(result, identity);
                this.ApplyDisablement(result, identity);
                result.Comments = this.GetComments(question, callerInterview);

                return result;
            }

            InterviewTreeStaticText staticText = callerInterview.GetStaticText(identity);
            if (staticText != null)
            {
                InterviewStaticText result = this.autoMapper.Map<InterviewTreeStaticText, InterviewStaticText>(staticText, map =>
                {
                    map.AfterMap((text, interviewStaticText) =>
                    {
                        var callerQuestionnaire = questionnaire;
                        var attachment = callerQuestionnaire.GetAttachmentForEntity(identity.Id);
                        if (attachment != null)
                        {
                            interviewStaticText.AttachmentContent = attachment.ContentId;
                        }
                    });
                });

                this.ApplyDisablement(result, identity);
                this.PutValidationMessages(result.Validity, callerInterview, identity);
                return result;
            }

            InterviewTreeGroup @group = callerInterview.GetGroup(identity);
            if (@group != null)
            {
                var result = this.autoMapper.Map<InterviewGroupOrRosterInstance>(@group);

                this.ApplyDisablement(result, identity);
                return result;
            }

            InterviewTreeRoster @roster = callerInterview.GetRoster(identity);
            if (@roster != null)
            {
                var result = this.autoMapper.Map<InterviewGroupOrRosterInstance>(@roster);

                this.ApplyDisablement(result, identity);
                return result;
            }

            return null;
        }

        private T Map<T>(InterviewTreeQuestion question, Action<T> afterMap = null)
        {
            return this.autoMapper.Map<InterviewTreeQuestion, T>(question, opts => opts.AfterMap((treeQuestion, target) => afterMap?.Invoke(target)));
        }

        public bool HasCoverPage()
        {
            var interview = this.GetCallerInterview();

            return this.GetCallerQuestionnaire().GetPrefilledQuestions().Any() 
                || interview.GetAllCommentedEnabledQuestions().Any()
                || !string.IsNullOrWhiteSpace(interview.SupervisorRejectComment);
        }

        public Sidebar GetSidebarChildSectionsOf(string[] parentIds)
        {
            var sectionId = this.CallerSectionid;
            var interview = this.GetCallerInterview();
            Sidebar result = new Sidebar();
            HashSet<Identity> visibleSections = new HashSet<Identity>();

            if (sectionId != null)
            {
                var currentOpenSection = interview.GetGroup(Identity.Parse(sectionId));
                
                //roster instance could be removed
                if (currentOpenSection != null)
                {
                    var shownPanels = currentOpenSection.Parents.Union(new[] {currentOpenSection});
                    visibleSections = new HashSet<Identity>(shownPanels.Select(p => p.Identity));
                }
            }
            foreach (var parentId in parentIds.Distinct())
            {
                var children = parentId == null
                    ? interview.GetEnabledSections()
                    : interview.GetGroup(Identity.Parse(parentId))?.Children.OfType<InterviewTreeGroup>().Where(g => !g.IsDisabled());

                foreach (var child in children ?? Array.Empty<InterviewTreeGroup>())
                {
                    var sidebar = this.autoMapper.Map<InterviewTreeGroup, SidebarPanel>(child, opts => SidebarMapOptions(opts, visibleSections));
                    result.Groups.Add(sidebar);
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
            var parentCascadingQuestion = (question.InterviewQuestion as InterviewTreeCascadingQuestion)?.GetCascadingParentQuestion();
            var parentCascadingQuestionAnswer = parentCascadingQuestion?.IsAnswered() ?? false
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
                var titleText = HtmlRemovalRegex.Replace(interview.GetTitleText(identity), string.Empty);
                var isPrefilled = interview.IsQuestionPrefilled(identity);
                var parentId = interview.GetParentGroup(identity).ToString();
                return new EntityWithError
                {
                    Id = identity.ToString(),
                    ParentId = parentId,
                    Title = titleText,
                    IsPrefilled = isPrefilled
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

        public Comment[] GetQuestionComments(string questionId)
        {
            var identity = Identity.Parse(questionId);

            var interview = this.GetCallerInterview();
            InterviewTreeQuestion question = interview.GetQuestion(identity);

            return GetComments(question, interview);
        }

        public CoverInfo GetCoverInfo()
        {
            var interview = this.GetCallerInterview();
            var commentedQuestionsCount = interview.CountCommentedQuestionsVisibledToInterviewer();
            var commentedQuestions = interview.GetCommentedBySupervisorQuestionsVisibledToInterviewer().Take(30).ToArray();

            var entitiesWithComments = commentedQuestions.Select(identity =>
            {
                var titleText = HtmlRemovalRegex.Replace(interview.GetTitleText(identity), string.Empty);
                var isPrefilled = interview.IsQuestionPrefilled(identity);
                var parentId = interview.GetParentGroup(identity).ToString();
                return new EntityWithComment
                {
                    Id = identity.ToString(),
                    ParentId = parentId,
                    Title = titleText,
                    IsPrefilled = isPrefilled
                };

            }).ToArray();

            var interviewEntityWithTypes = this.GetCallerQuestionnaire()
                .GetPrefilledQuestions()
                .Select(x => this.GetIdentifyingQuestion(x, interview))
                .ToList();
            

            var completeInfo = new CoverInfo
            {
                EntitiesWithComments = entitiesWithComments,
                IdentifyingQuestions = interviewEntityWithTypes,
                CommentedQuestionsCount = commentedQuestionsCount,
                SupervisorRejectComment = interview.SupervisorRejectComment
            };
            return completeInfo;
        }

        private void PutValidationMessages(Validity validity, IStatefulInterview callerInterview, Identity identity)
        {
            validity.Messages = callerInterview.GetFailedValidationMessages(identity, Strings.Error)
                .ToArray();
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

        private Comment[] GetComments(InterviewTreeQuestion question, IStatefulInterview statefulInterview)
        {
            return question.AnswerComments.Select(ac 
                => new Comment()
                {
                    Text = ac.Comment,
                    IsOwnComment = ac.UserId == statefulInterview.CurrentResponsibleId,
                    UserRole = ac.UserRole,
                    CommentTimeUtc = ac.CommentTime
                })
                .ToArray();
        }

        private static IEnumerable<LinkedOption> GetLinkedOptionsForLinkedQuestion(IStatefulInterview callerInterview,
            Identity identity, List<RosterVector> options) => options.Select(x => new LinkedOption
        {
            Value = x.ToString(),
            RosterVector = x,
            Title = callerInterview.GetLinkedOptionTitle(identity, x)
        });

        private static IEnumerable<CategoricalOption> GetOptionsLinkedToListQuestion(IStatefulInterview callerInterview,
            Identity identity, Guid linkedSourceId)
        {
            var listQuestion = callerInterview.FindQuestionInQuestionBranch(linkedSourceId, identity);

            if ((listQuestion == null) || listQuestion.IsDisabled() || listQuestion.GetAsTextListAnswer()?.Rows == null)
                return new List<CategoricalOption>();

            return new List<CategoricalOption>(listQuestion.GetAsTextListAnswer().Rows.Select(x => new CategoricalOption
            {
                Value = (int)x.Value,
                Title = x.Text
            }));
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
                    if (callerQuestionnaire.IsLinkedToListQuestion(entityId))
                        return InterviewEntityType.CategoricalMulti;
                    if (callerQuestionnaire.IsQuestionLinked(entityId)
                        || callerQuestionnaire.IsQuestionLinkedToRoster(entityId))
                        return InterviewEntityType.LinkedMulti;
                    return callerQuestionnaire.IsQuestionYesNo(entityId)
                        ? InterviewEntityType.CategoricalYesNo
                        : InterviewEntityType.CategoricalMulti;
                case QuestionType.SingleOption:
                    if (callerQuestionnaire.IsLinkedToListQuestion(entityId))
                        return InterviewEntityType.CategoricalSingle;
                    if (callerQuestionnaire.IsQuestionLinked(entityId)
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
                case QuestionType.QRBarcode:
                    return InterviewEntityType.QRBarcode;
                case QuestionType.Audio:
                    return InterviewEntityType.Audio;
                default:
                    return InterviewEntityType.Unsupported;
            }
        }
    }
}