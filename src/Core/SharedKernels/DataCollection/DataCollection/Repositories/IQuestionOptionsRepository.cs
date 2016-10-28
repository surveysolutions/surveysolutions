using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IQuestionOptionsRepository
    {
        IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity qestionnaireIdentity,
            IQuestion questionId, int? parentQuestionValue, string filter, Guid? translationId);

        CategoricalOption GetOptionForQuestionByOptionText(QuestionnaireIdentity qestionnaireIdentity,
            IQuestion questionId, string optionText, Guid? translationId);

        CategoricalOption GetOptionForQuestionByOptionValue(QuestionnaireIdentity qestionnaireIdentity,
            IQuestion questionId, decimal optionValue, Guid? translationId);
    }
}
