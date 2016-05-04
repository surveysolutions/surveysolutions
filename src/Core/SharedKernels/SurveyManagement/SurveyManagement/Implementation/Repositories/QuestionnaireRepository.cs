using System;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Aggregates;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories
{
    internal class QuestionnaireRepository : IPlainAggregateRootRepository<Questionnaire>
    {
        private readonly IServiceLocator serviceLocator;

        public QuestionnaireRepository(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public Questionnaire Get(Guid aggregateId)
        {
            // questionnaire aggregate root has no state so we do not store it. when it will put correct implementation here

            var questionnaire = this.serviceLocator.GetInstance<Questionnaire>();

            questionnaire.SetId(aggregateId);

            return questionnaire;
        }

        public void Save(Questionnaire aggregateRoot)
        {
            // questionnaire aggregate root has no state so we do not store it. when it will put correct implementation here
        }
    }
}