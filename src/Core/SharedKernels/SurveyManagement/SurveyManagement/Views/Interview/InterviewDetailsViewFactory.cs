using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewDetailsViewFactory : IInterviewDetailsViewFactory
    {
        private readonly IReadSideKeyValueStorage<InterviewData> interviewStore;

        private readonly IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore;
        private readonly IReadSideRepositoryReader<UserDocument> userStore;
        private readonly IInterviewDataAndQuestionnaireMerger merger;
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IAttachmentContentService attachmentContentService;

        public InterviewDetailsViewFactory(IReadSideKeyValueStorage<InterviewData> interviewStore,
            IReadSideRepositoryReader<UserDocument> userStore,
            IInterviewDataAndQuestionnaireMerger merger,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue,
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore,
            IAttachmentContentService attachmentContentService)
        {
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.merger = merger;
            this.changeStatusFactory = changeStatusFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.interviewLinkedQuestionOptionsStore = interviewLinkedQuestionOptionsStore;
            this.attachmentContentService = attachmentContentService;
        }

        public DetailsViewModel GetInterviewDetails(Guid interviewId, Guid? currentGroupId, decimal[] currentGroupRosterVector, InterviewDetailsFilter? filter)
        {
            var interview = interviewStore.GetById(interviewId);

            if (interview == null || interview.IsDeleted)
                return null;

            var user = this.userStore.GetById(interview.ResponsibleId);
            if (user == null)
                throw new ArgumentException($"User with id {interview.ResponsibleId} is not found.");

            var questionnaire = this.plainQuestionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireId, interview.QuestionnaireVersion);

            if (questionnaire == null)
                throw new ArgumentException(
                    $"Questionnaire with id {interview.QuestionnaireId} and version {interview.QuestionnaireVersion} is missing.");
            
            var attachmentIdAndTypes = attachmentContentService.GetAttachmentInfosByContentIds(questionnaire.Attachments.Select(x => x.ContentId).ToList());

            var interviewDetailsView = merger.Merge(interview, questionnaire, user.GetUseLight(), this.interviewLinkedQuestionOptionsStore.GetById(interviewId), attachmentIdAndTypes);

            var questionViews = interviewDetailsView.Groups.SelectMany(group => group.Entities).OfType<InterviewQuestionView>().ToList();
            var detailsStatisticView = new DetailsStatisticView()
            {
                AnsweredCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Answered, question)),
                UnansweredCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Unanswered, question)),
                CommentedCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Commented, question)),
                EnabledCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Enabled, question)),
                FlaggedCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Flagged, question)),
                InvalidCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Invalid, question)),
                SupervisorsCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Supervisors, question)),
                HiddenCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Hidden, question)),
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
                        .Where(question => (question is InterviewQuestionView && IsQuestionInFilter(filter, (InterviewQuestionView) question)) || question is InterviewStaticTextView)
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
                HasUnprocessedSyncPackages = this.incomingSyncPackagesQueue.HasPackagesByInterviewId(interviewId)
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

        private bool IsQuestionInFilter(InterviewDetailsFilter? filter, InterviewQuestionView question)
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
            return true;
        }
    }
}