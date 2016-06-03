using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class EntityWithErrorsViewModelFactory : IEntityWithErrorsViewModelFactory
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly SubstitutionViewModel substitutionViewModel;
        private readonly int maxCountOfErrors = 50;

        public EntityWithErrorsViewModelFactory(
            IStatefulInterviewRepository interviewRepository, 
            IPlainQuestionnaireRepository questionnaireRepository, 
            SubstitutionViewModel substitutionViewModel)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.substitutionViewModel = substitutionViewModel;
        }

        public IEnumerable<EntityWithErrorsViewModel> GetEntities(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            var questionnaire = questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);
            Identity[] invalidEntities = interview.GetInvalidEntitiesInInterview().Take(maxCountOfErrors).ToArray();
           
            var entitiesWithErrors = new List<EntityWithErrorsViewModel>();
            foreach (var invalidEntity in invalidEntities)
            {
                var entityWithErrorsViewModel = new EntityWithErrorsViewModel();

                var errorTitle = questionnaire.HasQuestion(invalidEntity.Id)
                    ? questionnaire.GetQuestionTitle(invalidEntity.Id)
                    : questionnaire.GetStaticText(invalidEntity.Id);

                this.substitutionViewModel.Init(interviewId, invalidEntity, errorTitle);
                entityWithErrorsViewModel.Init(NavigationIdentity.CreateForGroup(interview.GetParentGroup(invalidEntity), invalidEntity),
                    this.substitutionViewModel.ReplaceSubstitutions(), navigationState);
                entitiesWithErrors.Add(entityWithErrorsViewModel);
            }
            return entitiesWithErrors;
        }

    }
}