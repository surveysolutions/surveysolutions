﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.UI.Headquarters.Models.WebInterview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using GpsAnswer = WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers.GpsAnswer;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.API.WebInterview
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

        public LanguageInfo GetLanguageInfo() => new LanguageInfo
        {
            OriginalLanguageName = Headquarters.Resources.WebInterview.Original_Language,
            Languages = this.GetCallerQuestionnaire().GetTranslationLanguages(),
            CurrentLanguage = this.GetCallerInterview().Language
        };

        public InterviewInfo GetInterviewDetails()
        {
            var statefulInterview = this.GetCallerInterview();
            var questionnaire = this.GetCallerQuestionnaire();

            return new InterviewInfo
            {
                QuestionnaireTitle = IsReviewMode 
                    ? string.Format(Pages.QuestionnaireNameFormat, questionnaire.Title, questionnaire.Version) 
                    : questionnaire.Title,
                FirstSectionId = questionnaire.GetFirstSectionId().FormatGuid(),
                InterviewKey = statefulInterview.GetInterviewKey().ToString(),
                InterviewCannotBeChanged = statefulInterview.ReceivedByInterviewer || this.authorizedUser.IsObserving,
                ReceivedByInterviewer = statefulInterview.ReceivedByInterviewer
            };
        }

        public List<CommentedStatusHistroyView> GetStatusesHistory()
        {
            var statefulInterview = this.GetCallerInterview();
            return this.changeStatusFactory.GetFilteredStatuses(statefulInterview.Id);
        }

        public void SetFlag(string questionId, bool hasFlag)
        {
            if (this.authorizedUser.IsObserver)
                throw new InterviewAccessException(InterviewAccessExceptionReason.Forbidden, Strings.ObserverNotAllowed);

            var statefulInterview = this.GetCallerInterview();
            this.interviewFactory.SetFlagToQuestion(statefulInterview.Id, Identity.Parse(questionId), hasFlag);
        }

        public IEnumerable<string> GetFlags()
        {
            var statefulInterview = this.GetCallerInterview();
            return this.interviewFactory.GetFlaggedQuestionIds(statefulInterview.Id).Select(x => x.ToString());
        }

        public bool IsEnabled(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var statefulInterview = this.GetCallerInterview();
            return statefulInterview.IsEnabled(Identity.Parse(id));
        }

        public string GetInterviewStatus()
        {
            return this.interviewEntityFactory.GetInterviewSimpleStatus(this.GetCallerInterview(), IsReviewMode).ToString();
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
            if (entityType == InterviewEntityType.DateTime && (interviewQuestion.GetAsInterviewTreeDateTimeQuestion()).IsTimestamp)
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

            var ids = IsReviewMode ? statefulInterview.GetUnderlyingEntitiesForReview(sectionIdentity) : 
                                     statefulInterview.GetUnderlyingInterviewerEntities(sectionIdentity);
            var questionarie = this.GetCallerQuestionnaire();

            var entities = ids
                .Where(id => !questionarie.IsVariable(id.Id))
                .Select(x => new InterviewEntityWithType
                {
                    Identity = x.ToString(),
                    EntityType = this.GetEntityType(x.Id, questionarie).ToString()
                })
                .Union(ActionButtonsDefinition)
                .ToArray();

            return entities;
        }

        public ButtonState GetNavigationButtonState(string id, IQuestionnaire questionnaire = null)
        {
            ButtonState NewButtonState(ButtonState button, InterviewTreeGroup target)
            {
                button.Id = id;
                button.Target = target.Identity.ToString();
                button.Status = this.interviewEntityFactory.CalculateSimpleStatus(target, IsReviewMode);

                this.interviewEntityFactory.ApplyValidity(button.Validity, target, IsReviewMode);

                return button;
            }

            var sectionId = CallerSectionid;
            var callerQuestionnaire = questionnaire ?? this.GetCallerQuestionnaire();

            var statefulInterview = this.GetCallerInterview();

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
                    Title = Headquarters.Resources.WebInterview.CompleteInterview,
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

        public BreadcrumbInfo GetBreadcrumbs(string sectionIdArg)
        {
            var sectionId = CallerSectionid ?? sectionIdArg;

            if (sectionId == null) return new BreadcrumbInfo();

            Identity groupId = Identity.Parse(sectionId);

            var statefulInterview = this.GetCallerInterview();
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
            var questionnaire = this.GetCallerQuestionnaire();
            return ids.Select(id => 
                id == @"NavigationButton"
                    ? this.GetNavigationButtonState(id, questionnaire)
                    : this.interviewEntityFactory.GetEntityDetails(id, callerInterview, questionnaire, IsReviewMode))
                .ToArray();
        }

        private static readonly Regex HtmlRemovalRegex = new Regex(Constants.HtmlRemovalPattern, RegexOptions.Compiled);
        
        public bool HasCoverPage()
        {
            var interview = this.GetCallerInterview();

            return this.GetCallerQuestionnaire().GetPrefilledQuestions().Any()
                || interview.GetAllCommentedEnabledQuestions().Any()
                || !string.IsNullOrWhiteSpace(interview.SupervisorRejectComment);
        }

        public Sidebar GetSidebarChildSectionsOf(string[] parentIds)
        {
            var sectionId = CallerSectionid;
            var interview = this.GetCallerInterview();
            return this.interviewEntityFactory.GetSidebarChildSectionsOf(sectionId, interview, parentIds, IsReviewMode);
        }

        public DropdownItem[] GetTopFilteredOptionsForQuestion(string id, string filter, int count)
        {
            var questionIdentity = Identity.Parse(id);
            var statefulInterview = this.GetCallerInterview();
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
        
        public CoverInfo GetCoverInfo()
        {
            var interview = this.GetCallerInterview();

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