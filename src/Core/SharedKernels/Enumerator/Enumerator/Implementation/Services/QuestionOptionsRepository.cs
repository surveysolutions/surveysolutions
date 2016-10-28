using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
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

        public IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity qestionnaireIdentity,
            IQuestion question, int? parentQuestionValue, string filter, Guid? translationId)
        {
            return optionsRepository.GetFilteredQuestionOptions(qestionnaireIdentity, question.PublicKey, parentQuestionValue, filter, translationId);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(QuestionnaireIdentity qestionnaireIdentity,
             IQuestion question, string optionText, Guid? translationId)
        {
            return optionsRepository.GetQuestionOption(qestionnaireIdentity, question.PublicKey, optionText, translationId);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(QuestionnaireIdentity qestionnaireIdentity,
            IQuestion question, decimal optionValue, Guid? translationId)
        {
            return optionsRepository.GetQuestionOptionByValue(qestionnaireIdentity, question.PublicKey, optionValue, translationId);
        }
    }
}
