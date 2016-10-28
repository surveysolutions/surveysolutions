using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    using Interview = SharedKernels.DataCollection.Implementation.Aggregates.Interview;

    public class InterviewDetailsViewFactory : IInterviewDetailsViewFactory
    {
        private readonly IReadSideKeyValueStorage<InterviewData> interviewStore;

        private readonly IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore;
        private readonly IPlainStorageAccessor<UserDocument> userStore;
        private readonly IInterviewDataAndQuestionnaireMerger merger;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IEventSourcedAggregateRootRepository eventSourcedRepository;
        private readonly IAttachmentContentService attachmentContentService;
        private readonly ITranslationStorage translationStorage;
        private readonly IQuestionnaireTranslator questionnaireTranslator;

        public InterviewDetailsViewFactory(IReadSideKeyValueStorage<InterviewData> interviewStore,
            IPlainStorageAccessor<UserDocument> userStore,
            IInterviewDataAndQuestionnaireMerger merger,
            IChangeStatusFactory changeStatusFactory,
            IInterviewPackagesService incomingSyncPackagesQueue,
            IQuestionnaireStorage questionnaireStorage,
            IEventSourcedAggregateRootRepository eventSourcedRepository,
            IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore,
            IAttachmentContentService attachmentContentService,
            ITranslationStorage translationStorage,
            IQuestionnaireTranslator questionnaireTranslator)
        {
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.merger = merger;
            this.changeStatusFactory = changeStatusFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.questionnaireStorage = questionnaireStorage;
            this.eventSourcedRepository = eventSourcedRepository;
            this.interviewLinkedQuestionOptionsStore = interviewLinkedQuestionOptionsStore;
            this.attachmentContentService = attachmentContentService;
            this.translationStorage = translationStorage;
            this.questionnaireTranslator = questionnaireTranslator;
        }

        public DetailsViewModel GetInterviewDetails(Guid interviewId,
            Guid? currentGroupId,
            decimal[] currentGroupRosterVector,
            InterviewDetailsFilter? filter)
        {
            var interview = this.interviewStore.GetById(interviewId);

            if (interview == null || interview.IsDeleted)
                return null;

            var user = this.userStore.GetById(interview.ResponsibleId.FormatGuid());
            if (user == null)
                throw new ArgumentException($"User with id {interview.ResponsibleId} is not found.");

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(interview.QuestionnaireId,
                interview.QuestionnaireVersion);

            if (questionnaire == null)
                throw new ArgumentException(
                    $"Questionnaire with id {interview.QuestionnaireId} and version {interview.QuestionnaireVersion} is missing.");

            var currentTranslation = questionnaire.Translations.SingleOrDefault(t => t.Name == interview.CurrentLanguage);
            if (currentTranslation != null)
            {
                var questionnaireIdentity = new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion);
                var translation = this.translationStorage.Get(questionnaireIdentity, currentTranslation.Id);

                if (translation == null)
                    throw new ArgumentException($"No translation found for language '{interview.CurrentLanguage}' and questionnaire '{questionnaireIdentity}'.");

                questionnaire = this.questionnaireTranslator.Translate(questionnaire, translation);
            }

            var attachmentIdAndTypes =
                this.attachmentContentService.GetAttachmentInfosByContentIds(
                    questionnaire.Attachments.Select(x => x.ContentId).ToList());

            InterviewDetailsView interviewDetailsView = this.merger.Merge(interview, questionnaire, user.GetUseLight(),
                this.interviewLinkedQuestionOptionsStore.GetById(interviewId), attachmentIdAndTypes);

            var interviewEntityViews = interviewDetailsView.Groups
                .SelectMany(group => group.Entities)
                .Where(entity => entity is InterviewQuestionView || entity is InterviewStaticTextView)
                .ToList();
            var questionViews = interviewEntityViews.OfType<InterviewQuestionView>().ToList();

            this.FilterCategoricalQuestionOptions(interviewId, questionViews);

            var detailsStatisticView = new DetailsStatisticView
            {
                AnsweredCount = questionViews.Count(interviewEntityView => IsEntityInFilter(InterviewDetailsFilter.Answered, interviewEntityView)),
                UnansweredCount = questionViews.Count(interviewEntityView => IsEntityInFilter(InterviewDetailsFilter.Unanswered, interviewEntityView)),
                CommentedCount = questionViews.Count(interviewEntityView => IsEntityInFilter(InterviewDetailsFilter.Commented, interviewEntityView)),
                EnabledCount = interviewEntityViews.Count(interviewEntityView => IsEntityInFilter(InterviewDetailsFilter.Enabled, interviewEntityView)),
                FlaggedCount = questionViews.Count(interviewEntityView => IsEntityInFilter(InterviewDetailsFilter.Flagged, interviewEntityView)),
                InvalidCount = interviewEntityViews.Count(interviewEntityView => IsEntityInFilter(InterviewDetailsFilter.Invalid, interviewEntityView)),
                SupervisorsCount = questionViews.Count(interviewEntityView => IsEntityInFilter(InterviewDetailsFilter.Supervisors, interviewEntityView)),
                HiddenCount = questionViews.Count(interviewEntityView => IsEntityInFilter(InterviewDetailsFilter.Hidden, interviewEntityView)),
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
                        .Where(question => { return IsEntityInFilter(filter, question); })
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
                History = this.changeStatusFactory.Load(new ChangeStatusInputModel {InterviewId = interviewId}),
                HasUnprocessedSyncPackages = this.incomingSyncPackagesQueue.HasPendingPackageByInterview(interviewId),
                Translations = questionnaire.Translations.Select(translation => 
                    new InterviewTranslationView()
                    {
                        Id = translation.Id,
                        Name = translation.Name
                    }
                ).ToReadOnlyCollection()
            };
        }

        private void FilterCategoricalQuestionOptions(Guid interviewId, List<InterviewQuestionView> questionViews)
        {
            var interviewAggregate = (Interview) this.eventSourcedRepository.GetLatest(typeof(Interview), interviewId);

            foreach (var categoricalQuestion in questionViews.Where(x => x.IsFilteredCategorical))
            {
                var questionIdentity = new Identity(categoricalQuestion.Id, categoricalQuestion.RosterVector);

                categoricalQuestion.Options = 
                    interviewAggregate.GetFirstTopFilteredOptionsForQuestion(questionIdentity, null, string.Empty, 200)
                    .Select(x => new QuestionOptionView
                    {
                        Label = x.Title,
                        Value = x.Value
                    })
                    .ToList();
            }
        }

        public Guid? GetFirstChapterId(Guid interviewId)
        {
            var interview = this.interviewStore.GetById(interviewId);

            if (interview != null && !interview.IsDeleted)
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(
                    interview.QuestionnaireId, interview.QuestionnaireVersion);

                return questionnaire.Children[0].PublicKey;
            }

            return null;
        }

        private static bool IsEntityInFilter(InterviewDetailsFilter? filter, InterviewEntityView entity)
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