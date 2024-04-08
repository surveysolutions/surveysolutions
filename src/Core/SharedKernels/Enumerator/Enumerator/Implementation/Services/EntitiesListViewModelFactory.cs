using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class EntitiesListViewModelFactory : IEntitiesListViewModelFactory
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IDynamicTextViewModelFactory dynamicTextViewModelFactory;

        private readonly int maxNumberOfEntities = 30;

        public EntitiesListViewModelFactory(
            IStatefulInterviewRepository interviewRepository, 
            IInterviewViewModelFactory interviewViewModelFactory, 
            IDynamicTextViewModelFactory dynamicTextViewModelFactory)
        {
            this.interviewRepository = interviewRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
        }

        public IEnumerable<EntityWithErrorsViewModel> GetEntitiesWithErrors(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            Identity[] invalidEntities = interview.GetInvalidEntitiesInInterview().Take(this.maxNumberOfEntities).ToArray();
           
            return this.EntityWithErrorsViewModels<EntityWithErrorsViewModel>(interviewId, navigationState, invalidEntities, interview);
        }

        public IEnumerable<EntityWithCommentsViewModel> GetEntitiesWithComments(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            Identity[] commentedBySupervisorEntities = interview.GetCommentedBySupervisorQuestionsVisibleToInterviewer().Take(this.maxNumberOfEntities).ToArray();

            return this.EntityWithErrorsViewModels<EntityWithCommentsViewModel>(interviewId, navigationState, commentedBySupervisorEntities, interview);
        }

        public IEnumerable<EntityWithErrorsViewModel> GetUnansweredCriticalQuestions(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.GetOrThrow(interviewId);
            Identity[] invalidEntities = interview.GetAllUnansweredCriticalQuestions().Take(this.maxNumberOfEntities).ToArray();
           
            return this.EntityWithErrorsViewModels<EntityWithErrorsViewModel>(interviewId, navigationState, invalidEntities, interview);
        }

        public IEnumerable<FailCriticalityConditionViewModel> RunAndGetFailCriticalityConditions(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.GetOrThrow(interviewId);
            Guid[] ids = interview.RunAndGetFailCriticalityConditions().Take(this.maxNumberOfEntities).ToArray();
           
            var criticalityConditions = new List<FailCriticalityConditionViewModel>();
            foreach (var id in ids)
            {
                var criticalityConditionMessage = interview.GetCriticalityConditionMessage(id);
                var failCriticalityConditionViewModel = this.interviewViewModelFactory.GetNew<FailCriticalityConditionViewModel>();
                
                using (var title = this.dynamicTextViewModelFactory.CreateDynamicTextViewModel())
                {
                    title.InitAsStatic(criticalityConditionMessage);
                    failCriticalityConditionViewModel.Init(title.PlainText);
                }

                criticalityConditions.Add(failCriticalityConditionViewModel);
            }
            
            return criticalityConditions;
        }

        private IEnumerable<T> EntityWithErrorsViewModels<T>(string interviewId, NavigationState navigationState,
            Identity[] invalidEntities, IStatefulInterview interview) where T : ListEntityViewModel
        {
            var entitiesWithErrors = new List<T>();
            foreach (var invalidEntity in invalidEntities)
            {
                var entityWithErrorsViewModel = this.interviewViewModelFactory.GetNew<T>();

                var navigationIdentity = interview.IsQuestionPrefilled(invalidEntity)
                    ? NavigationIdentity.CreateForPrefieldScreen()
                    : NavigationIdentity.CreateForGroup(interview.GetParentGroup(invalidEntity), invalidEntity);

                //remove registration from bus
                using (var title = this.dynamicTextViewModelFactory.CreateDynamicTextViewModel())
                {
                    title.Init(interviewId, invalidEntity);
                    entityWithErrorsViewModel.Init(navigationIdentity, title.PlainText, navigationState);
                }

                entitiesWithErrors.Add(entityWithErrorsViewModel);
            }
            return entitiesWithErrors;
        }

        public int MaxNumberOfEntities => this.maxNumberOfEntities;
    }
}
