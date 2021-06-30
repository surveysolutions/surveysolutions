using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class QuestionOptionsRepository : IQuestionOptionsRepository
    {
        private readonly IServiceLocator serviceLocator;
        private IOptionsRepository optionsRepository => serviceLocator.GetInstance<IOptionsRepository>();

        public QuestionOptionsRepository(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator ?? throw new ArgumentException(nameof(optionsRepository));
        }

        public IEnumerable<CategoricalOption> GetOptionsForQuestion(IQuestionnaire questionnaire,
            Guid questionId, int? parentQuestionValue, string filter, Translation translation,
            int[] excludedOptionIds = null)
        {
            var categoryId = questionnaire.GetReusableCategoriesForQuestion(questionId);
            if (categoryId.HasValue)
            {
                return this.optionsRepository.GetFilteredCategoriesOptions(
                    new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), categoryId.Value,
                    parentQuestionValue, filter, translation?.Id, excludedOptionIds);
            }    
            
            return this.optionsRepository.GetFilteredQuestionOptions(
                new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), questionId, 
                parentQuestionValue, filter, translation?.Id, excludedOptionIds);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(IQuestionnaire questionnaire, Guid questionId, string optionText, int? parentQuestionValue, Translation translation)
        {
            var categoryId = questionnaire.GetReusableCategoriesForQuestion(questionId);
            if (categoryId.HasValue)
                return this.optionsRepository.GetCategoryOption(new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), categoryId.Value, optionText, parentQuestionValue, translation?.Id);
                
            return this.optionsRepository.GetQuestionOption(new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), questionId, optionText, parentQuestionValue, translation?.Id);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(IQuestionnaire questionnaire,
            Guid questionId, decimal optionValue, int? parentQuestionValue, Translation translation)
        {
            var categoryId = questionnaire.GetReusableCategoriesForQuestion(questionId);
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version);

            return categoryId.HasValue
                ? this.optionsRepository.GetCategoryOptionByValue(questionnaireIdentity, categoryId.Value, optionValue, parentQuestionValue, translation?.Id)
                : this.optionsRepository.GetQuestionOptionByValue(questionnaireIdentity, questionId, optionValue, parentQuestionValue, translation?.Id);
        }

        public IEnumerable<CategoricalOption> GetOptionsByOptionValues(IQuestionnaire questionnaire, Guid questionId,
            int[] optionsValues, Translation translation)
        {
            var categoryId = questionnaire.GetReusableCategoriesForQuestion(questionId);
            if (categoryId.HasValue)
                return this.optionsRepository.GetCategoryOptionsByValues(
                    new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), categoryId.Value,
                    optionsValues, translation?.Id);

            return this.optionsRepository.GetOptionsByValues(
                new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), questionId,
                optionsValues, translation?.Id);
        }
    }
}
