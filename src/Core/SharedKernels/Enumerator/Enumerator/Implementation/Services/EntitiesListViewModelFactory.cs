using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class EntitiesListViewModelFactory : IEntitiesListViewModelFactory
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IDynamicTextViewModelFactory dynamicTextViewModelFactory;

        private readonly int maxNumberOfEntities = 30;

        public EntitiesListViewModelFactory(
            IStatefulInterviewRepository interviewRepository, 
            IQuestionnaireStorage questionnaireRepository, 
            IInterviewViewModelFactory interviewViewModelFactory, 
            IDynamicTextViewModelFactory dynamicTextViewModelFactory)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
        }

        public IEnumerable<EntityWithErrorsViewModel> GetEntitiesWithErrors(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            Identity[] invalidEntities = interview.GetInvalidEntitiesInInterview().Take(this.maxNumberOfEntities).ToArray();
           
            return this.EntityWithErrorsViewModels<EntityWithErrorsViewModel>(interviewId, navigationState, invalidEntities, interview, questionnaire);
        }

        public IEnumerable<EntityWithCommentsViewModel> GetEntitiesWithComments(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            Identity[] invalidEntities = interview.GetCommentedQuestionsInInterview().Take(this.maxNumberOfEntities).ToArray();

            return this.EntityWithErrorsViewModels<EntityWithCommentsViewModel>(interviewId, navigationState, invalidEntities, interview, questionnaire);
        }

        private IEnumerable<T> EntityWithErrorsViewModels<T>(string interviewId, NavigationState navigationState,
            Identity[] invalidEntities, IStatefulInterview interview, IQuestionnaire questionnaire) where T : ListEntityViewModel
        {
            var entitiesWithErrors = new List<T>();
            foreach (var invalidEntity in invalidEntities)
            {
                var entityWithErrorsViewModel = this.interviewViewModelFactory.GetNew<T>();

                var navigationIdentity = NavigationIdentity.CreateForGroup(interview.GetParentGroup(invalidEntity),
                    invalidEntity);

                var errorTitle = questionnaire.HasQuestion(invalidEntity.Id)
                    ? questionnaire.GetQuestionTitle(invalidEntity.Id)
                    : questionnaire.GetStaticText(invalidEntity.Id);

                var title = this.dynamicTextViewModelFactory.CreateDynamicTextViewModel();

                title.Init(interviewId, navigationIdentity.AnchoredElementIdentity, errorTitle);

                entityWithErrorsViewModel.Init(navigationIdentity, title.PlainText, navigationState);
                entitiesWithErrors.Add(entityWithErrorsViewModel);
            }
            return entitiesWithErrors;
        }

        public int MaxNumberOfEntities => this.maxNumberOfEntities;
    }
}