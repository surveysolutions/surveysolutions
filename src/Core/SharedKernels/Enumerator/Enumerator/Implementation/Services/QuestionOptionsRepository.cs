using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
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

        public IEnumerable<CategoricalOption> GetOptionsForQuestion(IQuestionnaire questionnaire, Guid questionId, int? parentQuestionValue, string filter)
        {
            return optionsRepository.GetFilteredQuestionOptions(new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), questionId, parentQuestionValue, filter);
        }
    }
}
