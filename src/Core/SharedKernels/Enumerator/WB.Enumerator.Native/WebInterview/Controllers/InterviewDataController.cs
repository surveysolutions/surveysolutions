﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview.Models;
using GpsAnswer = WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers.GpsAnswer;

namespace WB.Enumerator.Native.WebInterview.Controllers
{
    public abstract class InterviewDataController : ControllerBase
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IWebInterviewInterviewEntityFactory interviewEntityFactory;

        public InterviewDataController(IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewNotificationService webInterviewNotificationService,
            IWebInterviewInterviewEntityFactory interviewEntityFactory)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.interviewEntityFactory = interviewEntityFactory;
        }

        protected virtual bool IsReviewMode() => false;

        protected virtual bool IsCurrentUserObserving() => false;

        private static InterviewEntityWithType[] ActionButtonsDefinition { get; } = {
            new InterviewEntityWithType
            {
                Identity = @"NavigationButton",
                EntityType = InterviewEntityType.NavigationButton.ToString()
            }
        };

        protected IQuestionnaire GetCallerQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string translation = null)
        {
            return questionnaireRepository.GetQuestionnaire(questionnaireIdentity, translation);
        }

        protected IStatefulInterview GetCallerInterview(Guid interviewId)
        {
            return statefulInterviewRepository.Get(interviewId.FormatGuid());
        }

        public virtual LanguageInfo GetLanguageInfo(Guid interviewId)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            if (statefulInterview == null) return null;

            return new LanguageInfo
            {
                OriginalLanguageName = Resources.WebInterview.Original_Language,
                Languages = this.GetCallerQuestionnaire(statefulInterview.QuestionnaireIdentity).GetTranslationLanguages(),
                CurrentLanguage = statefulInterview.Language
            };
        }

        public virtual InterviewInfo GetInterviewDetails(Guid interviewId)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            if (statefulInterview == null) return null;

            var questionnaire = this.GetCallerQuestionnaire(statefulInterview.QuestionnaireIdentity);

            return new InterviewInfo
            {
                QuestionnaireTitle = IsReviewMode()
                    ? string.Format(Resources.WebInterview.QuestionnaireNameFormat, questionnaire.Title, questionnaire.Version)
                    : questionnaire.Title,
                FirstSectionId = questionnaire.GetFirstSectionId().FormatGuid(),
                QuestionnaireVersion = questionnaire.Version,
                InterviewKey = statefulInterview.GetInterviewKey().ToString(),
                InterviewCannotBeChanged = 
                    statefulInterview.ReceivedByInterviewer 
                    || statefulInterview.Status == InterviewStatus.ApprovedByHeadquarters
                    || this.IsCurrentUserObserving(),
                CanAddComments = statefulInterview.Status != InterviewStatus.ApprovedByHeadquarters && !this.IsCurrentUserObserving(),
                ReceivedByInterviewer = statefulInterview.ReceivedByInterviewer,
                IsCurrentUserObserving = this.IsCurrentUserObserving(),
                DoesBrokenPackageExist = false
            };
        }

        public virtual bool IsEnabled(Guid interviewId, string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var statefulInterview = this.GetCallerInterview(interviewId);
            return statefulInterview.IsEnabled(Identity.Parse(id));
        }

        public virtual GroupStatus GetInterviewStatus(Guid interviewId)
        {
            var interview = this.GetCallerInterview(interviewId);
            if (interview == null) return GroupStatus.StartedInvalid;

            return this.interviewEntityFactory.GetInterviewSimpleStatus(interview, IsReviewMode());
        }

        private IdentifyingQuestion GetIdentifyingQuestion(Guid questionId, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            var result = new IdentifyingQuestion();
            var entityType = this.interviewEntityFactory.GetEntityType(new Identity(questionId, RosterVector.Empty), questionnaire, interview, IsReviewMode());

            result.Type = entityType.ToString();
            var questionIdentity = new Identity(questionId, RosterVector.Empty);
            result.Identity = questionIdentity.ToString();
            var interviewQuestion = interview.GetQuestion(questionIdentity);

            result.IsReadonly = interviewQuestion.IsReadonly;
            result.Title = interviewQuestion.Title.BrowserReadyText;
            //result.Instructions = interviewQuestion.Instructions.BrowserReadyText;

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

        public virtual InterviewEntityWithType[] GetPrefilledQuestions(Guid interviewId)
        {
            var interview = this.GetCallerInterview(interviewId);
            var questionnaire = this.GetCallerQuestionnaire(interview.QuestionnaireIdentity);
            InterviewEntityWithType[] interviewEntityWithTypes = questionnaire
                .GetPrefilledQuestions()
                .Select(x => new InterviewEntityWithType
                {
                    Identity = Identity.Create(x, RosterVector.Empty).ToString(),
                    EntityType = this.interviewEntityFactory.GetEntityType(new Identity(x, RosterVector.Empty), questionnaire, interview, IsReviewMode()).ToString()
                })
                .ToArray();

            return interviewEntityWithTypes;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual PrefilledPageData GetPrefilledEntities(Guid interviewId)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            if (statefulInterview == null) return null;
            var questionnaire = this.GetCallerQuestionnaire(statefulInterview.QuestionnaireIdentity);

            InterviewEntityWithType[] interviewEntityWithTypes = GetPrefilledQuestions(interviewId)
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

        public virtual InterviewEntityWithType[] GetSectionEntities(Guid interviewId, string sectionId)
        {
            if (sectionId == null) throw new ArgumentNullException(nameof(sectionId));

            var statefulInterview = GetCallerInterview(interviewId);
            if (statefulInterview == null) return null;

            var questionnaire = this.GetCallerQuestionnaire(statefulInterview.QuestionnaireIdentity);

            var sectionIdentity = Identity.Parse(sectionId);

            var ids = GetGroupEntitiesIds(sectionIdentity);

            IEnumerable<Identity> GetGroupEntitiesIds(Identity identity)
            {
                return IsReviewMode()
                    ? statefulInterview.GetUnderlyingEntitiesForReview(identity)
                    : statefulInterview.GetUnderlyingInterviewerEntities(identity);
            }

            List<Identity> groupIds = new List<Identity>();

            foreach (var elementId in ids)
            {
                if (questionnaire.IsTableRoster(elementId.Id) || questionnaire.IsMatrixRoster(elementId.Id))
                {
                    var tableRosterIdentity = new Identity(elementId.Id, sectionIdentity.RosterVector);
                    if (!groupIds.Contains(tableRosterIdentity))
                        groupIds.Add(tableRosterIdentity);
                    continue;
                }

                groupIds.Add(elementId);

                if (questionnaire.IsFlatRoster(elementId.Id))
                {
                    var groupEntitiesIds = GetGroupEntitiesIds(elementId);
                    groupIds.AddRange(groupEntitiesIds);
                }
            }

            var entities = groupIds
                .Select(x => new InterviewEntityWithType
                {
                    Identity = x.ToString(),
                    EntityType = this.interviewEntityFactory.GetEntityType(x, questionnaire, statefulInterview, IsReviewMode()).ToString()
                })
                .Union(ActionButtonsDefinition)
                .ToArray();

            return entities;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual SectionData GetFullSectionInfo(Guid interviewId, string sectionId)
        {
            var entities = GetSectionEntities(interviewId, sectionId);

            var details = GetEntitiesDetails(interviewId, entities.Select(e => e.Identity).ToArray(), sectionId);

            return new SectionData
            {
                Entities = entities,
                Details = details
            };
        }

        public virtual ButtonState GetNavigationButtonState(Guid interviewId, string sectionId, string id, IQuestionnaire questionnaire = null)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            if (statefulInterview == null) return null;

            var callerQuestionnaire = questionnaire ?? this.GetCallerQuestionnaire(statefulInterview.QuestionnaireIdentity);
            ButtonState NewButtonState(ButtonState button, InterviewTreeGroup target)
            {
                button.Id = id;
                button.Target = target.Identity.ToString();
                button.Status = button.Type == ButtonType.Complete
                    ? this.interviewEntityFactory.GetInterviewSimpleStatus(statefulInterview, IsReviewMode())
                    : this.interviewEntityFactory.CalculateSimpleStatus(target, IsReviewMode(), statefulInterview, questionnaire);

                this.interviewEntityFactory.ApplyValidity(button.Validity, button.Status);

                return button;
            }

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

            var parent = interviewEntityFactory.GetUIParent(statefulInterview, questionnaire, sectionIdentity);
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
        public virtual BreadcrumbInfo GetBreadcrumbs(Guid interviewId, string sectionId = null)
        {
            if (sectionId == null) return new BreadcrumbInfo();

            Identity groupId = Identity.Parse(sectionId);

            var statefulInterview = this.GetCallerInterview(interviewId);
            if (statefulInterview == null) return null;
            var questionnaire = this.GetCallerQuestionnaire(statefulInterview.QuestionnaireIdentity);
            if (questionnaire == null) return null;

            ReadOnlyCollection<Guid> parentIds = questionnaire.GetParentsStartingFromTop(groupId.Id)
                .Except(id => questionnaire.IsCustomViewRoster(id))
                .ToReadOnlyCollection();

            var breadCrumbs = new List<Breadcrumb>();
            int metRosters = 0;

            foreach (Guid parentId in parentIds)
            {
                if (questionnaire.IsRosterGroup(parentId))
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
                this.webInterviewNotificationService.ReloadInterview(interviewId);
                return new BreadcrumbInfo();
            }

            var info = new BreadcrumbInfo
            {
                Title = currentTreeGroup.Title.Text,
                RosterTitle = (currentTreeGroupAsRoster)?.RosterTitle,
                Breadcrumbs = breadCrumbs.ToArray(),
                Status = this.interviewEntityFactory.CalculateSimpleStatus(currentTreeGroup, IsReviewMode(), statefulInterview, questionnaire),
                IsRoster = currentTreeGroupAsRoster != null,
                HasCustomRosterTitle = questionnaire.HasCustomRosterTitle(currentTreeGroup.Identity.Id)
            };

            this.interviewEntityFactory.ApplyValidity(info.Validity, info.Status);

            return info;
        }

        public virtual InterviewEntity[] GetEntitiesDetails(Guid interviewId, string[] ids, string sectionId = null)
        {
            var callerInterview = this.GetCallerInterview(interviewId);
            if (callerInterview == null) return null;

            var questionnaire = this.GetCallerQuestionnaire(callerInterview.QuestionnaireIdentity, callerInterview.Language);
            var interviewEntities = new List<InterviewEntity>();

            foreach (var id in ids)
            {
                if (id == @"NavigationButton")
                {
                    interviewEntities.Add(this.GetNavigationButtonState(interviewId, sectionId, id, questionnaire));
                    continue;
                }

                var interviewEntity = this.interviewEntityFactory.GetEntityDetails(id, callerInterview, questionnaire, IsReviewMode());
                interviewEntities.Add(interviewEntity);

                if (interviewEntity is RosterEntity tableRoster)
                {
                    foreach (var tableRosterInstance in tableRoster.Instances)
                    {
                        var childQuestions = callerInterview.GetChildQuestions(Identity.Parse(tableRosterInstance.Id));
                        foreach (var childQuestion in childQuestions)
                        {
                            var question = this.interviewEntityFactory.GetEntityDetails(childQuestion.ToString(), callerInterview, questionnaire, IsReviewMode());
                            interviewEntities.Add(question);
                        }
                    }
                }
            }
            return interviewEntities.ToArray();
        }

        private static readonly Regex HtmlRemovalRegex = new Regex(Constants.HtmlRemovalPattern, RegexOptions.Compiled);

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual bool HasCoverPage(Guid interviewId)
        {
            var interview = this.GetCallerInterview(interviewId);
            if (interview == null) return false;

            return this.GetCallerQuestionnaire(interview.QuestionnaireIdentity).GetPrefilledQuestions().Any()
                || interview.GetAllCommentedEnabledQuestions().Any()
                || !string.IsNullOrWhiteSpace(interview.SupervisorRejectComment);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.sidebar.js")]
        public virtual Sidebar GetSidebarChildSectionsOf(Guid interviewId, string[] ids, string sectionId = null)
        {
            var interview = this.GetCallerInterview(interviewId);
            if (interview == null) return null;
            var questionnaire = this.GetCallerQuestionnaire(interview.QuestionnaireIdentity);
            if (questionnaire == null) return null;

            return this.interviewEntityFactory.GetSidebarChildSectionsOf(sectionId, interview, questionnaire, ids, IsReviewMode());
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @Combobox.vue")]
        public virtual DropdownItem[] GetTopFilteredOptionsForQuestion(Guid interviewId, string id, string filter, int count)
            => this.GetTopFilteredOptionsForQuestion(interviewId, id, filter, count, null);


        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @MultiCombobox.vue")]
        public virtual DropdownItem[] GetTopFilteredOptionsForQuestion(Guid interviewId, string id, string filter, int count, int[] excludedOptionIds = null)
        {
            var questionIdentity = Identity.Parse(id);
            var statefulInterview = this.GetCallerInterview(interviewId);
            if (statefulInterview == null) return null;

            var question = statefulInterview.GetQuestion(questionIdentity);
            var parentCascadingQuestion = question.GetAsInterviewTreeCascadingQuestion()?.GetCascadingParentQuestion();
            var parentCascadingQuestionAnswer = parentCascadingQuestion?.IsAnswered() ?? false
                ? parentCascadingQuestion.GetAnswer()?.SelectedValue
                : null;

            var topFilteredOptionsForQuestion = statefulInterview.GetTopFilteredOptionsForQuestion(questionIdentity, parentCascadingQuestionAnswer, filter, count, excludedOptionIds);
            return topFilteredOptionsForQuestion.Select(x => new DropdownItem(x.Value, x.Title)).ToArray();
        }

        public virtual CompleteInfo GetCompleteInfo(Guid interviewId)
        {
            var interview = this.GetCallerInterview(interviewId);
            if (interview == null) return null;
            var questionnaire = this.GetCallerQuestionnaire(interview.QuestionnaireIdentity);
            if (questionnaire == null) return null;

            var questionsCount = interview.CountActiveQuestionsInInterview();
            var answeredQuestionsCount = interview.CountActiveAnsweredQuestionsInInterview();
            var invalidAnswersCount = interview.CountInvalidEntitiesInInterview();
            Identity[] invalidEntityIds = interview.GetInvalidEntitiesInInterview().Take(30).ToArray();
            var invalidEntities = invalidEntityIds.Select(identity =>
            {
                var titleText = HtmlRemovalRegex.Replace(interview.GetTitleText(identity), string.Empty);
                var isPrefilled = interview.IsQuestionPrefilled(identity);
                var parentId = interviewEntityFactory.GetUIParent(interview, questionnaire, identity);
                return new EntityWithError
                {
                    Id = identity.ToString(),
                    ParentId = parentId.ToString(),
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
        public virtual CoverInfo GetCoverInfo(Guid interviewId)
        {
            var interview = this.GetCallerInterview(interviewId);
            if (interview == null) return null;
            var questionnaire = this.GetCallerQuestionnaire(interview.QuestionnaireIdentity);
            if (questionnaire == null) return null;

            var allCommented = IsReviewMode() ? interview.GetAllCommentedEnabledQuestions().ToList() :
                                              interview.GetCommentedBySupervisorQuestionsVisibleToInterviewer().ToList();

            var commentedQuestionsCount = allCommented.Count;
            var commentedQuestions = allCommented.Take(30).ToArray();

            var entitiesWithComments = commentedQuestions.Select(identity =>
            {
                var titleText = HtmlRemovalRegex.Replace(interview.GetTitleText(identity), string.Empty);
                var isPrefilled = interview.IsQuestionPrefilled(identity);
                var parentId = interviewEntityFactory.GetUIParent(interview, questionnaire, identity);
                return new EntityWithComment
                {
                    Id = identity.ToString(),
                    ParentId = parentId.ToString(),
                    Title = titleText,
                    IsPrefilled = isPrefilled
                };

            }).ToArray();

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
    }
}
