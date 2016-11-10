using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class QuestionOptionsRepository : IQuestionOptionsRepository
    {
        public QuestionOptionsRepository(IOptionsRepository optionsRepository)
        {
            this.optionsRepository = optionsRepository;
        }

        public readonly IOptionsRepository optionsRepository;

        public IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity qestionnaireIdentity,
            Guid questionId, int? parentQuestionValue, string filter, Translation translation)
        {
            return optionsRepository.GetFilteredQuestionOptions(qestionnaireIdentity, questionId, parentQuestionValue, filter, translation.Id);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(QuestionnaireIdentity qestionnaireIdentity,
             Guid questionId, string optionText, Translation translation)
        {
            return optionsRepository.GetQuestionOption(qestionnaireIdentity, questionId, optionText, translation.Id);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(QuestionnaireIdentity qestionnaireIdentity,
            Guid questionId, decimal optionValue, Translation translation)
        {
            return optionsRepository.GetQuestionOptionByValue(qestionnaireIdentity, questionId, optionValue, translation.Id);
        }
    }
}
