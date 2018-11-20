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
        private readonly IOptionsRepository optionsRepository;

        public QuestionOptionsRepository(IOptionsRepository optionsRepository)
        {
            this.optionsRepository = optionsRepository ?? throw new ArgumentException(nameof(optionsRepository));
        }

        public IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity questionnaireIdentity,
            Guid questionId, int? parentQuestionValue, string filter, Translation translation)
        {
            return this.optionsRepository.GetFilteredQuestionOptions(questionnaireIdentity, questionId, parentQuestionValue, filter, translation?.Id);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(QuestionnaireIdentity qestionnaireIdentity,
            Guid questionId, decimal optionValue, Translation translation)
        {
            return this.optionsRepository.GetQuestionOptionByValue(qestionnaireIdentity, questionId, optionValue, translation?.Id);
        }
    }
}
