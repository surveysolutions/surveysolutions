using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using MvvmCross;
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
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

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
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly ICompositeCollectionInflationService compositeCollectionInflationService;

        public CoverStateViewModel InterviewState { get; set; }
        public DynamicTextViewModel Name { get; }

        public CoverInterviewViewModel(ICommandService commandService,
            IPrincipal principal,
            CoverStateViewModel interviewState,
            DynamicTextViewModel dynamicTextViewModel, 
            IQuestionnaireStorage questionnaireRepository, 
            IStatefulInterviewRepository interviewRepository, 
            IEntitiesListViewModelFactory entitiesListViewModelFactory, 
            IDynamicTextViewModelFactory dynamicTextViewModelFactory,
            IInterviewViewModelFactory interviewViewModelFactory,
            ICompositeCollectionInflationService compositeCollectionInflationService)
        {
            this.commandService = commandService;
            this.principal = principal;

            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.entitiesListViewModelFactory = entitiesListViewModelFactory;
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.compositeCollectionInflationService = compositeCollectionInflationService;
        }

        public string InterviewKey { get; set; }

        public string AssignmentId { get; set; }

        public string QuestionnaireTitle { get; set; }

        public string SupervisorNote { get; set; }

        public bool HasPrefilledEntities { get; set; }

        public bool IsEditMode { get; set; }
        
        public IEnumerable<CoverPrefilledEntity> PrefilledReadOnlyEntities { get; set; }

        private CompositeCollection<ICompositeEntity> prefilledEditableEntities;
        public CompositeCollection<ICompositeEntity> PrefilledEditableEntities
        {
            get => this.prefilledEditableEntities;
            set { this.prefilledEditableEntities = value; this.RaisePropertyChanged(); }
        }

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

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            
            var pageTitle = questionnaire.IsCoverPageSupported
                ? questionnaire.GetGroupTitle(QuestionnaireDocument.CoverPageSectionId)
                : UIResources.Interview_Cover_Screen_Title;
            this.InterviewState.Init(interviewId, null);
            this.Name.InitAsStatic(pageTitle);

            var firstSectionId = questionnaire.GetAllSections().First(id => !questionnaire.IsCoverPage(id));
            this.firstSectionIdentity = new Identity(firstSectionId, RosterVector.Empty);
            this.QuestionnaireTitle = questionnaire.Title;
            
            var prefilledEntitiesFromQuestionnaire = questionnaire.GetPrefilledEntities();
            IsEditMode = interview.HasEditableIdentifyingQuestions;

            if (IsEditMode)
            {
                this.PrefilledReadOnlyEntities = new CoverPrefilledEntity[0];
                this.PrefilledEditableEntities = GetEditablePrefilledData(interviewId, navigationState);
            }
            else
            {
                this.PrefilledReadOnlyEntities = GetReadOnlyPrefilledData(interviewId, navigationState, prefilledEntitiesFromQuestionnaire, questionnaire, interview);
                this.PrefilledEditableEntities = new CompositeCollection<ICompositeEntity>();
            }

            this.HasPrefilledEntities = this.PrefilledReadOnlyEntities.Any() || this.PrefilledEditableEntities.Any();

            var interviewKey = interview.GetInterviewKey()?.ToString();
            this.InterviewKey = string.IsNullOrEmpty(interviewKey) ? null : string.Format(UIResources.InterviewKey, interviewKey);
            
            var assignmentId = interview.GetAssignmentId();
            this.AssignmentId = !assignmentId.HasValue ? null : string.Format(UIResources.AssignmentN, assignmentId);

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

        private CompositeCollection<ICompositeEntity> GetEditablePrefilledData(string interviewId, NavigationState navigationState)
        {
            var prefilledEntities = this.interviewViewModelFactory.GetPrefilledEntities(interviewId, navigationState).ToList();
            return this.compositeCollectionInflationService.GetInflatedCompositeCollection(prefilledEntities);
        }
        
        private List<CoverPrefilledEntity> GetReadOnlyPrefilledData(string interviewId, NavigationState navigationState, ReadOnlyCollection<Guid> prefilledEntitiesFromQuestionnaire, IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            return prefilledEntitiesFromQuestionnaire
                .Select(entityId => new
                {
                    EntityId = entityId,
                    IsStaticText = questionnaire.IsStaticText(entityId),
                    QuestionType = questionnaire.IsQuestion(entityId) 
                        ? questionnaire.GetQuestionType(entityId)
                        : (QuestionType?)null,
                })
                .Where(entity => entity.IsStaticText || 
                                 (entity.QuestionType.HasValue
                                  && entity.QuestionType.Value != QuestionType.GpsCoordinates))
                .Select(entity =>
                {
                    var entityIdentity = new Identity(entity.EntityId, RosterVector.Empty);
                    var attachmentViewModel = this.interviewViewModelFactory.GetNew<AttachmentViewModel>();
                    attachmentViewModel.Init(interviewId, entityIdentity, navigationState);

                    return new CoverPrefilledEntity
                    {
                        Title = this.CreatePrefilledTitle(questionnaire, interviewId, entityIdentity),
                        Answer = entity.QuestionType.HasValue
                            ? interview.GetAnswerAsString(Identity.Create(entity.EntityId, RosterVector.Empty), CultureInfo.CurrentCulture)
                            : string.Empty,
                        Attachment = attachmentViewModel
                    };
                })
                .ToList();
        }

        private DynamicTextViewModel CreatePrefilledTitle(IQuestionnaire questionnaire, string interviewId, Identity entityIdentity)
        {
            var title = this.dynamicTextViewModelFactory.CreateDynamicTextViewModel();
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
            var prefilledQuestionsLocal = PrefilledReadOnlyEntities;
            foreach (var prefilledQuestion in prefilledQuestionsLocal)
            {
                prefilledQuestion.Dispose();
            }

            Name?.Dispose();
        }
    }
}
