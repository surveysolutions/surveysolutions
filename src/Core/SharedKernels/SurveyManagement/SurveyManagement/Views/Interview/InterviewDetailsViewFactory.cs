using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewDetailsViewFactory : IInterviewDetailsViewFactory
    {
        private readonly IReadSideKeyValueStorage<InterviewData> interviewStore;

        private readonly IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore;
        private readonly IReadSideKeyValueStorage<InterviewVariables> interviewVariablesStore;
        private readonly IPlainStorageAccessor<UserDocument> userStore;
        private readonly IInterviewDataAndQuestionnaireMerger merger;
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IAttachmentContentService attachmentContentService;

        public InterviewDetailsViewFactory(IReadSideKeyValueStorage<InterviewData> interviewStore,
            IPlainStorageAccessor<UserDocument> userStore,
            IInterviewDataAndQuestionnaireMerger merger,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
            IInterviewPackagesService incomingSyncPackagesQueue,
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore,
            IAttachmentContentService attachmentContentService, IReadSideKeyValueStorage<InterviewVariables> interviewVariablesStore)
        {
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.merger = merger;
            this.changeStatusFactory = changeStatusFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.interviewLinkedQuestionOptionsStore = interviewLinkedQuestionOptionsStore;
            this.attachmentContentService = attachmentContentService;
            this.interviewVariablesStore = interviewVariablesStore;
        }

        public DetailsViewModel GetInterviewDetails(Guid interviewId, Guid? currentGroupId, decimal[] currentGroupRosterVector, InterviewDetailsFilter? filter)
        {
            var interview = interviewStore.GetById(interviewId);

            if (interview == null || interview.IsDeleted)
                return null;

            var user = this.userStore.GetById(interview.ResponsibleId.FormatGuid());
            if (user == null)
                throw new ArgumentException($"User with id {interview.ResponsibleId} is not found.");

            var questionnaire = this.plainQuestionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireId, interview.QuestionnaireVersion);

            if (questionnaire == null)
                throw new ArgumentException(
                    $"Questionnaire with id {interview.QuestionnaireId} and version {interview.QuestionnaireVersion} is missing.");
            
            var attachmentIdAndTypes = attachmentContentService.GetAttachmentInfosByContentIds(questionnaire.Attachments.Select(x => x.ContentId).ToList());

            var interviewDetailsView = merger.Merge(interview, questionnaire, user.GetUseLight(), this.interviewLinkedQuestionOptionsStore.GetById(interviewId), attachmentIdAndTypes,
                this.interviewVariablesStore.GetById(interviewId));

            var interviewEntityViews = interviewDetailsView.Groups
                .SelectMany(group => group.Entities)
                .Where(entity => entity is InterviewQuestionView || entity is InterviewStaticTextView)
                .ToList();
            var questionViews = interviewEntityViews.OfType<InterviewQuestionView>().ToList();

            var detailsStatisticView = new DetailsStatisticView()
            {
                AnsweredCount = questionViews.Count(interviewEntityView => this.IsEntityInFilter(InterviewDetailsFilter.Answered, interviewEntityView)),
                UnansweredCount = questionViews.Count(interviewEntityView => this.IsEntityInFilter(InterviewDetailsFilter.Unanswered, interviewEntityView)),
                CommentedCount = questionViews.Count(interviewEntityView => this.IsEntityInFilter(InterviewDetailsFilter.Commented, interviewEntityView)),
                EnabledCount = interviewEntityViews.Count(interviewEntityView => this.IsEntityInFilter(InterviewDetailsFilter.Enabled, interviewEntityView)),
                FlaggedCount = questionViews.Count(interviewEntityView => this.IsEntityInFilter(InterviewDetailsFilter.Flagged, interviewEntityView)),
                InvalidCount = interviewEntityViews.Count(interviewEntityView => this.IsEntityInFilter(InterviewDetailsFilter.Invalid, interviewEntityView)),
                SupervisorsCount = questionViews.Count(interviewEntityView => this.IsEntityInFilter(InterviewDetailsFilter.Supervisors, interviewEntityView)),
                HiddenCount = questionViews.Count(interviewEntityView => this.IsEntityInFilter(InterviewDetailsFilter.Hidden, interviewEntityView)),
            };

            var selectedGroups = new List<InterviewGroupView>();

            var currentGroup = interviewDetailsView.Groups.Find(group => currentGroupId != null && group.Id == currentGroupId &&
                                                                group.RosterVector.SequenceEqual(currentGroupRosterVector));

            foreach (var interviewGroupView in interviewDetailsView.Groups)
            {
                if (currentGroup?.ParentId != null)
                {
                    if (interviewGroupView.Id == currentGroup.Id &&
                        interviewGroupView.RosterVector.SequenceEqual(currentGroup.RosterVector) ||
                        selectedGroups.Any(_ => _.Id == interviewGroupView.ParentId))
                    {
                        selectedGroups.Add(interviewGroupView);
                    }
                }
                else
                {
                    interviewGroupView.Entities = interviewGroupView.Entities
                        .Where(question =>
                        {
                            return this.IsEntityInFilter(filter, question);
                        })
                        .ToList();

                    if (interviewGroupView.Entities.Any())
                        selectedGroups.Add(interviewGroupView);
                }
            }

            return new DetailsViewModel()
            {
                Filter = filter.Value,
                SelectedGroupId = currentGroupId,
                SelectedGroupRosterVector = currentGroupRosterVector,
                InterviewDetails = interviewDetailsView,
                FilteredGroups = selectedGroups,
                Statistic = detailsStatisticView,
                History = this.changeStatusFactory.Load(new ChangeStatusInputModel { InterviewId = interviewId }),
                HasUnprocessedSyncPackages = this.incomingSyncPackagesQueue.HasPendingPackageByInterview(interviewId)
            };
        }

        public Guid? GetFirstChapterId(Guid interviewId)
        {
            var interview = interviewStore.GetById(interviewId);

            if (interview != null && !interview.IsDeleted)
            {
                var questionnaire = this.plainQuestionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireId, interview.QuestionnaireVersion);

                return questionnaire.Children[0].PublicKey;
            }

            return null;
        }

        private bool IsEntityInFilter(InterviewDetailsFilter? filter, InterviewEntityView entity)
        {
            var question = entity as InterviewQuestionView;

            if (question != null)
            {
                switch (filter)
                {
                    case InterviewDetailsFilter.Answered:
                        return question.IsAnswered;
                    case InterviewDetailsFilter.Unanswered:
                        return question.IsEnabled && !question.IsAnswered;
                    case InterviewDetailsFilter.Commented:
                        return question.Comments != null && question.Comments.Any();
                    case InterviewDetailsFilter.Enabled:
                        return question.IsEnabled;
                    case InterviewDetailsFilter.Flagged:
                        return question.IsFlagged;
                    case InterviewDetailsFilter.Invalid:
                        return !question.IsValid;
                    case InterviewDetailsFilter.Supervisors:
                        return question.Scope == QuestionScope.Supervisor;
                    case InterviewDetailsFilter.Hidden:
                        return question.Scope == QuestionScope.Hidden;
                }
            }

            var staticText = entity as InterviewStaticTextView;
            if (staticText != null)
            {
                switch (filter)
                {
                    case InterviewDetailsFilter.Enabled:
                        return staticText.IsEnabled;
                    case InterviewDetailsFilter.Invalid:
                        return !staticText.IsValid;
                    case InterviewDetailsFilter.Flagged:
                        return false;
                    case InterviewDetailsFilter.All:
                        return true;
                    default:
                        return false;
                }
            }
            return true;
        }
    }
}