﻿using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IQuestionOptionsRepository
    {
        IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity qestionnaireIdentity,
            Guid questionId, int? parentQuestionValue, string filter, Translation translationId);

        CategoricalOption GetOptionForQuestionByOptionText(QuestionnaireIdentity qestionnaireIdentity,
            Guid questionId, string optionText, Translation translationId);

        CategoricalOption GetOptionForQuestionByOptionValue(QuestionnaireIdentity qestionnaireIdentity,
            Guid questionId, decimal optionValue, Translation translationId);
    }
}
