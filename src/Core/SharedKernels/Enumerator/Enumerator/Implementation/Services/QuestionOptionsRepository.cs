using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class QuestionOptionsRepository : IQuestionOptionsRepository
    {
        public QuestionOptionsRepository(IOptionsRepository optionsRepository)
        {
            this.optionsRepository = optionsRepository;
        }

        public readonly IOptionsRepository optionsRepository;

        public IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity qestionnaireIdentity, ICategoricalOptionsProvider categoricalOptionsProvider, Guid questionId, int? parentQuestionValue, string filter)
        {
            return optionsRepository.GetFilteredQuestionOptions(qestionnaireIdentity, questionId, parentQuestionValue, filter);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(QuestionnaireIdentity qestionnaireIdentity, ICategoricalOptionsProvider categoricalOptionsProvider, Guid questionId, string optionText)
        {
            return optionsRepository.GetQuestionOption(qestionnaireIdentity, questionId, optionText);
        }
    }
}
