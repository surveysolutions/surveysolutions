using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class QuestionnaireQuestionOptionsRepository : IQuestionOptionsRepository
    {
        public IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity qestionnaireIdentity, ICategoricalOptionsProvider categoricalOptionsProvider, Guid questionId, int? parentQuestionValue, string filter, Guid? translationId)
        {
            return categoricalOptionsProvider.GetOptionsForQuestionFromStructure(questionId, parentQuestionValue, filter, translationId);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(QuestionnaireIdentity qestionnaireIdentity, ICategoricalOptionsProvider categoricalOptionsProvider, Guid questionId, string optionText, Guid? translationId)
        {
            return categoricalOptionsProvider.GetOptionForQuestionFromStructureByOptionText(questionId, optionText, translationId);
        }
    }
}
