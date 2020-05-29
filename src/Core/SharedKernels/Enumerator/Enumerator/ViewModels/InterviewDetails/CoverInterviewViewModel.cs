using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CoverInterviewViewModel : MvxViewModel, IDisposable
    {
        private readonly ICommandService commandService;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly IPrincipal principal;
        private readonly IEntitiesListViewModelFactory entitiesListViewModelFactory;
        private readonly IDynamicTextViewModelFactory dynamicTextViewModelFactory;

        public CoverStateViewModel InterviewState { get; set; }
        public DynamicTextViewModel Name { get; }

        public CoverInterviewViewModel(ICommandService commandService,
            IPrincipal principal,
            CoverStateViewModel interviewState,
            DynamicTextViewModel dynamicTextViewModel, 
            IQuestionnaireStorage questionnaireRepository, 
            IStatefulInterviewRepository interviewRepository, 
            IEntitiesListViewModelFactory entitiesListViewModelFactory, 
            IDynamicTextViewModelFactory dynamicTextViewModelFactory)
        {
            this.commandService = commandService;
            this.principal = principal;

            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.entitiesListViewModelFactory = entitiesListViewModelFactory;
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
        }

        public string InterviewKey { get; set; }

        public string AssignmentId { get; set; }

        public string QuestionnaireTitle { get; set; }

        public string SupervisorNote { get; set; }

        public IEnumerable<CoverPrefilledEntity> PrefilledEntities { get; set; }

        public bool HasPrefilledEntities { get; set; }

        public IList<EntityWithCommentsViewModel> CommentedEntities { get; private set; }

        public bool DoesShowCommentsBlock { get; set; }
        public string CommentedEntitiesDescription { get; set; }
        public int CountOfCommentedQuestions { get; set; }

        protected Guid interviewId;
        protected NavigationState navigationState;

        public virtual void Configure(string interviewId, NavigationState navigationState)
        {
            this.navigationState = navigationState;
            this.interviewId = Guid.Parse(interviewId);

            this.InterviewState.Init(interviewId, null);
            this.Name.InitAsStatic(UIResources.Interview_Cover_Screen_Title);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.firstSectionIdentity = new Identity(questionnaire.GetAllSections().First(), RosterVector.Empty);
            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledEntities = questionnaire
                .GetPrefilledEntities()
                .Where(entityId => questionnaire.IsStaticText(entityId) || 
                                   (questionnaire.IsQuestion(entityId)
                                    && questionnaire.GetQuestionType(entityId) != QuestionType.GpsCoordinates))
                .Select(entityId => new CoverPrefilledEntity
                {
                    Title = this.CreatePrefilledTitle(questionnaire, interviewId, new Identity(entityId, RosterVector.Empty)),
                    Answer = interview.GetAnswerAsString(Identity.Create(entityId, RosterVector.Empty), CultureInfo.CurrentCulture)
                })
                .ToList();

            var interviewKey = interview.GetInterviewKey()?.ToString();
            this.InterviewKey = string.IsNullOrEmpty(interviewKey) ? null : string.Format(UIResources.InterviewKey, interviewKey);
            var assignmentId = interview.GetAssignmentId();
            this.AssignmentId = !assignmentId.HasValue ? null : string.Format(UIResources.AssignmentN, assignmentId);
           
            this.HasPrefilledEntities = this.PrefilledEntities.Any();

            this.CountOfCommentedQuestions = interview.GetCommentedBySupervisorQuestionsVisibleToInterviewer().Count();
            this.CommentedEntities = entitiesListViewModelFactory.GetEntitiesWithComments(interviewId, navigationState).ToList();

            this.CommentedEntitiesDescription = CountOfCommentedQuestions == 0
                ? UIResources.Interview_Cover_Supervisor_Comments_does_not_exists
                : CommentedEntities.Count == this.CountOfCommentedQuestions
                    ? UIResources.Interview_Cover_Questions_With_Comments
                    : string.Format(UIResources.Interview_Cover_First_n_Questions_With_Comments, entitiesListViewModelFactory.MaxNumberOfEntities);

            this.DoesShowCommentsBlock = CountOfCommentedQuestions > 0 || interview.WasCompleted || interview.WasRejected;

            this.SupervisorNote = interview.GetLastSupervisorComment();
        }

        private DynamicTextViewModel CreatePrefilledTitle(IQuestionnaire questionnaire, string interviewId, Identity entityIdentity)
        {
            var title = this.dynamicTextViewModelFactory.CreateDynamicTextViewModel();

            if (questionnaire.IsStaticText(entityIdentity.Id))
                title.InitAsStatic();
            title.Init(interviewId, entityIdentity);

            return title;
        }

        private Identity firstSectionIdentity;

        public IMvxAsyncCommand StartInterviewCommand => new MvxAsyncCommand(this.StartInterviewAsync);

        private async Task StartInterviewAsync()
        {
            await this.commandService.WaitPendingCommandsAsync();
            await this.navigationState.NavigateTo(NavigationIdentity.CreateForGroup(firstSectionIdentity));
        }

        public void Dispose()
        {
            var prefilledQuestionsLocal = PrefilledEntities;
            foreach (var prefilledQuestion in prefilledQuestionsLocal)
            {
                prefilledQuestion.Dispose();
            }

            Name?.Dispose();
        }
    }
}
