using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
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
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CoverInterviewViewModel : MvxViewModel, IDisposable
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly IPrincipal principal;
        private readonly IEntitiesListViewModelFactory entitiesListViewModelFactory;
        private readonly IDynamicTextViewModelFactory dynamicTextViewModelFactory;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly ICompositeCollectionInflationService compositeCollectionInflationService;

        public CoverStateViewModel InterviewState { get; set; }
        public DynamicTextViewModel Name { get; }

        public CoverInterviewViewModel(
            IPrincipal principal,
            CoverStateViewModel interviewState,
            DynamicTextViewModel dynamicTextViewModel, 
            IQuestionnaireStorage questionnaireRepository, 
            IStatefulInterviewRepository interviewRepository, 
            IEntitiesListViewModelFactory entitiesListViewModelFactory, 
            IDynamicTextViewModelFactory dynamicTextViewModelFactory,
            IInterviewViewModelFactory interviewViewModelFactory,
            ICompositeCollectionInflationService compositeCollectionInflationService,
            GroupNavigationViewModel nextGroupNavigationViewModel)
        {
            this.principal = principal;

            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.entitiesListViewModelFactory = entitiesListViewModelFactory;
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.compositeCollectionInflationService = compositeCollectionInflationService;
            this.NextGroupNavigationViewModel = nextGroupNavigationViewModel;
        }

        public string InterviewKey { get; set; }

        public string AssignmentId { get; set; }

        public string QuestionnaireTitle { get; set; }

        public string SupervisorNote { get; set; }

        public bool HasPrefilledEntities { get; set; }

        public bool IsEditMode { get; set; }
        
        public IReadOnlyCollection<CoverPrefilledEntity> PrefilledReadOnlyEntities { get; set; }

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

        public GroupNavigationViewModel NextGroupNavigationViewModel { get; }

        protected Guid interviewId;
        protected NavigationState navigationState;

        public virtual void Configure(string interviewId, NavigationState navigationState, Identity anchoredElementIdentity)
        {
            this.navigationState = navigationState;
            this.interviewId = Guid.Parse(interviewId);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            if (questionnaire.IsCoverPageSupported)
                this.Name.Init(interviewId, new Identity(questionnaire.CoverPageSectionId, RosterVector.Empty));
            else
                this.Name.InitAsStatic(UIResources.Interview_Cover_Screen_Title);
            
            this.InterviewState.Init(interviewId, null);
            this.QuestionnaireTitle = questionnaire.Title;
            
            var prefilledEntitiesFromQuestionnaire = questionnaire.GetPrefilledEntities();
            IsEditMode = interview.HasEditableIdentifyingQuestions;

            if (IsEditMode)
            {
                this.PrefilledReadOnlyEntities = Array.Empty<CoverPrefilledEntity>();
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
            NextGroupNavigationViewModel.Init(this.interviewId.FormatGuid(), null, this.navigationState);
            this.SetScrollTo(anchoredElementIdentity);
        }
        
        private void SetScrollTo(Identity scrollTo)
        {
            if (scrollTo != null)
            {
                ScrollToIdentity = scrollTo;
            }
        }

        public Identity ScrollToIdentity { get; set; }

        private CompositeCollection<ICompositeEntity> GetEditablePrefilledData(string interviewId, NavigationState navigationState)
        {
            prefilledEntities = this.interviewViewModelFactory.GetPrefilledEntities(interviewId, navigationState).ToList();
            return this.compositeCollectionInflationService.GetInflatedCompositeCollection(prefilledEntities);
        }
        
        private List<CoverPrefilledEntity> GetReadOnlyPrefilledData(string interviewId, NavigationState navigationState, ReadOnlyCollection<Guid> prefilledEntitiesFromQuestionnaire, IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            return prefilledEntitiesFromQuestionnaire
                .Select(entityId => new
                {
                    EntityId = entityId,
                    IsStaticText = questionnaire.IsStaticText(entityId),
                    IsVariable = questionnaire.IsVariable(entityId),
                    QuestionType = questionnaire.IsQuestion(entityId) 
                        ? questionnaire.GetQuestionType(entityId)
                        : (QuestionType?)null,
                })
                .Where(entity => entity.IsStaticText || 
                                 entity.IsVariable ||
                                 entity.QuestionType.HasValue)
                .Select(entity =>
                {
                    var entityIdentity = new Identity(entity.EntityId, RosterVector.Empty);
                    AttachmentViewModel attachmentViewModel = null;
                    if (entity.IsStaticText)
                    {
                        attachmentViewModel = this.interviewViewModelFactory.GetNew<AttachmentViewModel>();
                        attachmentViewModel.Init(interviewId, entityIdentity, navigationState);
                    }


                    var title = this.CreatePrefilledTitle(questionnaire, interviewId, entityIdentity);
                    var value = entity.QuestionType.HasValue
                        ? interview.GetAnswerAsString(Identity.Create(entity.EntityId, RosterVector.Empty), CultureInfo.CurrentCulture)
                        : entity.IsVariable
                            ? interview.GetVariableValueAsString(Identity.Create(entity.EntityId, RosterVector.Empty))
                            : string.Empty;

                    GpsLocation gpsLocation = null;
                    if (entity.QuestionType == QuestionType.GpsCoordinates)
                    {
                        var gpsQuestion = interview.GetGpsQuestion(Identity.Create(entity.EntityId, RosterVector.Empty));
                        var gpsQuestionAnswer = gpsQuestion.GetAnswer();

                        if (gpsQuestionAnswer != null)
                        {
                            var gpsAnswer = gpsQuestionAnswer.Value;
                            gpsLocation = new GpsLocation(gpsAnswer.Accuracy, gpsAnswer.Altitude, gpsAnswer.Latitude,
                                gpsAnswer.Longitude, DateTimeOffset.MinValue);
                            value = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", gpsAnswer.Latitude, gpsAnswer.Longitude); 
                        }
                    }
                    
                    return new CoverPrefilledEntity
                    {
                        Identity = entityIdentity,
                        Title = title,
                        Answer = value,
                        GpsLocation = gpsLocation,
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

        private bool isDisposed = false;
        private List<IInterviewEntityViewModel> prefilledEntities;

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            if (PrefilledReadOnlyEntities != null)
            {
                var prefilledQuestionsLocal = PrefilledReadOnlyEntities.ToArray();
                prefilledQuestionsLocal.ForEach(viewModel => viewModel?.Dispose());
            }

            if (prefilledEntities != null)
            {
                var prefilledEntity = prefilledEntities.ToArray();
                prefilledEntity.ForEach(viewModel => viewModel?.DisposeIfDisposable());
            }         
            
            if (PrefilledEditableEntities != null)
            {
                var prefilledEditable = PrefilledEditableEntities.ToArray();
                prefilledEditable.ForEach(viewModel => viewModel?.DisposeIfDisposable());
            }

            if (CommentedEntities != null)
            {
                var commentedEntities = CommentedEntities.ToArray();
                commentedEntities.ForEach(viewModel => viewModel?.DisposeIfDisposable());
            }

            NextGroupNavigationViewModel?.Dispose();
            
            Name?.Dispose();
        }
    }
}
