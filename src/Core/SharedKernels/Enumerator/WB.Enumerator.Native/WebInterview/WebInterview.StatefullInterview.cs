using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview.Models;
using GpsAnswer = WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers.GpsAnswer;

namespace WB.Enumerator.Native.WebInterview
{
    public partial class WebInterview
    {
        private static InterviewEntityWithType[] ActionButtonsDefinition { get; } = {
            new InterviewEntityWithType
            {
                Identity = @"NavigationButton",
                EntityType = InterviewEntityType.NavigationButton.ToString()
            }
        };

        public LanguageInfo GetLanguageInfo()
        {
            var statefulInterview = this.GetCallerInterview();
            if (statefulInterview == null) return null;

            return new LanguageInfo
            {
                OriginalLanguageName = Resources.WebInterview.Original_Language,
                Languages = this.GetCallerQuestionnaire().GetTranslationLanguages(),
                CurrentLanguage = statefulInterview.Language
            };
        }

        public virtual InterviewInfo GetInterviewDetails()
        {
            var statefulInterview = this.GetCallerInterview();
            if (statefulInterview == null) return null;

            var questionnaire = this.GetCallerQuestionnaire();

            return new InterviewInfo
            {
                QuestionnaireTitle = IsReviewMode 
                    ? string.Format(Resources.WebInterview.QuestionnaireNameFormat, questionnaire.Title, questionnaire.Version) 
                    : questionnaire.Title,
                FirstSectionId = questionnaire.GetFirstSectionId().FormatGuid(),
                InterviewKey = statefulInterview.GetInterviewKey().ToString(),
                InterviewCannotBeChanged = statefulInterview.ReceivedByInterviewer || this.IsCurrentUserObserving,
                ReceivedByInterviewer = statefulInterview.ReceivedByInterviewer,
                IsCurrentUserObserving = this.IsCurrentUserObserving,
                DoesBrokenPackageExist = false
            };
        }

        public bool IsEnabled(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var statefulInterview = this.GetCallerInterview();
            return statefulInterview.IsEnabled(Identity.Parse(id));
        }

        public GroupStatus GetInterviewStatus()
        {
            var interview = this.GetCallerInterview();
            if (interview == null) return GroupStatus.Invalid;

            return this.interviewEntityFactory.GetInterviewSimpleStatus(interview, IsReviewMode);
        }

        private IdentifyingQuestion GetIdentifyingQuestion(Guid questionId, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            var result = new IdentifyingQuestion();
            var entityType = this.GetEntityType(questionId, questionnaire);

            result.Type = entityType.ToString();
            var questionIdentity = new Identity(questionId, RosterVector.Empty);
            result.Identity = questionIdentity.ToString();
            var interviewQuestion = interview.GetQuestion(questionIdentity);

            result.IsReadonly = interviewQuestion.IsReadonly;
            result.Title = interviewQuestion.Title.BrowserReadyText;

            if (entityType == InterviewEntityType.Gps)
            {
                GpsAnswer questionAnswer = interviewQuestion.GetAsInterviewTreeGpsQuestion().GetAnswer();
                string answer = questionAnswer?.Value != null ? $@"{questionAnswer.Value.Latitude},{questionAnswer.Value.Longitude}" : null;
                result.Answer = answer;
            }
            else if (entityType == InterviewEntityType.DateTime && (interviewQuestion.GetAsInterviewTreeDateTimeQuestion()).IsTimestamp)
            {
                DateTimeAnswer questionAnswer = interviewQuestion.GetAsInterviewTreeDateTimeQuestion().GetAnswer();
                string answer = questionAnswer?.Value != null
                    ? $@"<time datetime=""{questionAnswer.Value:o}"">{interview.GetAnswerAsString(questionIdentity)}</time>"
                    : null;
                result.Answer = answer;
            }
            else
            {
                result.Answer = interview.GetAnswerAsString(questionIdentity, CultureInfo.InvariantCulture);
            }

            return result;
        }
        
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public PrefilledPageData GetPrefilledEntities()
        {
            var questionnaire = this.GetCallerQuestionnaire();

            var interviewEntityWithTypes = questionnaire
                .GetPrefilledQuestions()
                .Select(x => new InterviewEntityWithType
                {
                    Identity = Identity.Create(x, RosterVector.Empty).ToString(),
                    EntityType = this.GetEntityType(x, questionnaire).ToString()
                })
                .Union(ActionButtonsDefinition)
                .ToArray();

            var result = new PrefilledPageData
            {
                FirstSectionId = questionnaire.GetFirstSectionId().FormatGuid(),
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
            if (statefulInterview == null) return null;

            var ids = IsReviewMode ? statefulInterview.GetUnderlyingEntitiesForReview(sectionIdentity) : 
                                     statefulInterview.GetUnderlyingInterviewerEntities(sectionIdentity);
            var questionarie = this.GetCallerQuestionnaire();

            var entities = ids
                .Select(x => new InterviewEntityWithType
                {
                    Identity = x.ToString(),
                    EntityType = this.GetEntityType(x.Id, questionarie).ToString()
                })
                .Union(ActionButtonsDefinition)
                .ToArray();

            return entities;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public SectionData GetFullSectionInfo(string sectionId)
        {
            var entities = GetSectionEntities(sectionId);

            var details = GetEntitiesDetails(entities.Select(e => e.Identity).ToArray());

            return new SectionData
            {
                Entities = entities,
                Details = details
            };
        }

        public ButtonState GetNavigationButtonState(string id, IQuestionnaire questionnaire = null)
        {
            var statefulInterview = this.GetCallerInterview();
            if (statefulInterview == null) return null;

            ButtonState NewButtonState(ButtonState button, InterviewTreeGroup target)
            {
                button.Id = id;
                button.Target = target.Identity.ToString();
                button.Status = button.Type == ButtonType.Complete
                    ? this.interviewEntityFactory.GetInterviewSimpleStatus(statefulInterview, IsReviewMode)
                    : this.interviewEntityFactory.CalculateSimpleStatus(target, IsReviewMode);

                this.interviewEntityFactory.ApplyValidity(button.Validity, target, IsReviewMode);

                return button;
            }

            var sectionId = CallerSectionid;
            var callerQuestionnaire = questionnaire ?? this.GetCallerQuestionnaire();
            
            var sections = callerQuestionnaire.GetAllSections()
                .Where(sec => statefulInterview.IsEnabled(Identity.Create(sec, RosterVector.Empty)))
                .ToArray();

            if (sectionId == null)
            {
                var firstSection = statefulInterview.GetGroup(Identity.Create(sections[0], RosterVector.Empty));

                return NewButtonState(new ButtonState
                {
                    Title = firstSection.Title.Text,
                    Type = ButtonType.Start
                }, firstSection);
            }

            Identity sectionIdentity = Identity.Parse(sectionId);

            var parent = statefulInterview.GetParentGroup(sectionIdentity);
            if (parent != null)
            {
                var parentGroup = statefulInterview.GetGroup(parent);
                var parentRoster = parentGroup as InterviewTreeRoster;

                return NewButtonState(new ButtonState
                {
                    Title = parentGroup.Title.Text,
                    RosterTitle = parentRoster?.RosterTitle,
                    Type = ButtonType.Parent
                }, parentGroup);
            }

            var currentSectionIdx = Array.IndexOf(sections, sectionIdentity.Id);

            if (currentSectionIdx + 1 >= sections.Length)
            {
                return NewButtonState(new ButtonState
                {
                    Title = Resources.WebInterview.CompleteInterview,
                    Type = ButtonType.Complete
                }, statefulInterview.GetGroup(sectionIdentity));
            }

            var nextSectionId = Identity.Create(sections[currentSectionIdx + 1], RosterVector.Empty);

            return NewButtonState(new ButtonState
            {
                Title = statefulInterview.GetGroup(nextSectionId).Title.Text,
                Type = ButtonType.Next
            }, statefulInterview.GetGroup(nextSectionId));
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public BreadcrumbInfo GetBreadcrumbs(string sectionIdArg)
        {
            var sectionId = CallerSectionid ?? sectionIdArg;

            if (sectionId == null) return new BreadcrumbInfo();

            Identity groupId = Identity.Parse(sectionId);

            var statefulInterview = this.GetCallerInterview();
            if (statefulInterview == null) return null;

            var callerQuestionnaire = this.GetCallerQuestionnaire();
            ReadOnlyCollection<Guid> parentIds = callerQuestionnaire.GetParentsStartingFromTop(groupId.Id);

            var breadCrumbs = new List<Breadcrumb>();
            int metRosters = 0;

            foreach (Guid parentId in parentIds)
            {
                if (callerQuestionnaire.IsRosterGroup(parentId))
                {
                    metRosters++;
                    var itemRosterVector = groupId.RosterVector.Shrink(metRosters);
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
                    var itemIdentity = new Identity(parentId, groupId.RosterVector.Shrink(metRosters));

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

            var currentTreeGroup = statefulInterview.GetGroup(groupId);
            var currentTreeGroupAsRoster = currentTreeGroup as InterviewTreeRoster;

            if (currentTreeGroup == null)
            {
                this.webInterviewNotificationService.ReloadInterview(Guid.Parse(CallerInterviewId));
            }

            var info = new BreadcrumbInfo
            {
                Title = currentTreeGroup?.Title.Text,
                RosterTitle = (currentTreeGroupAsRoster)?.RosterTitle,
                Breadcrumbs = breadCrumbs.ToArray(),
                Status = this.interviewEntityFactory.CalculateSimpleStatus(currentTreeGroup, IsReviewMode),
                IsRoster = currentTreeGroupAsRoster != null
            };

            this.interviewEntityFactory.ApplyValidity(info.Validity, currentTreeGroup, IsReviewMode);

            return info;
        }

        public InterviewEntity[] GetEntitiesDetails(string[] ids)
        {
            var callerInterview = this.GetCallerInterview();
            if (callerInterview == null) return null;

            var questionnaire = this.GetCallerQuestionnaire();
            return ids.Select(id => 
                id == @"NavigationButton"
                    ? this.GetNavigationButtonState(id, questionnaire)
                    : this.interviewEntityFactory.GetEntityDetails(id, callerInterview, questionnaire, IsReviewMode))
                .ToArray();
        }

        private static readonly Regex HtmlRemovalRegex = new Regex(Constants.HtmlRemovalPattern, RegexOptions.Compiled);

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public bool HasCoverPage()
        {
            var interview = this.GetCallerInterview();
            if (interview == null) return false;

            return this.GetCallerQuestionnaire().GetPrefilledQuestions().Any()
                || interview.GetAllCommentedEnabledQuestions().Any()
                || !string.IsNullOrWhiteSpace(interview.SupervisorRejectComment);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.sidebar.js")]
        public Sidebar GetSidebarChildSectionsOf(string[] parentIds)
        {
            var sectionId = CallerSectionid;
            var interview = this.GetCallerInterview();
            if (interview == null) return null;

            return this.interviewEntityFactory.GetSidebarChildSectionsOf(sectionId, interview, parentIds, IsReviewMode);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @Combobox.vue")]
        public virtual DropdownItem[] GetTopFilteredOptionsForQuestion(string id, string filter, int count)
        {
            var questionIdentity = Identity.Parse(id);
            var statefulInterview = this.GetCallerInterview();
            if (statefulInterview == null) return null;

            var question = statefulInterview.GetQuestion(questionIdentity);
            var parentCascadingQuestion = question.GetAsInterviewTreeCascadingQuestion()?.GetCascadingParentQuestion();
            var parentCascadingQuestionAnswer = parentCascadingQuestion?.IsAnswered() ?? false
                ? parentCascadingQuestion.GetAnswer()?.SelectedValue
                : null;

            var topFilteredOptionsForQuestion = statefulInterview.GetTopFilteredOptionsForQuestion(questionIdentity, parentCascadingQuestionAnswer, filter, count);
            return topFilteredOptionsForQuestion.Select(x => new DropdownItem(x.Value, x.Title)).ToArray();
        }

        public CompleteInfo GetCompleteInfo()
        {
            var interview = this.GetCallerInterview();
            if (interview == null) return null;

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

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public CoverInfo GetCoverInfo()
        {
            var interview = this.GetCallerInterview();
            if (interview == null) return null;

            var allCommented = IsReviewMode ? interview.GetAllCommentedEnabledQuestions().ToList() : 
                                              interview.GetCommentedBySupervisorQuestionsVisibledToInterviewer().ToList();

            var commentedQuestionsCount = allCommented.Count;
            var commentedQuestions = allCommented.Take(30).ToArray();

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

            var questionnaire = this.GetCallerQuestionnaire();

            var interviewEntityWithTypes = questionnaire
                .GetPrefilledQuestions()
                .Select(x => this.GetIdentifyingQuestion(x, interview, questionnaire))
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
        
        private InterviewEntityType GetEntityType(Guid entityId, IQuestionnaire callerQuestionnaire)
        {
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
                case QuestionType.Area:
                    return IsReviewMode ? InterviewEntityType.Area : InterviewEntityType.Unsupported;
                default:
                    return InterviewEntityType.Unsupported;
            }
        }
    }
}
