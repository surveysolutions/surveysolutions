using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewDetailsViewFactory : IInterviewDetailsViewFactory
    {
        private readonly IReadSideKeyValueStorage<InterviewData> interviewStore;
        private readonly IReadSideRepositoryReader<UserDocument> userStore;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructures;
        private readonly IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions;
        private readonly IInterviewDataAndQuestionnaireMerger merger;
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;

        public InterviewDetailsViewFactory(IReadSideKeyValueStorage<InterviewData> interviewStore,
            IReadSideRepositoryReader<UserDocument> userStore,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore,
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructures,
            IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions,
            IInterviewDataAndQuestionnaireMerger merger,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue)
        {
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.questionnaireStore = questionnaireStore;
            this.questionnaireRosterStructures = questionnaireRosterStructures;
            this.questionnaireReferenceInfoForLinkedQuestions = questionnaireReferenceInfoForLinkedQuestions;
            this.merger = merger;
            this.changeStatusFactory = changeStatusFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
        }

        public DetailsViewModel GetInterviewDetails(Guid interviewId, Guid? currentGroupId, decimal[] currentGroupRosterVector, InterviewDetailsFilter? filter)
        {
            var interview = interviewStore.GetById(interviewId);

            if (interview == null || interview.IsDeleted)
                return null;

            QuestionnaireDocumentVersioned questionnaire = this.questionnaireStore.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);

            if (questionnaire == null)
                throw new ArgumentException(string.Format("Questionnaire with id {0} and version {1} is missing.", interview.QuestionnaireId, interview.QuestionnaireVersion));

            var questionnaireReferenceInfo = this.questionnaireReferenceInfoForLinkedQuestions.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);

            var questionnaireRosters = this.questionnaireRosterStructures.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);

            var user = this.userStore.GetById(interview.ResponsibleId);
            if (user == null)
                throw new ArgumentException(string.Format("User with id {0} is not found.", interview.ResponsibleId));

            var interviewDetailsView = merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);

            var questionViews = interviewDetailsView.Groups.SelectMany(group => group.Entities).OfType<InterviewQuestionView>();
            var detailsStatisticView = new DetailsStatisticView()
            {
                AnsweredCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Answered, question)),
                UnansweredCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Unanswered, question)),
                CommentedCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Commented, question)),
                EnabledCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Enabled, question)),
                FlaggedCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Flagged, question)),
                InvalidCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Invalid, question)),
                SupervisorsCount = questionViews.Count(question => this.IsQuestionInFilter(InterviewDetailsFilter.Supervisors, question))
            };

            var selectedGroups = new List<InterviewGroupView>();

            var currentGroup = interviewDetailsView.Groups.Find(group => currentGroupId != null && group.Id == currentGroupId &&
                                                                group.RosterVector.SequenceEqual(currentGroupRosterVector));

            foreach (var interviewGroupView in interviewDetailsView.Groups)
            {
                if (currentGroup != null && currentGroup.ParentId.HasValue)
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
                    interviewGroupView.Entities = interviewGroupView.Entities.OfType<InterviewQuestionView>()
                        .Where(question => IsQuestionInFilter(filter, question))
                        .Select(question => (InterviewEntityView) question)
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
                QuestionnaireDocumentVersioned questionnaire = this.questionnaireStore.AsVersioned()
                    .Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);

                return questionnaire.Questionnaire.Children[0].PublicKey;
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
            }
            return true;
        }
    }
}