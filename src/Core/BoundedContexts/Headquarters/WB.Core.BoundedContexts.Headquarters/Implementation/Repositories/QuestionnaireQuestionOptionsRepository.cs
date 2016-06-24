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
        public IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity qestionnaireIdentity, ICategoricalOptionsProvider categoricalOptionsProvider, Guid questionId, int? parentQuestionValue, string filter)
        {
            return categoricalOptionsProvider.GetOptionsForQuestionFromStructure(questionId, parentQuestionValue, filter);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(QuestionnaireIdentity qestionnaireIdentity, ICategoricalOptionsProvider categoricalOptionsProvider, Guid questionId, string optionText)
        {
            return categoricalOptionsProvider.GetOptionForQuestionFromStructureByOptionText(questionId, optionText);
        }
    }
}
