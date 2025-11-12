using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
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

        public EntitiesListViewModelFactoryResult GetTopEntitiesWithErrors(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            var total = interview.GetInvalidEntitiesInInterview().Count();
            Identity[] invalidEntities = interview.GetInvalidEntitiesInInterview().Take(this.maxNumberOfEntities).ToArray();
            return new EntitiesListViewModelFactoryResult(
                this.EntityWithErrorsViewModels<EntityWithErrorsViewModel>(interviewId, navigationState, invalidEntities, interview),
                total
                );
        }

        public EntitiesListViewModelFactoryResult GetTopUnansweredQuestions(string interviewId, NavigationState navigationState, bool forSupervisor)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            var total = interview.GetAllUnansweredQuestions(forSupervisor).Count();
            Identity[] invalidEntities = interview.GetAllUnansweredQuestions(forSupervisor).Take(this.maxNumberOfEntities).ToArray();
            return new EntitiesListViewModelFactoryResult(
                this.EntityWithErrorsViewModels<EntityWithErrorsViewModel>(interviewId, navigationState, invalidEntities, interview, false),
                total
                );
        }

        public EntitiesListViewModelFactoryResult GetTopEntitiesWithComments(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            var total = interview.GetCommentedBySupervisorQuestionsVisibleToInterviewer().Count();
            Identity[] commentedBySupervisorEntities = interview.GetCommentedBySupervisorQuestionsVisibleToInterviewer().Take(this.maxNumberOfEntities).ToArray();
            return new EntitiesListViewModelFactoryResult(
                this.EntityWithErrorsViewModels<EntityWithErrorsViewModel>(interviewId, navigationState, commentedBySupervisorEntities, interview),
                total
                );
        }

        public EntitiesListViewModelFactoryResult GetTopUnansweredCriticalQuestions(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.GetOrThrow(interviewId);
            var total = interview.GetAllUnansweredCriticalQuestions().Count();
            Identity[] invalidEntities = interview.GetAllUnansweredCriticalQuestions().Take(this.maxNumberOfEntities).ToArray();
            return new EntitiesListViewModelFactoryResult(
                this.EntityWithErrorsViewModels<EntityWithErrorsViewModel>(interviewId, navigationState, invalidEntities, interview),
                total
                );
        }
        
        private IEnumerable<EntityWithErrorsViewModel> GetFailedCriticalRules(IStatefulInterview interview, NavigationState navigationState, Guid[] ids)
        {
            var interviewId = interview.Id.FormatGuid();
            var criticalRules = new List<EntityWithErrorsViewModel>();
            
            foreach (var ruleId in ids)
            {
                var criticalRuleMessage = interview.GetCriticalRuleMessage(ruleId);

                var entityWithErrorsViewModel = this.interviewViewModelFactory.GetNew<EntityWithErrorsViewModel>();

                using (var title = this.dynamicTextViewModelFactory.CreateDynamicTextViewModel())
                {
                    title.InitAsStatic(criticalRuleMessage);
                    entityWithErrorsViewModel.Init(null, null, null, title.PlainText, navigationState);
                    entityWithErrorsViewModel.IsError = true;
                    entityWithErrorsViewModel.AllowInnerLinks(interviewId, Identity.Create(interview.Id, RosterVector.Empty));
                }

                criticalRules.Add(entityWithErrorsViewModel);
            }
            return criticalRules;
        }
        
        public EntitiesListViewModelFactoryResult GetTopFailedCriticalRules(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.GetOrThrow(interviewId);
            var total = interview.CollectInvalidCriticalRules().Count();
            Guid[] ids = interview.CollectInvalidCriticalRules().Take(this.maxNumberOfEntities).ToArray();

            return new EntitiesListViewModelFactoryResult(
                GetFailedCriticalRules(interview, navigationState, ids),
                total);
        }

        public EntitiesListViewModelFactoryResult GetTopFailedCriticalRulesFromState(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.GetOrThrow(interviewId);
            var total = interview.GetFailedCriticalRules().Count();
            Guid[] ids = interview.GetFailedCriticalRules().Take(this.maxNumberOfEntities).ToArray();

            return new EntitiesListViewModelFactoryResult(
                GetFailedCriticalRules(interview, navigationState, ids),
                total);
        }

        private IEnumerable<T> EntityWithErrorsViewModels<T>(string interviewId, NavigationState navigationState,
            Identity[] invalidEntities, IStatefulInterview interview, bool isError = true) where T : EntityWithErrorsViewModel
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

                    var question = interview.GetQuestion(invalidEntity);
                    var comment = question?.AnswerComments.LastOrDefault()?.Comment;
                    var error = interview.GetFailedValidationMessages(invalidEntity, null).FirstOrDefault();
                    entityWithErrorsViewModel.Init(navigationIdentity, title.PlainText, comment, error, navigationState);
                    entityWithErrorsViewModel.IsError = isError;
                }

                entitiesWithErrors.Add(entityWithErrorsViewModel);
            }
            return entitiesWithErrors;
        }

        public int MaxNumberOfEntities => this.maxNumberOfEntities;
    }
}
