using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CoverInterviewViewModel : MvxViewModel
    {
        private readonly ICommandService commandService;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAnswerToStringService answerToStringService;
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
            IAnswerToStringService answerToStringService, 
            IEntitiesListViewModelFactory entitiesListViewModelFactory, 
            IDynamicTextViewModelFactory dynamicTextViewModelFactory)
        {
            this.commandService = commandService;
            this.principal = principal;

            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.answerToStringService = answerToStringService;
            this.entitiesListViewModelFactory = entitiesListViewModelFactory;
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
        }

        public string QuestionnaireTitle { get; set; }

        public string SupervisorNote { get; set; }

        public IEnumerable<CoverPrefilledQuestion> PrefilledQuestions { get; set; }

        public bool HasPrefilledQuestions { get; set; }

        public IList<EntityWithCommentsViewModel> CommentedEntities { get; private set; }

        public string CommentedEntitiesDescription { get; set; }
        public int CountOfCommentedQuestions { get; set; }

        protected Guid interviewId;
        protected NavigationState navigationState;

        public virtual void Init(string interviewId, NavigationState navigationState)
        {
            this.navigationState = navigationState;
            this.interviewId = Guid.Parse(interviewId);

            this.InterviewState.Init(interviewId, null);
            this.Name.InitAsStatic(UIResources.Interview_Cover_Screen_Title);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.firstSectionIdentity = new Identity(questionnaire.GetAllSections().First(), RosterVector.Empty);
            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire
                .GetPrefilledQuestions()
                .Where(questionId => questionnaire.GetQuestionType(questionId) != QuestionType.GpsCoordinates)
                .Select(questionId => new CoverPrefilledQuestion
                {
                    Question = this.CreateQuestionTitle(interviewId, new Identity(questionId, RosterVector.Empty), questionnaire.GetQuestionTitle(questionId)),
                    Answer = this.GetAnswer(questionnaire, interview, questionId)
                })
                .ToList();
           
            this.HasPrefilledQuestions = this.PrefilledQuestions.Any();

            this.CountOfCommentedQuestions = interview.CountCommentedQuestions();
            this.CommentedEntities = entitiesListViewModelFactory.GetEntitiesWithComments(interviewId, navigationState).ToList();

            this.CommentedEntitiesDescription = CommentedEntities.Count == this.CountOfCommentedQuestions
                ? UIResources.Interview_Cover_Questions_With_Comments
                : string.Format(UIResources.Interview_Cover_First_n_Questions_With_Comments, entitiesListViewModelFactory.MaxNumberOfEntities);

            this.SupervisorNote = interview.GetLastSupervisorComment();
        }

        private DynamicTextViewModel CreateQuestionTitle(string interviewId, Identity entityIdentity, string textWithSubstitutions)
        {
            var title = this.dynamicTextViewModelFactory.CreateDynamicTextViewModel();

            title.Init(interviewId, entityIdentity, textWithSubstitutions);

            return title;
        }

        private Identity firstSectionIdentity;

        public IMvxAsyncCommand StartInterviewCommand => new MvxAsyncCommand(this.StartInterviewAsync);

        private async Task StartInterviewAsync()
        {
            await this.commandService.WaitPendingCommandsAsync();
            this.navigationState.NavigateTo(NavigationIdentity.CreateForGroup(firstSectionIdentity));
        }

        private string GetAnswer(IQuestionnaire questionnaire, IStatefulInterview interview, Guid referenceToQuestionId)
        {
            var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(referenceToQuestionId, RosterVector.Empty);
            var interviewAnswer = interview.Answers.ContainsKey(identityAsString) ? interview.Answers[identityAsString] : null;
            return this.answerToStringService.AnswerToUIString(referenceToQuestionId, interviewAnswer, interview, questionnaire);
        }
    }
}